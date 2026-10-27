using UnityEngine;
namespace MaritimeLegend {
public sealed class ManualSeaV2Window : MonoBehaviour {
 System.Collections.IEnumerator Start(){yield return null;yield return null;Debug.Log($"V2_WINDOW_STATE: {Screen.width}x{Screen.height}; mode={Screen.fullScreenMode}");}
 void Awake(){
#if UNITY_STANDALONE && !UNITY_EDITOR
  Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
#endif
 }
}
}

