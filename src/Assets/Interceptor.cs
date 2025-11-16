using System.IO;
using System.Linq;
using System.Reflection;
using AugustDaysMod.Utils;
using Backend.Gamedesign;
using Backend.Managers;
using HarmonyLib;
using Newtonsoft.Json;

// Перехватываем вызовы игры на загрузку ресурсов и загружаем свои
namespace AugustDaysMod.Assets
{
    [HarmonyPatch(typeof(GameStateManager), "LoadGameRules")]
    public static class PatchGameStateManagerLoadGameRules
    {
      static bool Prefix(GameStateManager __instance)
      {
        string text = File.ReadAllText(Interceptor.GetPath("rules.json"));
        
        JsonSerializerSettings settings = (JsonSerializerSettings)ReflectionUtils.getFieldValue(__instance, "jsonSettings");
        
        GameRules rules = JsonConvert.DeserializeObject<GameRules>(text, settings);
        
        ReflectionUtils.SetFieldValue(__instance, "gameRules", rules);
        
        return false;
      }
    }
    
    [HarmonyPatch(typeof(GameStateManager), "LoadGameScenario")]
    public static class PatchGameStateManagerLoadGameScenario
    {
      static bool Prefix(GameStateManager __instance, ScenarioType type)
      {
        string text = File.ReadAllText(Interceptor.GetPath("scenario.json"));

        var settings = (JsonSerializerSettings)ReflectionUtils.getFieldValue(__instance, "jsonSettings");

        GameScenario scenario = JsonConvert.DeserializeObject<GameScenario>(text, settings);

        ReflectionUtils.SetFieldValue(__instance, "gameScenario", scenario);
        
        PortraitsManager.inst.LoadPortraits(scenario.portraitsInfo);

        return false;
      }
    }
    
    [HarmonyPatch(typeof(GameStateManager), "LoadStartGameState")]
    public static class PatchGameStateManagerLoadStartGameState
    {
      static bool Prefix(GameStateManager __instance, ScenarioType type)
      {
        string text = File.ReadAllText(Interceptor.GetPath("state.json"));

        var settings = (JsonSerializerSettings)ReflectionUtils.getFieldValue(__instance, "jsonSettings");

        GameState state = JsonConvert.DeserializeObject<GameState>(text, settings);

        ReflectionUtils.SetFieldValue(__instance, "gameState", state);
        
        var loadScenarioMethod = typeof(GameStateManager).GetMethod("LoadGameScenario", BindingFlags.NonPublic | BindingFlags.Instance);
        loadScenarioMethod.Invoke(__instance, new object[] { type });
        
        return false;
      }
    }

    public static class Interceptor
    {
      public static string GetPath(string item)
      {
        var currentDir = Directory.GetCurrentDirectory();
        
        string[] parts = currentDir.Split(Path.DirectorySeparatorChar);
        
        var basePath = string.Join(Path.DirectorySeparatorChar.ToString(), parts.Take(parts.Length - 2));
        
        return Path.Combine(
          basePath,
          "workshop",
          "content",
          ModEntryPoint.GAME_ID.ToString(),
          ModEntryPoint.MOD_ID.ToString(),
          item
        );
      }
    }
}
