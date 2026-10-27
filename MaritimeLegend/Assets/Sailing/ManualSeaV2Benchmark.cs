using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.Profiling;
namespace MaritimeLegend {
[DefaultExecutionOrder(1000)]
public sealed class ManualSeaV2Benchmark : MonoBehaviour {
 public ManualSeaV2Environment environment;public ManualSeaV2Camera orbit;public ManualSeaV2Instances instances;
 public Vector3[] sampleFocuses;
 RenderTexture target;Texture2D syncPixel;
 void LateUpdate(){if(!target)return;var camera=orbit.GetComponent<Camera>();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;syncPixel.ReadPixels(new Rect(0,0,1,1),0,0,false);RenderTexture.active=previous;}
 void Capture(string path){var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);RenderTexture.active=previous;}
 IEnumerator Start(){
  string[] args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-seaV2Benchmark");if(flag<0)yield break;
  string output=flag+1<args.Length?args[flag+1]:Application.persistentDataPath;Directory.CreateDirectory(output);
  QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;Application.runInBackground=true;environment.showControls=false;
  target=new RenderTexture(1920,1080,24){antiAliasing=2};target.Create();syncPixel=new Texture2D(1,1,TextureFormat.RGB24,false);var camera=orbit.GetComponent<Camera>();camera.targetTexture=target;camera.aspect=1920f/1080;camera.enabled=false;
  var report=new StringBuilder();report.AppendLine("Manual Sailing V2 standalone development-player benchmark");report.AppendLine($"Device: {SystemInfo.graphicsDeviceName}; CPU: {SystemInfo.processorType}");report.AppendLine($"API: {SystemInfo.graphicsDeviceType}; Resolution: {Screen.width}x{Screen.height}; Instancing: {SystemInfo.supportsInstancing}; VSync: {QualitySettings.vSyncCount}");report.AppendLine("300 frames per sample after 90 warmup frames; camera path moves during measurement. Frame time is wall-clock player time, not isolated GPU time.");
  report.AppendLine("Forced 1920x1080 offscreen Camera.Render; a 1-pixel GPU readback every frame prevents skipped rendering in hidden windows. Results include GPU synchronization/readback overhead; they are not interactive-window FPS.");
  var draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);var tris=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count",1);
  try {
   for(int mode=0;mode<3;mode++)for(int pose=0;pose<3;pose++){
    environment.mode=(ManualSeaV2Environment.SkyMode)mode;environment.Apply();
    orbit.pitch=35;orbit.yaw=pose==1?70:0;orbit.distance=pose==1?220:115;orbit.focus=sampleFocuses!=null&&sampleFocuses.Length==3?sampleFocuses[pose]:pose==1?new Vector3(-40,0,130):new Vector3(0,0,-40);orbit.ApplyPose();
    for(int warm=0;warm<90;warm++)yield return null;
    var frames=new List<float>();long drawSum=0,triangleSum=0;int visible=0;
    for(int n=0;n<300;n++){orbit.yaw+=.035f;yield return null;frames.Add(Time.unscaledDeltaTime*1000);drawSum+=draws.Valid?draws.LastValue:0;triangleSum+=tris.Valid?tris.LastValue:0;visible+=instances.VisibleInstances;}
    frames.Sort();float sum=0;foreach(var f in frames)sum+=f;
    report.AppendLine($"{environment.mode} / {(pose==0?"near-35":pose==1?"far-north-35":"horizon-35")}: avg={sum/300:F2}ms ({300000/sum:F1} FPS), median={frames[150]:F2}ms, p95={frames[284]:F2}ms; avg draws={(draws.Valid?(drawSum/300).ToString():"unavailable")}; triangles={(tris.Valid?(triangleSum/300).ToString():"unavailable")}; visible props={visible/300}");
    Capture(Path.Combine(output,$"runtime-{mode}-{pose}.png"));for(int wait=0;wait<15;wait++)yield return null;
   }
   report.AppendLine($"Unity allocated memory: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576} MiB");
   File.WriteAllText(Path.Combine(output,"performance.txt"),report.ToString());Debug.Log("V2_BENCHMARK_PASS\n"+report);Application.Quit(0);
  } finally {draws.Dispose();tris.Dispose();camera.targetTexture=null;camera.enabled=true;target.Release();Destroy(target);Destroy(syncPixel);}
 }
}
}


