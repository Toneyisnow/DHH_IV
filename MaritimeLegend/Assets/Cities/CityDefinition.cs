using UnityEngine;
namespace MaritimeLegend {
[CreateAssetMenu(menuName="Maritime Legend/City Definition")]
public sealed class CityDefinition : ScriptableObject {
 public int cityId; public string cityKey,displayName;
 public Texture2D background; public GameObject contentRoot;
 public string weather="港湾晴朗",musicTheme="城市";
 public string[] eventIds;
 public Texture2D buildingAtlas,walkerAtlas;public TextAsset buildingRects,walkerRects;public Shader spriteShader,depthShader,waterShader,surfaceShader,atmosphereShader;public bool refined;
 public Color palette=new Color(.8f,.7f,.5f);
}
}
