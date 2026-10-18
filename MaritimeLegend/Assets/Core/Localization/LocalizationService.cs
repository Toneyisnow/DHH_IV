using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
namespace MaritimeLegend.Domain {
    [Serializable] public class LocaleTable { public string locale; public bool complete; public LocaleEntry[] entries; }
    [Serializable] public class LocaleEntry { public string key,text; }
    public sealed class LocalizationService {
        readonly Dictionary<string,LocaleTable> tables=new Dictionary<string,LocaleTable>();
        readonly LocalizationPlan plan;
        readonly Func<string,string> read;
        public LocalizationService(LocalizationPlan plan,Func<string,string> read=null) {
            this.plan=plan;this.read=read??(locale=>Resources.Load<TextAsset>("Localization/"+locale).text);
        }
        LocaleTable Table(string locale) {
            if(!tables.ContainsKey(locale)) {
                var table=JsonUtility.FromJson<LocaleTable>(read(locale));
                if(table==null||table.entries==null||table.entries.Select(e=>e.key).Distinct().Count()!=table.entries.Length)throw new InvalidOperationException("Invalid locale table "+locale);
                tables.Add(locale,table);
            }
            return tables[locale];
        }
        public string Text(string locale,string key,params object[] args) {
            if(!plan.locales.Contains(locale))locale=plan.fallbackLocale;
            var entry=Table(locale).entries.FirstOrDefault(e=>e.key==key)??Table(plan.fallbackLocale).entries.FirstOrDefault(e=>e.key==key);
            return entry==null?"["+key+"]":string.Format(CultureInfo.GetCultureInfo(locale),entry.text,args);
        }
    }
}
