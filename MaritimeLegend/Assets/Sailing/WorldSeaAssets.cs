using UnityEngine;
namespace MaritimeLegend {
internal static class WorldSeaAssets {
 // Only used for objects owned by the world runtime, never imported source assets.
 public static void Destroy(Object obj){if(!obj)return;if(Application.isPlaying)Object.Destroy(obj);else Object.DestroyImmediate(obj);}
}
}
