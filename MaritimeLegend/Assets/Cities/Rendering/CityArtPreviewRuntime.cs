using System;using System.Collections;using System.IO;using System.Linq;using UnityEngine;using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class CityArtPreviewRuntime : MonoBehaviour {
 public static bool Active;int city;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){Active=false;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Bootstrap(){int chosen=-1;if(Environment.GetCommandLineArgs().Contains("-cityArtSmoke"))chosen=0;
#if UNITY_EDITOR
 int editorChoice=UnityEditor.SessionState.GetInt("MaritimeLegend.CityArtPreview",-1);if(editorChoice>=0){chosen=editorChoice;UnityEditor.SessionState.SetInt("MaritimeLegend.CityArtPreview",-1);}
#endif
 if(chosen<0)return;Active=true;var go=new GameObject("CityArtPreviewSession");DontDestroyOnLoad(go);var run=go.AddComponent<CityArtPreviewRuntime>();run.city=chosen;}
 IEnumerator Start(){yield return null;var world=FindAnyObjectByType<WorldSession>();var game=FindAnyObjectByType<MaritimeGame>();SaveStore.TestDirectory=Path.Combine(Application.temporaryCachePath,"CityArtPreview",Guid.NewGuid().ToString("N"));world.State=GameService.New(world.Catalog,6,city,"休闲",new Appearance{baseId=1},"海岚",71527);world.State.pendingEvents.Clear();world.Service=new GameService(world.Catalog,world.State);game.Change("城市");}
}
}
