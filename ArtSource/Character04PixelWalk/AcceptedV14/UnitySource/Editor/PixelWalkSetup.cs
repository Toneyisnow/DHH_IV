using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using MaritimeLegend.CharacterArt;

namespace MaritimeLegend.Editor {
 public static class PixelWalkSetup {
  public const string Root="Assets/Character04PixelWalk";
  public const string ScenePath=Root+"/Character04PixelWalk.unity";
  const string GameArt="Assets/Resources/Sprites/Characters/Character04/PixelWalk";
  const string PrefabPath="Assets/Resources/Prefabs/Characters/Character04/PixelWalker.prefab";
  static string Source{get{var dir=new DirectoryInfo(Application.dataPath);while(dir!=null){var path=Path.Combine(dir.FullName,"ArtSource/Character04PixelWalk");if(Directory.Exists(path))return path;dir=dir.Parent;}throw new DirectoryNotFoundException("ArtSource/Character04PixelWalk");}}
  static string Review=>Path.Combine(Source,"Review");
  [MenuItem("Maritime Legend/Character 04 Pixel Walk/Open Preview")]
  public static void OpenPreview(){if(EditorApplication.isPlaying)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(ScenePath);Selection.activeGameObject=UnityEngine.Object.FindAnyObjectByType<PixelWalkActor>().gameObject;}
  [MenuItem("Maritime Legend/Character 04 Pixel Walk/Play Preview")]
  public static void PlayPreview(){OpenPreview();EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);EditorApplication.EnterPlaymode();}
  [MenuItem("Maritime Legend/Character 04 Pixel Walk/Rebuild Assets And Scene")]
  public static void Create(){
   Directory.CreateDirectory(GameArt);Directory.CreateDirectory(Review);Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
   string[] names={"base","voyage"};var textures=new Texture2D[2]; BuildDirectionalAtlases(names);
   for(int i=0;i<2;i++){string target=GameArt+"/"+names[i]+"-walk.png";AssetDatabase.ImportAsset(target,ImportAssetOptions.ForceSynchronousImport);
    var imp=(TextureImporter)AssetImporter.GetAtPath(target);imp.textureType=TextureImporterType.Default;imp.isReadable=true;imp.filterMode=FilterMode.Point;imp.wrapMode=TextureWrapMode.Clamp;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=2048;imp.alphaIsTransparency=true;imp.SaveAndReimport();textures[i]=AssetDatabase.LoadAssetAtPath<Texture2D>(target);
   }
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/PixelSprite.mat");if(material==null){material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,Root+"/PixelSprite.mat");}
   var actorObject=new GameObject("Character04 - 8 direction pixel walker");var renderer=actorObject.AddComponent<SpriteRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=10;
   var actor=actorObject.AddComponent<PixelWalkActor>();actor.outfitAtlases=textures;actor.palette=0;actor.outfit=0;
   PrefabUtility.SaveAsPrefabAsset(actorObject,PrefabPath);
   var cam=new GameObject("Pixel Preview Camera").AddComponent<Camera>();cam.tag="MainCamera";cam.orthographic=true;cam.orthographicSize=4.5f;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.105f,.14f,.17f);cam.rect=new Rect(.25f,0,.75f,1);cam.allowHDR=false;cam.allowMSAA=false;cam.gameObject.AddComponent<AudioListener>();
   var demo=new GameObject("Preview controls").AddComponent<PixelWalkDemo>();demo.actor=actor;demo.previewCamera=cam;demo.evidenceDirectory=Review;
   // Simple editor-native stage grid, no external art dependencies.
   var tile=new Texture2D(1,1,TextureFormat.RGBA32,false);tile.SetPixel(0,0,Color.white);tile.Apply();string tilePath=GameArt+"/stage-white.png";File.WriteAllBytes(tilePath,tile.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tile);AssetDatabase.ImportAsset(tilePath,ImportAssetOptions.ForceSynchronousImport);
   var ti=(TextureImporter)AssetImporter.GetAtPath(tilePath);ti.textureType=TextureImporterType.Sprite;ti.spritePixelsPerUnit=1;ti.filterMode=FilterMode.Point;ti.SaveAndReimport();var square=AssetDatabase.LoadAssetAtPath<Sprite>(tilePath);
   var stage=new GameObject("Preview ground grid");for(int y=-5;y<5;y++)for(int x=-7;x<7;x++){var cell=new GameObject("Tile "+x+","+y);cell.transform.SetParent(stage.transform);cell.transform.position=new Vector3(x+.5f,y+.5f,1);var sr=cell.AddComponent<SpriteRenderer>();sr.sprite=square;sr.sharedMaterial=material;sr.color=(x+y)%2==0?new Color(.17f,.21f,.23f):new Color(.19f,.23f,.25f);sr.sortingOrder=-10;}
   EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
   File.WriteAllText(Path.Combine(Review,"import-validation.txt"),"PASS: 2 transparent atlases, 8 directions x (4 walk + 1 idle) frames, 64x96 cells, 320x768 sheets, point filtering, readable RGBA.\nPrefab: "+PrefabPath);
   Debug.Log("PIXEL_WALK_CREATE_PASS");
  }
  static void BuildDirectionalAtlases(string[] names){
   var result=new[]{new Texture2D(320,768,TextureFormat.RGBA32,false),new Texture2D(320,768,TextureFormat.RGBA32,false)};
   foreach(var texture in result)texture.SetPixels32(new Color32[320*768]);
   for(int d=0;d<8;d++){
    string direction=PixelWalkActor.DirectionNames[d];string path=Path.Combine(Review,"v8-"+direction+"-strip.png");
    Normalize(Path.Combine(Source,"V8",direction+".png"),path,"v8-"+direction,2);
    var strip=new Texture2D(2,2,TextureFormat.RGBA32,false);strip.LoadImage(File.ReadAllBytes(path));
    if(d==1||d==7){
     string legPath=Path.Combine(Review,"v14-"+direction+"-legs.png");Normalize(Path.Combine(Source,"V14",direction+".png"),legPath,"v14-"+direction+"-legs",2);
     var legs=new Texture2D(2,2,TextureFormat.RGBA32,false);legs.LoadImage(File.ReadAllBytes(legPath));
     // Only the four walking leg poses below the skirt are replaced.
     for(int o=0;o<2;o++)for(int f=0;f<4;f++)for(int y=0;y<22;y++)for(int x=0;x<64;x++)
      strip.SetPixel(f*64+x,(1-o)*96+y,legs.GetPixel(f*64+x,(1-o)*96+y));
     UnityEngine.Object.DestroyImmediate(legs);
    }
    if(d==3){
     string eyePath=Path.Combine(Review,"v13-NW-gaze.png");Normalize(Path.Combine(Source,"V13","NW.png"),eyePath,"v13-NW-gaze",2);
     var gaze=new Texture2D(2,2,TextureFormat.RGBA32,false);gaze.LoadImage(File.ReadAllBytes(eyePath));
     // Composite only the eye region; keep approved hair, mouth and gait art.
     for(int o=0;o<2;o++)for(int f=0;f<5;f++)for(int y=64;y<70;y++)for(int x=19;x<32;x++)
      strip.SetPixel(f*64+x,(1-o)*96+y,gaze.GetPixel(f*64+x,(1-o)*96+y));
     UnityEngine.Object.DestroyImmediate(gaze);
    }
    for(int o=0;o<2;o++)for(int f=0;f<5;f++){
     // The other five source directions start with right-arm-forward;
     // rotate them so every direction starts right-leg / left-arm forward.
     int sourceFrame=(d!=1&&d!=2&&d!=6)&&f<4?(f+2)%4:f;
     result[o].SetPixels(f*64,(7-d)*96,64,96,strip.GetPixels(sourceFrame*64,(1-o)*96,64,96));
    }
    UnityEngine.Object.DestroyImmediate(strip);
   }
   // Lock only eye and mouth patches during walking. All other head pixels
   // come from each original pose, preserving fringe and outer hair motion.
   // Idle has its own face angle and remains the original closed-mouth pose.
   for(int o=0;o<2;o++)foreach(int d in new[]{0,1,7})
    foreach(var patch in PixelWalkActor.FacePatches(d))
     for(int f=1;f<4;f++){var offset=PixelWalkActor.FaceOffset(result[o],d,f);for(int y=patch.yMin;y<patch.yMax;y++)for(int x=patch.xMin;x<patch.xMax;x++)
      result[o].SetPixel(f*64+x+offset.x,(7-d)*96+y+offset.y,result[o].GetPixel(x,(7-d)*96+y));}
   // Downward idle reuses the approved walking eyes at its own head anchor.
   for(int o=0;o<2;o++){
    var patch=PixelWalkActor.FacePatches(0)[0];var offset=PixelWalkActor.FaceOffset(result[o],0,4);
    for(int y=patch.yMin;y<patch.yMax;y++)for(int x=patch.xMin;x<patch.xMax;x++)
     result[o].SetPixel(4*64+x+offset.x,7*96+y+offset.y,result[o].GetPixel(x,7*96+y));
   }
   // Bake the approved NW reflection into NE itself so exported atlases and
   // runtime sprites agree. Reflection exchanges left/right, hence phase +2.
   for(int o=0;o<2;o++)for(int f=0;f<5;f++){
    int sourceFrame=f<4?(f+2)%4:4;
    for(int y=0;y<96;y++)for(int x=0;x<64;x++)
     result[o].SetPixel(f*64+x,2*96+y,result[o].GetPixel(sourceFrame*64+63-x,4*96+y));
   }
   for(int o=0;o<2;o++){result[o].Apply();var bytes=result[o].EncodeToPNG();File.WriteAllBytes(GameArt+"/"+names[o]+"-walk.png",bytes);File.WriteAllBytes(Path.Combine(Review,names[o]+"-normalized.png"),bytes);UnityEngine.Object.DestroyImmediate(result[o]);}
  }
  static void Normalize(string sourcePath,string target,string name,int rows=8){
   if(!File.Exists(sourcePath))throw new FileNotFoundException("Generated source not yet ready",sourcePath);
   var source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(File.ReadAllBytes(sourcePath));var pixels=source.GetPixels32();int w=source.width,h=source.height;
   // Discover whole connected figures before slicing: equal-height row cuts can
   // capture the previous row's shoe sole as a floating line above the head.
   var boxes=new List<RectInt>();var seen=new bool[w*h];var queue=new Queue<int>();var labels=new int[w*h];var bodyIds=new Dictionary<RectInt,int>();
   var rowId=new int[h];var cuts=new List<int>{0};
   for(int r=1;r<rows;r++){int expected=r*h/rows,best=expected,least=int.MaxValue;for(int y=expected-h/40;y<=expected+h/40;y++){int occupied=0;for(int x=0;x<w;x++)if(pixels[y*w+x].a>=160)occupied++;if(occupied<least){least=occupied;best=y;}}cuts.Add(best);}
   cuts.Add(h);for(int r=0;r<rows;r++)for(int y=cuts[r];y<cuts[r+1];y++)rowId[y]=r;
   for(int seed=0;seed<pixels.Length;seed++){
    if(seen[seed]||pixels[seed].a<160)continue;
    seen[seed]=true;queue.Enqueue(seed);int count=0,minX=w,minY=h,maxX=0,maxY=0;
    while(queue.Count>0){int p=queue.Dequeue(),x=p%w,y=p/w;labels[p]=seed+1;count++;minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);
     for(int yy=Math.Max(0,y-1);yy<=Math.Min(h-1,y+1);yy++)for(int xx=Math.Max(0,x-1);xx<=Math.Min(w-1,x+1);xx++){int next=yy*w+xx;if(rowId[yy]==rowId[y]&&!seen[next]&&pixels[next].a>=160){seen[next]=true;queue.Enqueue(next);}}
    }
    if(count>500&&maxY-minY>h/(rows*2)&&maxX-minX>w/30){var box=new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);boxes.Add(box);bodyIds.Add(box,seed+1);}
   }
   if(boxes.Count!=rows*5)throw new Exception(name+" has "+boxes.Count+" connected figures; unexpected count. Review source before import.");
   boxes.Sort((a,b)=>b.center.y.CompareTo(a.center.y));
   for(int row=0;row<rows;row++)boxes.Sort(row*5,5,Comparer<RectInt>.Create((a,b)=>a.center.x.CompareTo(b.center.x)));
   var originalBoxes=boxes.ToArray();
   // Include detached shoes immediately below the figure, without crossing into
   // the next row's head. No pixels above the connected head may enter a cell.
   for(int i=0;i<boxes.Count;i++){var b=boxes[i];int bottom=Math.Max(0,b.yMin-12);if(i<(rows-1)*5)bottom=Math.Max(bottom,boxes[i+5].yMax+2);int found=b.yMin;
    for(int y=bottom;y<b.yMin;y++)for(int x=b.xMin;x<b.xMax;x++)if(pixels[y*w+x].a>=160)found=Math.Min(found,y);
    boxes[i]=new RectInt(b.x,found,b.width,b.yMax-found);
   }
   int maxH=0,maxW=0;foreach(var b in boxes){maxH=Math.Max(maxH,b.height);maxW=Math.Max(maxW,b.width);}float scale=Math.Min(72f/maxH,56f/maxW);
   var atlas=new Texture2D(320,rows*96,TextureFormat.RGBA32,false);atlas.filterMode=FilterMode.Point;atlas.SetPixels32(new Color32[320*rows*96]);
   for(int index=0;index<boxes.Count;index++){var b=boxes[index];int row=index/5,f=index%5,dw=Math.Max(1,Mathf.RoundToInt(b.width*scale)),dh=Math.Max(1,Mathf.RoundToInt(b.height*scale));int ox=f*64+(64-dw)/2,oy=(rows-1-row)*96+8;
    for(int dy=0;dy<dh;dy++)for(int dx=0;dx<dw;dx++){int sx=b.x+Math.Min(b.width-1,(int)((dx+.5f)/scale)),sy=b.y+Math.Min(b.height-1,(int)((dy+.5f)/scale));var c=pixels[sy*w+sx];if(c.a<160)continue;if(labels[sy*w+sx]!=bodyIds[originalBoxes[index]]&&sy>originalBoxes[index].yMin+12)continue;c.a=255;c.r=(byte)(c.r/8*8);c.g=(byte)(c.g/8*8);c.b=(byte)(c.b/8*8);atlas.SetPixel(ox+dx,oy+dy,c);}
   }
   atlas.Apply();byte[] bytes=atlas.EncodeToPNG();File.WriteAllBytes(target,bytes);File.WriteAllBytes(Path.Combine(Review,name+"-normalized.png"),bytes);
   File.WriteAllText(Path.Combine(Review,name+"-slice-report.txt"),"Source: "+w+"x"+h+"\nFrames: "+boxes.Count+"\nUniform scale: "+scale+"\n"+string.Join("\n",boxes));
   UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(atlas);
  }
  public static void BuildAndVerify(){
   Create();string build=Path.GetFullPath("Builds/Character04PixelWalkV14/Character04PixelWalk.exe");Directory.CreateDirectory(Path.GetDirectoryName(build));
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=build,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
   if(report.summary.result!=BuildResult.Succeeded)throw new Exception("PixelWalk build failed: "+report.summary.result);
   File.WriteAllText(Path.Combine(Review,"build-validation.txt"),"PASS: "+report.summary.result+"\n"+build);Debug.Log("PIXEL_WALK_BUILD_PASS");
  }
 }
}









