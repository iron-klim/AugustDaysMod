using System.IO;
using System.Reflection;
using Backend.Gamedesign;
using Backend.Managers;
using HarmonyLib;
using Newtonsoft.Json;

// Перехватываем вызовы игры на загрузку ресурсов и загружаем свои
namespace AugustDaysMod.Assets
{
    // Сценарий загружает игра, а мы поверх меняем только те поля, что заданы в scenario.json
    [HarmonyPatch(typeof(GameStateManager), "LoadGameScenario")]
    public static class PatchGameStateManagerLoadGameScenario
    {
      static void Postfix(GameStateManager __instance)
      {
        JsonConvert.PopulateObject(File.ReadAllText(Interceptor.GetPath("scenario.json")), __instance.GameScenario, Interceptor.JsonSettingsRef(__instance));
      }
    }

    // У метода две перегрузки: LoadStartGameState(type) просто вызывает LoadStartGameState(type, files), поэтому патчим вторую
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.LoadStartGameState), typeof(ScenarioType), typeof(ScenarioType))]
    public static class PatchGameStateManagerLoadStartGameState
    {
      static readonly AccessTools.FieldRef<GameStateManager, GameState> GameStateRef =
        AccessTools.FieldRefAccess<GameStateManager, GameState>("gameState");

      // Вызывается раз за новую игру, поэтому хватает обычного Invoke.
      // AccessTools.MethodDelegate не используем: в игре может быть загружен Harmony старее, чем тот, с которым собирается мод
      static readonly MethodInfo LoadGameScenario = AccessTools.Method(typeof(GameStateManager), "LoadGameScenario");

      static bool Prefix(GameStateManager __instance, ScenarioType type, ScenarioType files)
      {
        GameState state = JsonConvert.DeserializeObject<GameState>(File.ReadAllText(Interceptor.GetPath("state.json")), Interceptor.JsonSettingsRef(__instance));
        state.scenarioType = type;
        state.customScenarioBase = files;
        GameStateRef(__instance) = state;

        LoadGameScenario.Invoke(__instance, new object[] { type });

        return false;
      }
    }

    public static class Interceptor
    {
      internal static readonly AccessTools.FieldRef<GameStateManager, JsonSerializerSettings> JsonSettingsRef =
        AccessTools.FieldRefAccess<GameStateManager, JsonSerializerSettings>("jsonSettings");

      private static string modFolder;

      public static string GetPath(string item)
      {
        if (modFolder == null)
        {
          // Папку мода знает менеджер модов игры: это может быть и папка Steam Workshop, и локальная папка mods
          ModsManager.ModEntry entry = ModsManager.inst != null ? ModsManager.inst.GetModInfo(ModEntryPoint.MOD_INFO_ID) : null;
          modFolder = entry != null
            ? entry.pathToModFolder
            : Path.GetDirectoryName(typeof(Interceptor).Assembly.Location);
        }

        return Path.Combine(modFolder, item);
      }
    }
}
