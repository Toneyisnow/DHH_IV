using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaritimeLegend.Editor
{
    /// <summary>Deterministic, baked coastal art study. Regenerate from the menu; never modifies the original sailing scene.</summary>
    public static class ManualSeaV2Study
    {
        const string Root = "Assets/Resources";
        const string ScenePath = "Assets/Scenes/ManualSailingV2Test.unity";
        struct Island {public Vector3 center;public float scale;public int variant;public bool snow;}
        static List<Island> islands;
        static float N(float x,float z,float s)=>Mathf.PerlinNoise(x*s+173.7f,z*s+91.3f);
        static float LocalHeight(float x,float z,int variant){
            float radius=Mathf.Sqrt(x*x/(42*42f)+z*z/(32*32f));
            float edge=1-radius+(N(x+variant*37,z,.12f)-.5f)*.24f;
            float shore=edge*13;
            if(shore<.8f)return Mathf.Max(-32,shore);
            float ridges=Mathf.Pow(Mathf.Max(0,(N(x+variant*37,z,.055f)-.38f)*2.2f),2);
            return .8f+(shore-.8f)*.24f+ridges*Mathf.SmoothStep(0,1,edge*3)*22;
        }
        static float Height(float x,float z){float h=-32;foreach(var island in islands){float lx=(x-island.center.x)/island.scale,lz=(z-island.center.z)/island.scale;if(Mathf.Abs(lx)>95||Mathf.Abs(lz)>85)continue;h=Mathf.Max(h,LocalHeight(lx,lz,island.variant));}return h;}
        static Color Paint(float x,float z,float h,int variant,bool snow){
            float slope=new Vector2(LocalHeight(x+.5f,z,variant)-LocalHeight(x-.5f,z,variant),LocalHeight(x,z+.5f,variant)-LocalHeight(x,z-.5f,variant)).magnitude;
            Color grass=Color.Lerp(new Color(.25f,.38f,.12f),new Color(.53f,.58f,.24f),N(x,z,.24f));
            Color c=Color.Lerp(new Color(.86f,.74f,.48f),grass,Mathf.SmoothStep(0,1,(h-.4f)*1.7f));
            c=Color.Lerp(c,new Color(.66f,.58f,.37f),Mathf.Max(Mathf.SmoothStep(0,1,(slope-.6f)*.9f),Mathf.SmoothStep(0,1,(h-3)/6)));
            float inland=1-Mathf.Sqrt(x*x/(42*42f)+z*z/(32*32f));
            c=Color.Lerp(c,new Color(.48f,.36f,.18f),Mathf.SmoothStep(0,1,(h-15)/8)*Mathf.SmoothStep(0,1,(inland-.45f)/.25f));
            if(snow)c=Color.Lerp(c,Color.Lerp(new Color(.64f,.77f,.88f),new Color(.96f,.98f,1),N(x,z,.3f)),Mathf.SmoothStep(0,1,(h-6+N(x,z,.45f)*1.7f)/3));
            return c*Mathf.Lerp(.92f,1.07f,N(x,z,1.4f));
        }
        static T Save<T>(T asset,string name) where T:UnityEngine.Object
        {
            string category=asset is Mesh?"Models/Environment":asset is Texture2D?"Textures":"Materials";
            string folder=Root+"/"+category+"/ManualSailingV2";
            if(!Directory.Exists(folder)){Directory.CreateDirectory(folder);AssetDatabase.Refresh();}
            string path=folder+"/"+name;
            var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old) {
                if(old is Mesh target && asset is Mesh source){target.Clear();target.indexFormat=source.indexFormat;target.vertices=source.vertices;target.colors=source.colors;target.normals=source.normals;target.triangles=source.triangles;target.RecalculateBounds();}
                else if(old is Texture2D targetTexture && asset is Texture2D sourceTexture){targetTexture.Reinitialize(sourceTexture.width,sourceTexture.height,sourceTexture.format,false);targetTexture.SetPixels(sourceTexture.GetPixels());targetTexture.Apply();}
                else EditorUtility.CopySerialized(asset,old);
                EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(asset);return old;
            }
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        static GameObject MeshObject(string name, Mesh mesh, Material material)
        {
            var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=material;return obj;
        }
        sealed class Geometry
        {
            readonly List<Vector3> vertices=new List<Vector3>();
            readonly List<Color> colors=new List<Color>();
            readonly List<int> indices=new List<int>();
            public void Tri(Vector3 a,Vector3 b,Vector3 c,Color color)
            {int k=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);indices.Add(k);indices.Add(k+1);indices.Add(k+2);}
            public void Rock(Vector3 center,Vector3 scale,Color color,System.Random rng,int sides=7)
            {
                var rings=new Vector3[4,sides];
                for(int r=0;r<4;r++) for(int s=0;s<sides;s++) {
                    float angle=s*Mathf.PI*2/sides + r*.13f;
                    float radius=(r==0?.75f:r==1?1:r==2?.73f:.12f)*(.82f+(float)rng.NextDouble()*.32f);
                    rings[r,s]=center+Vector3.Scale(new Vector3(Mathf.Cos(angle)*radius,r/3f,Mathf.Sin(angle)*radius),scale);
                }
                for(int r=0;r<3;r++) for(int s=0;s<sides;s++) {int n=(s+1)%sides;var c=color*(.82f+(float)rng.NextDouble()*.30f);Tri(rings[r,s],rings[r+1,s],rings[r,n],c);Tri(rings[r,n],rings[r+1,s],rings[r+1,n],c);}
                for(int s=0;s<sides;s++) Tri(rings[3,s],center+Vector3.up*scale.y,rings[3,(s+1)%sides],color);
            }
            public Mesh Finish(string name) {var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(vertices);m.SetColors(colors);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
        [MenuItem("Maritime Legend/Manual Sailing V2/Open Test Scene")]
        public static void OpenTestScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        }
        [MenuItem("Maritime Legend/Manual Sailing V2/Play Test Scene")]
        public static void PlayTestScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorApplication.EnterPlaymode();
        }
        [MenuItem("Maritime Legend/Manual Sailing V2/Create Test Scene")]
        public static void CreateInteractive()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Generate();
        }
        public static void Generate(){
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("V2 Environment • sky and water controls");
            var environment=root.AddComponent<ManualSeaV2Environment>();root.AddComponent<ManualSeaV2Window>();
            var sun=new GameObject("Sun / Moon").AddComponent<Light>();sun.transform.SetParent(root.transform);sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;sun.shadowStrength=.85f;sun.shadowBias=.035f;sun.shadowNormalBias=.2f;
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=58;camera.nearClipPlane=.5f;camera.farClipPlane=40000;camera.clearFlags=CameraClearFlags.Skybox;camera.allowHDR=false;
            var orbit=camera.gameObject.AddComponent<ManualSeaV2Camera>();orbit.ApplyPose();
            var landMaterial=new Material(Shader.Find("Maritime/ManualSeaV2/Land")){name="V2 shared painted terrain",enableInstancing=true};var land=Save(landMaterial,"Land.mat");
            islands=new List<Island>();var rng=new System.Random(261027);
            for(int row=0;row<4;row++)for(int col=0;col<6;col++){
                islands.Add(new Island{center=new Vector3(-265+col*105+(float)rng.NextDouble()*12,0,-170+row*110+(float)rng.NextDouble()*15),scale=.66f+(float)rng.NextDouble()*.42f,variant=(row+col)%3,snow=row==3});
            }
            var meshes=new Mesh[6,2];
            for(int variant=0;variant<6;variant++)for(int lod=0;lod<2;lod++){
                int shape=variant%3;bool snow=variant>=3;float step=lod==0?1.25f:2.5f;var geo=new Geometry();
                for(float z=-40;z<40;z+=step)for(float x=-50;x<50;x+=step){
                    Vector3 a=new Vector3(x,LocalHeight(x,z,shape),z),b=new Vector3(x,LocalHeight(x,z+step,shape),z+step),c=new Vector3(x+step,LocalHeight(x+step,z,shape),z),d=new Vector3(x+step,LocalHeight(x+step,z+step,shape),z+step);
                    if(Mathf.Max(Mathf.Max(a.y,b.y),Mathf.Max(c.y,d.y))<-.9f)continue;
                    geo.Tri(a,b,c,Paint(x+step/3,z+step/3,(a.y+b.y+c.y)/3,shape,snow));geo.Tri(c,b,d,Paint(x+step*.66f,z+step*.66f,(c.y+b.y+d.y)/3,shape,snow));
                }
                meshes[variant,lod]=Save(geo.Finish("Island template "+variant+" LOD "+lod),"Island-"+variant+"-LOD"+lod+".asset");
            }
            foreach(var island in islands){
                var group=new GameObject(island.snow?"Northern snowy mountains":"Earth summit island");group.transform.SetParent(root.transform);group.transform.position=island.center;group.transform.localScale=new Vector3(island.scale,1,island.scale);
                var lods=new LOD[2];for(int l=0;l<2;l++){var obj=MeshObject("Terrain LOD "+l,meshes[island.variant+(island.snow?3:0),l],land);obj.transform.SetParent(group.transform,false);lods[l]=new LOD(l==0?.18f:.015f,new[]{obj.GetComponent<Renderer>()});}
                var lodGroup=group.AddComponent<LODGroup>();lodGroup.SetLODs(lods);lodGroup.RecalculateBounds();
            }
            var depth=new Texture2D(1536,1152,TextureFormat.RFloat,false,true){name="V2 large map bathymetry",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};var pixels=new Color[1536*1152];
            for(int y=0;y<1152;y++)for(int x=0;x<1536;x++)pixels[y*1536+x]=new Color((Height((x+.5f)/1536*640-320,(y+.5f)/1152*480-240)+32)/64,0,0,1);
            depth.SetPixels(pixels);depth.Apply();depth=Save(depth,"Bathymetry.asset");
            var water=new Material(Shader.Find("Maritime/ManualSeaV2/Water")){name="V2 azure ocean"};water.SetTexture("_DepthMap",depth);water=Save(water,"Water.mat");
            var sea=new Geometry();sea.Tri(new Vector3(-20000,0,-20000),new Vector3(-20000,0,20000),new Vector3(20000,0,-20000),Color.white);sea.Tri(new Vector3(20000,0,-20000),new Vector3(-20000,0,20000),new Vector3(20000,0,20000),Color.white);
            MeshObject("Ocean • large swell and sky reflections",Save(sea.Finish("V2 sea plane"),"Ocean.asset"),water).transform.SetParent(root.transform);
            var instances=root.AddComponent<ManualSeaV2Instances>();instances.view=camera;instances.material=land;instances.nearMeshes=new Mesh[3];instances.farMeshes=new Mesh[3];
            for(int variant=0;variant<3;variant++)for(int lod=0;lod<2;lod++){
                var geo=new Geometry();int sides=lod==0?6:4;
                if(variant<2){geo.Rock(Vector3.zero,new Vector3(.14f,1.5f,.14f),new Color(.27f,.20f,.11f),rng,4);int count=lod==0?3:1;
                    for(int j=0;j<count;j++)geo.Rock(new Vector3(Mathf.Cos(j*2.4f)*.3f,1+j*.35f,Mathf.Sin(j*2.4f)*.3f),new Vector3(.85f,1.65f,.8f),variant==0?new Color(.17f,.35f,.18f):new Color(.32f,.44f,.16f),rng,sides);
                }else geo.Rock(Vector3.zero,new Vector3(1,2.2f,.85f),new Color(.48f,.44f,.35f),rng,sides);
                var mesh=Save(geo.Finish("Shared prop "+variant+" LOD "+lod),"Prop-"+variant+"-LOD"+lod+".asset");if(lod==0)instances.nearMeshes[variant]=mesh;else instances.farMeshes[variant]=mesh;
            }
            var buckets=new Dictionary<Vector3Int,List<Matrix4x4>>();
            foreach(var island in islands)for(int i=0;i<1600;i++){
                float lx=(float)rng.NextDouble()*90-45,lz=(float)rng.NextDouble()*70-35,h=LocalHeight(lx,lz,island.variant);float x=island.center.x+lx*island.scale,z=island.center.z+lz*island.scale;
                float slope=new Vector2(LocalHeight(lx+.5f,lz,island.variant)-LocalHeight(lx-.5f,lz,island.variant),LocalHeight(lx,lz+.5f,island.variant)-LocalHeight(lx,lz-.5f,island.variant)).magnitude;
                if(h<.3f||h>6||slope>1.3f||N(x,z,.2f)<.38f)continue;
                int variant=h<1||i%9==0?2:i%2;float scale=.65f+(float)rng.NextDouble()*.8f;
                var key=new Vector3Int(Mathf.FloorToInt(x/40),Mathf.FloorToInt(z/40),variant);if(!buckets.ContainsKey(key))buckets[key]=new List<Matrix4x4>();
                buckets[key].Add(Matrix4x4.TRS(new Vector3(x,h-.05f,z),Quaternion.Euler(0,(float)rng.NextDouble()*360,0),Vector3.one*scale));
            }
            var cells=new List<ManualSeaV2Instances.Cell>();int propCount=0;
            foreach(var pair in buckets){for(int offset=0;offset<pair.Value.Count;offset+=1023){int count=Math.Min(1023,pair.Value.Count-offset);cells.Add(new ManualSeaV2Instances.Cell{variant=pair.Key.z,bounds=new Bounds(new Vector3(pair.Key.x*40+20,8,pair.Key.y*40+20),new Vector3(46,24,46)),transforms=pair.Value.GetRange(offset,count).ToArray()});propCount+=count;}}
            instances.cells=cells.ToArray();
            var skies=new Material[3];for(int i=0;i<3;i++){var sky=new Material(Shader.Find("Maritime/ManualSeaV2/Sky")){name=i==0?"Sunny sky":i==1?"Sunset sky":"Starry moonlit sky"};sky.SetFloat("_Mode",i);sky.SetVector("_SunDirection",-(Quaternion.Euler(i==0?48:i==1?8:6,i==0?-35:180,0)*Vector3.forward));skies[i]=Save(sky,"Sky-"+i+".mat");}
            environment.skies=skies;environment.water=water;environment.sun=sun;environment.view=camera;environment.Apply();
            var benchmark=root.AddComponent<ManualSeaV2Benchmark>();benchmark.environment=environment;benchmark.orbit=orbit;benchmark.instances=instances;
            foreach(var name in new[]{"Maritime/ManualSeaV2/Water","Maritime/ManualSeaV2/Land","Maritime/ManualSeaV2/Sky"}){var shader=Shader.Find(name);if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("Shader failed: "+name);}
            EditorSceneManager.SaveScene(camera.gameObject.scene,ScenePath);AssetDatabase.SaveAssets();
            // Editor captures are deterministic; instances are submitted to the capture camera as well.
            instances.Render();Capture(camera,water,12,"overview");
            orbit.focus=new Vector3(-55,0,163);orbit.distance=120;orbit.ApplyPose();instances.Render();Capture(camera,water,12,"northern-snow");
            orbit.focus=new Vector3(0,0,-40);orbit.pitch=35;orbit.ApplyPose();
            for(int i=0;i<3;i++){environment.mode=(ManualSeaV2Environment.SkyMode)i;environment.Apply();instances.Render();Capture(camera,water,12,"sky-"+i);}
            orbit.pitch=35;orbit.ApplyPose();environment.mode=ManualSeaV2Environment.SkyMode.Sunny;environment.Apply();
            EditorSceneManager.SaveScene(camera.gameObject.scene,ScenePath);AssetDatabase.SaveAssets();
            File.WriteAllText(ReviewDirectory()+"/geometry.txt",$"Map: 640 x 480 (16x previous area)\nIslands: {islands.Count}\nShared island meshes: 6 shapes/colors x 2 LODs\nProps: {propCount}\nProp cells: {cells.Count}\nShared prop meshes: 3 x 2 LODs\nOcean: 2 triangles\n");
            Debug.Log("MANUAL_SAILING_V2_PASS: expanded map, three skies, shader checks and captures complete.");
        }
        static string ReviewDirectory(){var repo=new DirectoryInfo(Application.dataPath);while(repo!=null&&!File.Exists(Path.Combine(repo.FullName,"AGENTS.md")))repo=repo.Parent;if(repo==null)throw new DirectoryNotFoundException("Repository root not found");string path=Path.Combine(repo.FullName,"ArtSource/ManualSailingV2/Review");Directory.CreateDirectory(path);return path;}
        public static void BuildBenchmark(){PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;Generate();Directory.CreateDirectory("Builds/V2");var result=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/V2/ManualSeaV2.exe",target=UnityEditor.BuildTarget.StandaloneWindows64,options=UnityEditor.BuildOptions.Development});if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("V2 build failed");}
        static void Capture(Camera camera,Material water,float time,string name)
        {
            var repo=new DirectoryInfo(Application.dataPath);
            while(repo!=null && !File.Exists(Path.Combine(repo.FullName,"AGENTS.md"))) repo=repo.Parent;
            if(repo==null)throw new DirectoryNotFoundException("Repository AGENTS.md not found");
            string output=Path.Combine(repo.FullName,"ArtSource/ManualSailingV2/Review");Directory.CreateDirectory(output);
            var rt=new RenderTexture(1600,1000,24){antiAliasing=4};var previous=RenderTexture.active;
            water.SetFloat("_PreviewTime",time);camera.targetTexture=rt;camera.aspect=1.6f;camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());
            camera.targetTexture=null;camera.ResetAspect();RenderTexture.active=previous;water.SetFloat("_PreviewTime",-1);UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}



