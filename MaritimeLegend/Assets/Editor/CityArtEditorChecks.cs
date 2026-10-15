using System;
using UnityEditor;
using UnityEngine;
namespace MaritimeLegend.Editor {
[InitializeOnLoad] public static class CityArtEditorChecks {
 const string Key="MaritimeLegend.CityArtEditorCheck";static double started;
 static CityArtEditorChecks(){EditorApplication.update+=Update;EditorApplication.playModeStateChanged+=Changed;}
 public static void Verify(){SessionState.SetInt(Key,1);started=EditorApplication.timeSinceStartup;CityArtSetup.PreviewLisbon();}
 static void Changed(PlayModeStateChange mode){if(SessionState.GetInt(Key,0)==0)return;if(mode==PlayModeStateChange.EnteredPlayMode)started=EditorApplication.timeSinceStartup;if(mode==PlayModeStateChange.EnteredEditMode&&SessionState.GetInt(Key,0)==3){SessionState.SetInt(Key,0);Debug.Log("CITY_ART_EDITOR_PASS");EditorApplication.Exit(0);}}
 static void Update(){int phase=SessionState.GetInt(Key,0);if(phase==0||phase==3||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup-started<6)return;try{var city=UnityEngine.Object.FindAnyObjectByType<CityScene>();if(city==null||city.Diorama==null||city.Diorama.Player==null||!CityArtPreviewRuntime.Active)throw new Exception("City art editor preview missing");int expected=phase==1?0:2;if(city.Current.cityId!=expected)throw new Exception("Unexpected preview city");foreach(var point in city.Diorama.Landmarks)if(!city.Diorama.HasPath(city.Diorama.Player.position,point.Value))throw new Exception("Unreachable landmark "+point.Key);Debug.Log("CITY_ART_EDITOR_CITY_PASS "+city.Current.cityKey);if(phase==1){SessionState.SetInt(Key,2);CityArtSetup.PreviewVenice();started=EditorApplication.timeSinceStartup;}else{SessionState.SetInt(Key,3);EditorApplication.ExitPlaymode();}}catch(Exception e){Debug.LogException(e);SessionState.SetInt(Key,0);EditorApplication.Exit(1);}}
}
}
