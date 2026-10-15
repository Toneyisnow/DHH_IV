using UnityEngine;
using UnityEngine.SceneManagement;
namespace MaritimeLegend {
public static class SceneNavigator {
 public static readonly string[] Names={"MainMenuScene","SaveLoadScene","CharacterCreationScene","CityScene","IndustryScene","SailingScene","AutoSailingScene","WorldMapScene"};
 public static string ForPage(string page){switch(page){case "标题":return Names[0];case "存档":return Names[1];case "模板":case "定制":return Names[2];case "产业园":case "仓库":return Names[4];case "航海":case "战斗":return Names[5];case "自动航线":case "航线配置":return Names[6];case "海图":return Names[7];default:return Names[3];}}
 public static bool Ensure(string page){string target=ForPage(page);if(SceneManager.GetActiveScene().name==target)return true;SceneManager.LoadScene(target,LoadSceneMode.Single);return false;}
 public static void Render(MaritimeGame game){var controller=Object.FindAnyObjectByType<FunctionalScene>();if(controller!=null)controller.Render(game);}
}
public abstract class FunctionalScene : MonoBehaviour {public abstract void Render(MaritimeGame game);}
}
