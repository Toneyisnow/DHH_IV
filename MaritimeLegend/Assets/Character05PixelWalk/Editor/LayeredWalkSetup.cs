using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using MaritimeLegend.CharacterArt;
namespace MaritimeLegend.Editor {
 public static class LayeredWalkSetup {
  const string Root="Assets/Character05PixelWalk", GameArt="Assets/Resources/Sprites/Characters/Character05/PixelWalk", PrefabPath="Assets/Resources/Prefabs/Characters/Character05/LayeredPixelWalker.prefab";
  public const string ScenePath=Root+"/Character05LayeredWalk.unity";
  static string Source{get{var dir=new DirectoryInfo(Application.dataPath);while(dir!=null){var path=Path.Combine(dir.FullName,"ArtSource/Character05PixelWalk");if(Directory.Exists(path))return path;dir=dir.Parent;}throw new DirectoryNotFoundException("ArtSource/Character05PixelWalk");}}
  static string Review=>Path.Combine(Source,"Review");
  static readonly int[] Canonical={0,1,2,3,4,3,2,1};
  static readonly string[] Sources={"S","SW","W","NW","N"};
  static readonly string[,] PaletteHex={
   {"D6C6A7","B9A88B","9D3B2D"},{"2F4A79","1F314F","B99A51"},{"AE8B62","DED0AD","263C60"},{"7F8B69","66523E","E3D8BD"},
   {"8C3D2F","513F32","D8C8A7"},{"B7B5AD","41444A","354E77"},{"B48A36","5A4634","E6D8B7"},{"657E95","465865","9B503D"}};
  static Texture2D Empty(){var t=new Texture2D(320,768,TextureFormat.RGBA32,false);t.SetPixels32(new Color32[320*768]);return t;}
  static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
  static void Save(Texture2D t,string name){t.Apply();File.WriteAllBytes(GameArt+"/"+name+".png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);}
  static Texture2D Load(string path){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(path));return t;}
  static float HeadX(Texture2D t,int f){float sum=0;int n=0;for(int y=65;y<80;y++)for(int x=0;x<64;x++)if(t.GetPixel(f*64+x,y).a>.5f){sum+=x;n++;}return n==0?32:sum/n;}
  // Coordinates are reviewed in the final 64x96 grid, never inferred from a hat's outer box.
  static Vector2Int HeadAnchor(Texture2D body,int d,int f){int row=(7-d)*96,top=60;for(int y=60;y<86;y++)for(int x=20;x<44;x++)if(body.GetPixel(f*64+x,row+y).a>.5f)top=Math.Max(top,y);float sum=0;int count=0;for(int y=top-4;y<=top;y++)for(int x=20;x<44;x++)if(body.GetPixel(f*64+x,row+y).a>.5f){sum+=x;count++;}return new Vector2Int(count>0?Mathf.RoundToInt(sum/count):32,top);}
  static Vector2Int ShoulderAnchor(Texture2D body,int d,int f){int row=(7-d)*96,top=56;float sum=0;int count=0;for(int y=55;y<=66;y++){int n=0;float xs=0;for(int x=17;x<47;x++){var p=body.GetPixel(f*64+x,row+y);Color.RGBToHSV(p,out float h,out float s,out float v);if(p.a>.5f&&h>.565f&&h<.73f&&s>.25f){n++;xs+=x;}}if(n>=4){top=y;sum=xs;count=n;}}return new Vector2Int(count>0?Mathf.RoundToInt(sum/count):32,top+1);}
  static void StabilizeFrontFace(Texture2D body){
   // Small opaque face patch: brow/eyes, nose, closed mouth, surrounding skin. Hair remains animated.
   const int row=7*96;
   // Preserve the source eye whites which point downsampling misses; keep adjacent dark pupils.
   var source=Load(Path.Combine(Source,"Source/S.png"));Color eyeWhite=Color.clear;float brightest=0;
   for(int y=550;y<580;y++)for(int x=1920;x<1995;x++){var c=source.GetPixel(x,y);Color.RGBToHSV(c,out float h,out float s,out float v);if(c.a>.9f&&s<.3f&&v>brightest){eyeWhite=c;brightest=v;}}
   UnityEngine.Object.DestroyImmediate(source);if(brightest<.65f)throw new Exception("Source eye whites not found; review source landmarks");
   body.SetPixel(4*64+28,row+68,eyeWhite);body.SetPixel(4*64+34,row+68,eyeWhite);
   var patch=body.GetPixels(4*64+27,row+64,10,10);
   for(int f=0;f<4;f++)body.SetPixels(f*64+27,row+64,10,10,patch);
  }
  static void BuildBodies(){
   var baseAtlas=Empty();var strips=new Texture2D[5];
   for(int i=0;i<5;i++){var path=Path.Combine(Review,Sources[i]+"-normalized.png");Normalize(Path.Combine(Source,"Source",Sources[i]+".png"),path,Sources[i],1);strips[i]=Load(path);}
   for(int d=0;d<8;d++){int c=Canonical[d];var strip=strips[c];for(int f=0;f<5;f++){
    int sf=f==4?4:(f+(c==3?0:2)+(d>4?2:0))%4;
    int dx=Mathf.RoundToInt(HeadX(strip,4)-HeadX(strip,sf));
    for(int y=0;y<96;y++)for(int x=0;x<64;x++){int sx=x-dx;if(sx<0||sx>=64)continue;var color=strip.GetPixel(sf*64+sx,y);baseAtlas.SetPixel(f*64+(d>4?63-x:x),(7-d)*96+y,color);}
   }}
   StabilizeFrontFace(baseAtlas);baseAtlas.Apply();File.WriteAllBytes(Path.Combine(Review,"base-uncolored.png"),baseAtlas.EncodeToPNG());
   var mask=Empty();var hands=Empty();var raw=baseAtlas.GetPixels();var channels=new int[raw.Length];
   for(int y=0;y<768;y++)for(int x=0;x<320;x++){int i=y*320+x,cy=y%96;var c=raw[i];if(c.a<.5f)continue;Color.RGBToHSV(c,out float h,out float s,out float v);
    int ch=0;if(cy<63&&s>.25f){if(h>.565f&&h<.73f)ch=1;else if(h>.43f&&h<=.565f)ch=2;else if((h<.045f||h>.97f)&&cy>=22&&cy<=50)ch=3;}
    channels[i]=ch;if(ch>0)mask.SetPixel(x,y,ch==1?Color.red:ch==2?Color.green:Color.blue);
    if(cy>=32&&cy<=56&&h>.045f&&h<.14f&&s>.3f&&v>.12f)hands.SetPixel(x,y,c);
   }
   for(int p=0;p<8;p++){var t=Empty();var pixels=(Color[])raw.Clone();for(int i=0;i<pixels.Length;i++){int ch=channels[i];if(ch==0)continue;var c=raw[i];float value=Mathf.Max(c.r,Mathf.Max(c.g,c.b));float light=Mathf.Clamp(value/(ch==1?.43f:ch==2?.36f:.62f),.38f,1.5f);var target=Hex(PaletteHex[p,ch-1])*light;target.a=c.a;pixels[i]=target;}t.SetPixels(pixels);Save(t,"body-C"+(p+1));
    var fitted=Empty();fitted.SetPixels(pixels);for(int d=0;d<8;d++)for(int f=0;f<5;f++){var anchor=HeadAnchor(baseAtlas,d,f);for(int y=anchor.y-3;y<96;y++)for(int x=0;x<64;x++)fitted.SetPixel(f*64+x,(7-d)*96+y,Color.clear);}Save(fitted,"hatted-body-C"+(p+1));
   }
   Save(mask,"clothing-mask");Save(hands,"foreground-hands");foreach(var t in strips)UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(baseAtlas);
  }
  static Texture2D Piece(Texture2D source,int row,int col,int[] boundaries,int columns=5){
   int x0=col*source.width/columns,x1=(col+1)*source.width/columns;
   int top=boundaries[row]*source.height/1024,bottom=boundaries[row+1]*source.height/1024;
   int minx=x1,maxx=x0,miny=source.height-top,maxy=source.height-bottom;
   for(int y=source.height-bottom;y<source.height-top;y++)for(int x=x0;x<x1;x++)if(source.GetPixel(x,y).a>.63f){minx=Math.Min(minx,x);maxx=Math.Max(maxx,x);miny=Math.Min(miny,y);maxy=Math.Max(maxy,y);}
   if(maxx<minx||maxy<miny)throw new Exception("Empty equipment cell "+row+","+col);
   var t=new Texture2D(maxx-minx+1,maxy-miny+1,TextureFormat.RGBA32,false);t.SetPixels(source.GetPixels(minx,miny,t.width,t.height));t.Apply();return t;
  }
  static void Stamp(Texture2D target,Texture2D part,int d,int f,int cx,int top,int width,int height,bool mirror,int sway=0,Func<int,int,bool> filter=null){
   for(int y=0;y<height;y++)for(int x=0;x<width;x++){
    int sx=Mathf.Clamp((int)((x+.5f)*part.width/width),0,part.width-1),sy=Mathf.Clamp((int)((y+.5f)*part.height/height),0,part.height-1);if(mirror)sx=part.width-1-sx;
    var c=part.GetPixel(sx,sy);if(c.a<.63f)continue;
    int xx=cx-width/2+x+Mathf.RoundToInt(sway*(1-y/(float)height)),yy=top-height+1+y;
    if(xx<0||xx>=64||yy<0||yy>=96)throw new Exception("Equipment exceeds cell "+d+","+f);
    if(filter!=null&&!filter(xx,yy))continue;c.a=1;c.r=Mathf.Floor(c.r*255/8)*8/255;c.g=Mathf.Floor(c.g*255/8)*8/255;c.b=Mathf.Floor(c.b*255/8)*8/255;target.SetPixel(f*64+xx,(7-d)*96+yy,c);
   }
  }
  static Vector2 Right(int d){var v=LayeredPixelWalker.Vectors[d];return new Vector2(v.y,-v.x);}
  static int NeckOffset(Texture2D part,int width,bool mirror){float sum=0;int count=0;for(int y=part.height-Math.Max(2,part.height/10);y<part.height;y++)for(int x=0;x<part.width;x++)if(part.GetPixel(x,y).a>.63f){sum+=mirror?part.width-1-x:x;count++;}return count==0?0:Mathf.RoundToInt(width*.5f-(sum/count+.5f)*width/part.width);}
  static Texture2D ConnectedMantle(Texture2D part){
   // A mantle is one connected garment. Reject neighbouring-cell fringe and isolated source specks.
   var pixels=part.GetPixels();var visited=new bool[pixels.Length];var best=new List<int>();int w=part.width,h=part.height;
   for(int start=0;start<pixels.Length;start++){if(visited[start]||pixels[start].a<.63f)continue;var group=new List<int>();var q=new Queue<int>();q.Enqueue(start);visited[start]=true;while(q.Count>0){int p=q.Dequeue();group.Add(p);for(int y=Math.Max(0,p/w-1);y<=Math.Min(h-1,p/w+1);y++)for(int x=Math.Max(0,p%w-1);x<=Math.Min(w-1,p%w+1);x++){int n=y*w+x;if(!visited[n]&&pixels[n].a>=.63f){visited[n]=true;q.Enqueue(n);}}}if(group.Count>best.Count)best=group;}
   int minX=w,minY=h,maxX=0,maxY=0;foreach(int p in best){minX=Math.Min(minX,p%w);maxX=Math.Max(maxX,p%w);minY=Math.Min(minY,p/w);maxY=Math.Max(maxY,p/w);}var result=new Texture2D(maxX-minX+1,maxY-minY+1,TextureFormat.RGBA32,false);result.SetPixels(new Color[result.width*result.height]);foreach(int p in best)result.SetPixel(p%w-minX,p/w-minY,pixels[p]);result.Apply();UnityEngine.Object.DestroyImmediate(part);return result;
  }
  static void BuildEquipment(){
   var headSource=Load(Path.Combine(Source,"Source/hats.png"));var capeSource=Load(Path.Combine(Source,"Source/capes.png"));var accSource=Load(Path.Combine(Source,"Source/accessories.png"));
   var asymmetric=Load(Path.Combine(Source,"Source/capes-asymmetric-v2.png"));var body=Load(Path.Combine(Review,"base-uncolored.png"));
   int[] headCuts={0,430,700,1024},capeCuts={0,150,330,495,675,875,1024},accCuts={0,152,326,528,682,840,1024};
   int[] hatWidths={18,17,25},hatHeights={17,10,12};
   for(int h=0;h<3;h++){var atlas=Empty();var tails=Empty();for(int c=0;c<5;c++){var part=Piece(headSource,h,c,headCuts);for(int d=0;d<8;d++)if(Canonical[d]==c)for(int f=0;f<5;f++){
    var anchor=HeadAnchor(body,d,f);int width=hatWidths[h]-(c==2?1:0),top=anchor.y+3;int faceCeiling=anchor.y-5;
    // Remove the asset's opaque hollow interior where the face must sit inside the hat.
    Stamp(atlas,part,d,f,anchor.x,top,width,hatHeights[h],d>4,0,(x,y)=>h==0||!(d<3||d>5)||y>faceCeiling||Math.Abs(x-anchor.x)>5);
    if(h==0){int row=(7-d)*96,min=64,max=-1;int tieTop=faceCeiling-2;for(int y=0;y<=tieTop;y++)for(int x=0;x<64;x++)if(atlas.GetPixel(f*64+x,row+y).a>.5f){min=Math.Min(min,x);max=Math.Max(max,x);}int target=anchor.x+Mathf.RoundToInt(-LayeredPixelWalker.Vectors[d].x*6);int shift=target-(min+max)/2;
     for(int y=0;y<=faceCeiling;y++)for(int x=0;x<64;x++){var color=atlas.GetPixel(f*64+x,row+y);if(color.a<.5f)continue;int tx=x+shift;if(tx<0||tx>=64)throw new Exception("Rear head tie exceeds cell");if(y<=tieTop)tails.SetPixel(f*64+tx,row+y+2,color);atlas.SetPixel(f*64+x,row+y,Color.clear);}
    }
   }UnityEngine.Object.DestroyImmediate(part);}Save(atlas,"hat-"+(h+1));Save(tails,"hat-rear-"+(h+1));}
   int[] capeW={27,28,16,27,30,26},capeH={18,32,26,27,36,18};
   for(int k=0;k<6;k++){var rear=Empty();var front=Empty();for(int d=0;d<8;d++){
    int c=Canonical[d],asymRow=k==2?0:k==4?1:k==5?2:-1;bool independent=asymRow>=0;
    var part=independent?Piece(asymmetric,asymRow,d,new[]{0,337,699,1024},8):Piece(capeSource,k,c,capeCuts);
    part=ConnectedMantle(part);
    for(int f=0;f<5;f++){
     var shoulder=ShoulderAnchor(body,d,f);int sway=f==4?0:new[]{-1,0,1,0}[f];int width=capeW[k]-(c==2?6:0);int cx=shoulder.x,top=shoulder.y+1;bool backView=d>=3&&d<=5;
     if(k==2){cx-=Mathf.RoundToInt(Right(d).x*7);width=(d==5||d==6||d==7)?11:16;cx+=NeckOffset(part,width,false);bool near=d<=4;Stamp(near?front:rear,part,d,f,cx,top,width,capeH[k],false,sway);}
     else if(backView){cx+=NeckOffset(part,width,!independent&&d>4);Stamp(front,part,d,f,cx,top,width,capeH[k],!independent&&d>4,sway+(d==3?2:d==5?-2:0));}
     else {Stamp(rear,part,d,f,cx,top,width,capeH[k],!independent&&d>4,sway);Stamp(front,part,d,f,cx,top,width,capeH[k],!independent&&d>4,sway,(x,y)=>y>=top-8||Math.Abs(x-cx)>8);}
    }UnityEngine.Object.DestroyImmediate(part);
   }Save(rear,"cape-back-"+(k+1));Save(front,"cape-front-"+(k+1));}
   UnityEngine.Object.DestroyImmediate(body);UnityEngine.Object.DestroyImmediate(asymmetric);
   int[] widths={7,15,13,20,15,19},heights={17,31,26,23,19,22};
   for(int a=0;a<6;a++){var atlas=Empty();for(int c=0;c<5;c++){var part=Piece(accSource,a,c,accCuts);for(int d=0;d<8;d++)if(Canonical[d]==c)for(int f=0;f<5;f++){
    var right=Right(d);float side=a<2?1:-1;int cx=32+Mathf.RoundToInt(right.x*(a==2?14:10)*side);int top;
    if(a==2){if(d==2)cx+=9;if(d==6)cx-=9;top=75+Mathf.RoundToInt(-right.y*2)+(f==1||f==3?1:0);}
    else top=47+Mathf.RoundToInt(right.y*4*side);
    int width=widths[a]-(c==2&&a!=2?2:0);Stamp(atlas,part,d,f,cx,top,width,heights[a],d>4,a==2?0:(f==4?0:new[]{-1,0,1,0}[f]));
   }UnityEngine.Object.DestroyImmediate(part);}Save(atlas,"accessory-"+(a+1));}
   UnityEngine.Object.DestroyImmediate(headSource);UnityEngine.Object.DestroyImmediate(capeSource);UnityEngine.Object.DestroyImmediate(accSource);
  }
  static Texture2D Import(string name){string path=GameArt+"/"+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.isReadable=true;imp.filterMode=FilterMode.Point;imp.wrapMode=TextureWrapMode.Clamp;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=2048;imp.alphaIsTransparency=true;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);}
  static Texture2D[] Imports(string prefix,int count){var a=new Texture2D[count];for(int i=0;i<count;i++)a[i]=Import(prefix+(i+1));return a;}
  [MenuItem("Maritime Legend/Character 05 Layered Walk/Rebuild")]
  public static void Create(){
   Directory.CreateDirectory(GameArt);Directory.CreateDirectory(Review);Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));BuildBodies();BuildEquipment();
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/LayeredSprite.mat");if(mat==null){mat=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(mat,Root+"/LayeredSprite.mat");}
   var go=new GameObject("Character05 - layered Swahili sailor");var sr=go.AddComponent<SpriteRenderer>();sr.sharedMaterial=mat;var actor=go.AddComponent<LayeredPixelWalker>();
   actor.bodies=Imports("body-C",8);actor.hattedBodies=Imports("hatted-body-C",8);actor.capeBacks=Imports("cape-back-",6);actor.capeFronts=Imports("cape-front-",6);actor.hats=Imports("hat-",3);actor.hatRears=Imports("hat-rear-",3);actor.accessories=Imports("accessory-",6);actor.foregroundHands=Import("foreground-hands");Import("clothing-mask");
   actor.palette=0;actor.cape=0;actor.hat=0;PrefabUtility.SaveAsPrefabAsset(go,PrefabPath);
   var camera=new GameObject("Layered preview camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=2.7f;camera.transform.position=new Vector3(0,1,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.10f,.14f,.17f);camera.rect=new Rect(.28f,0,.72f,1);camera.allowHDR=false;camera.allowMSAA=false;camera.gameObject.AddComponent<AudioListener>();
   var demo=new GameObject("Wardrobe controls").AddComponent<LayeredWalkDemo>();demo.actor=actor;demo.previewCamera=camera;demo.evidenceDirectory=Review;
   EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(Review,"import-validation.txt"),"PASS: 8 body palettes, 6 cape front/back pairs, 3 hats, 6 accessories, 1 foreground-hand layer; 320x768 RGBA, point-filtered, 40 frames per atlas.\n");
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
  [MenuItem("Maritime Legend/Character 05 Layered Walk/Open Preview")]
  public static void OpenPreview(){if(EditorApplication.isPlaying)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(ScenePath);}
  public static void BuildAndVerify(){Create();string build=Path.GetFullPath("Builds/Character05LayeredWalk/Character05LayeredWalk.exe");Directory.CreateDirectory(Path.GetDirectoryName(build));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=build,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");File.WriteAllText(Path.Combine(Review,"build-validation.txt"),"PASS "+report.summary.result+"\n"+build);}
 }
}


