using Frontend.NewGame;
using HarmonyLib;

// Разработчики сделали заготовки для сценария на 1991 год и мы этим пользуемся
namespace AugustDaysMod.Scenarios
{
    [HarmonyPatch(typeof(NewGameController), nameof(NewGameController.Repaint))]
    public static class PatchNewGameControllerRepaint
    {
        static void Postfix(NewGameController __instance)
        {
            __instance.politicSets[0].SetActive(false); // Отключаем политиков 1985 года
            __instance.politicSets[1].SetActive(true); // Включаем политиков 1991 года
        }
    }
}
