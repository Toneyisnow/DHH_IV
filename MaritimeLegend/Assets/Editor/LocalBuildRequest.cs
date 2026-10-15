using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace MaritimeLegend.Editor {
[InitializeOnLoad] public static class LocalBuildRequest {
static LocalBuildRequest(){EditorApplication.update+=Update;}
static void Update(){if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;const string request="Documentation/build.request";if(!File.Exists(request))return;File.Delete(request);EditorApplication.delayCall+=()=>{try{BuildGame.Build();File.WriteAllText("Documentation/BuildStatus.txt","SUCCESS "+DateTime.UtcNow.ToString("O"));}catch(Exception e){File.WriteAllText("Documentation/BuildStatus.txt",e.ToString());Debug.LogException(e);}};}
}}
