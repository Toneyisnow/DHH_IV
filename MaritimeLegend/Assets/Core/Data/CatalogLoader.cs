using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MaritimeLegend.Domain {
[Serializable] public class DataManifest {
    public int version,maxFileBytes;
    public string[] content,balance,defaults,schemas;
}
[Serializable] public class ParameterConstraints { public string group; public FieldConstraint[] fields; }
[Serializable] public class FieldConstraint { public string name; public bool integer; public double minimum,maximum; }

// Resource I/O is injected for headless validation. No fallback catalog or default numbers.
public static class CatalogLoader {
    public static T Copy<T>(T value) { return JsonUtility.FromJson<T>(JsonUtility.ToJson(value)); }
    public static Catalog Load() { return Load(path=> {
        var asset=Resources.Load<TextAsset>("Data/"+path);
        if(asset==null)throw new InvalidOperationException("Missing game data: Data/"+path);
        return asset.text;
    }); }
    public static Catalog Load(Func<string,string> read,Func<Type,string,object> deserialize=null) {
        if(deserialize==null)deserialize=(type,json)=>JsonUtility.FromJson(json,type);
        var manifest=JsonUtility.FromJson<DataManifest>(read("manifest"));
        if(manifest==null||manifest.version<=0||manifest.maxFileBytes<=0||manifest.content==null||manifest.balance==null||manifest.defaults==null||manifest.schemas==null)
            throw new InvalidOperationException("Invalid data manifest");
        var paths=manifest.content.Concat(manifest.balance).Concat(manifest.defaults).Concat(manifest.schemas).ToArray();
        if(paths.Distinct().Count()!=paths.Length)throw new InvalidOperationException("Duplicate manifest path");
        Func<string,string> checkedRead=path=> {
            if(string.IsNullOrEmpty(path)||path.Contains("..")||path.StartsWith("/")||path.Contains(":"))throw new InvalidOperationException("Invalid resource path "+path);
            var text=read(path);
            if(string.IsNullOrWhiteSpace(text)||Encoding.UTF8.GetByteCount(text)>manifest.maxFileBytes)throw new InvalidOperationException("Missing/oversized JSON: "+path);
            return text;
        };
        var c=new Catalog {version=manifest.version,balance=new BalanceConfig(),initial=new InitialData()};
        foreach(var path in manifest.content){var text=checkedRead(path);Merge(c,deserialize(typeof(Catalog),text),text,true,path);}
        var rawBalance=new Dictionary<string,string>();
        foreach(var path in manifest.balance){var text=checkedRead(path);Merge(c.balance,deserialize(typeof(BalanceConfig),text),text,false,path);rawBalance.Add(path.Substring(path.LastIndexOf('/')+1),text);}
        foreach(var path in manifest.defaults){var text=checkedRead(path);Merge(c.initial,deserialize(typeof(InitialData),text),text,false,path);}
        RequireReferences(c.balance);RequireReferences(c.initial);
        var checkedGroups=new HashSet<string>();
        foreach(var path in manifest.schemas){
            var schema=JsonUtility.FromJson<ParameterConstraints>(checkedRead(path));
            if(schema==null||!checkedGroups.Add(schema.group))throw new InvalidOperationException("Invalid/duplicate schema "+path);
            var groupField=typeof(BalanceConfig).GetField(schema.group);
            if(groupField==null||!rawBalance.ContainsKey(schema.group))throw new InvalidOperationException("Unknown parameter group "+schema.group);
            var group=groupField.GetValue(c.balance);var fields=group.GetType().GetFields();
            if(schema.fields==null||schema.fields.Length!=fields.Length||schema.fields.Select(x=>x.name).Distinct().Count()!=fields.Length)throw new InvalidOperationException("Incomplete schema "+path);
            // Each balance file is one flat typed group. Detect missing/unknown/duplicate keys before JsonUtility can ignore them.
            var keys=Regex.Matches(rawBalance[schema.group],"\"([^\"]+)\"\\s*:").Cast<Match>().Select(m=>m.Groups[1].Value).ToArray();
            var expected=fields.Select(f=>f.Name).Concat(new[]{schema.group}).OrderBy(x=>x).ToArray();
            if(!keys.OrderBy(x=>x).SequenceEqual(expected))throw new InvalidOperationException("Missing/unknown/duplicate parameter in "+schema.group);
            foreach(var rule in schema.fields){
                var field=fields.FirstOrDefault(f=>f.Name==rule.name);
                if(field==null)throw new InvalidOperationException("Unknown parameter "+rule.name);
                var token=Regex.Match(rawBalance[schema.group],"\""+Regex.Escape(rule.name)+"\"\\s*:\\s*(-?(?:0|[1-9][0-9]*)(?:\\.[0-9]+)?(?:[eE][+-]?[0-9]+)?)\\s*[,}]");
                if(!token.Success)throw new InvalidOperationException("Non-numeric parameter "+rule.name);
                double number=double.Parse(token.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);
                if(double.IsNaN(number)||double.IsInfinity(number)||number<rule.minimum||number>rule.maximum||(rule.integer&&number!=Math.Truncate(number)))throw new InvalidOperationException("Out-of-range parameter "+schema.group+"."+rule.name);
                if(rule.integer!=(field.FieldType==typeof(int)))throw new InvalidOperationException("Schema type mismatch "+rule.name);
            }
        }
        if(checkedGroups.Count!=typeof(BalanceConfig).GetFields().Length)throw new InvalidOperationException("Missing balance schemas");
        if(c.productOrder==null||c.recipeOrder==null||c.productOrder.Length!=c.products.Length||c.recipeOrder.Length!=c.recipes.Length||c.productOrder.Distinct().Count()!=c.products.Length||c.recipeOrder.Distinct().Count()!=c.recipes.Length)throw new InvalidOperationException("Invalid catalog order tables");
        c.products=c.productOrder.Select(id=>c.products.Single(p=>p.id==id)).ToArray();
        c.recipes=c.recipeOrder.Select(id=>c.recipes.Single(r=>r.id==id)).ToArray();
        c.Validate();return c;
    }
    static void RequireReferences(object value){foreach(var f in value.GetType().GetFields())if(f.GetValue(value)==null)throw new InvalidOperationException("Missing configuration "+f.Name);}
    static void Merge(object target,object part,string json,bool append,string path){
        if(part==null)throw new InvalidOperationException("Invalid JSON "+path);
        // Unity may materialize absent inline classes/arrays. Presence comes from
        // the JSON document, never from the deserialized object's non-null fields.
        string name=SingleRootField(json,path);
        var field=target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.Public);
        if(field==null||field.FieldType.IsValueType||(target is Catalog&&(name==nameof(Catalog.balance)||name==nameof(Catalog.initial))))
            throw new InvalidOperationException("Unknown configuration category "+name+" in "+path);
        var value=field.GetValue(part);
        if(value==null)throw new InvalidOperationException("Null configuration "+name+" in "+path);
        var current=field.GetValue(target);
        if(current==null){field.SetValue(target,value);return;}
        if(!append||!field.FieldType.IsArray)throw new InvalidOperationException("Duplicate configuration "+name+" in "+path);
        var a=(Array)current;var b=(Array)value;
        var merged=Array.CreateInstance(field.FieldType.GetElementType(),a.Length+b.Length);
        Array.Copy(a,merged,a.Length);Array.Copy(b,0,merged,a.Length,b.Length);field.SetValue(target,merged);
    }
    static string SingleRootField(string json,string path){
        // Category names are schema identifiers. Scan their value with balanced
        // delimiters, honoring escaped quotes, so nested keys cannot look like roots.
        var root=Regex.Match(json,@"^\s*\{\s*""([A-Za-z_][A-Za-z_0-9]*)""\s*:\s*");
        if(!root.Success)throw new InvalidOperationException("Expected one category in "+path);
        int i=root.Length,start=i;bool quoted=false,escaped=false;var nesting=new Stack<char>();
        for(;i<json.Length;i++){
            char ch=json[i];
            if(quoted){if(escaped)escaped=false;else if(ch=='\\')escaped=true;else if(ch=='"')quoted=false;continue;}
            if(ch=='"'){quoted=true;continue;}
            if(ch=='{'||ch=='['){nesting.Push(ch);continue;}
            if(ch=='}'||ch==']'){
                if(nesting.Count==0)break;
                char open=nesting.Pop();
                if((open=='{'&&ch!='}')||(open=='['&&ch!=']'))throw new InvalidOperationException("Unbalanced JSON in "+path);
            }else if(ch==','&&nesting.Count==0)break;
        }
        string value=json.Substring(start,i-start).Trim();
        if(quoted||nesting.Count!=0||value.Length==0||value=="null"||i>=json.Length||json[i]!='}'||json.Substring(i+1).Trim().Length!=0)
            throw new InvalidOperationException("Each data file must contain exactly one non-null category: "+path);
        return root.Groups[1].Value;
    }
}
}
