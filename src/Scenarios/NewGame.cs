using System;
using Backend.Gamedesign;
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
            // Игра показывает набор политиков выбранного года. Если не показан ни один, значит выбран старт мода
            if (!Array.Exists(__instance.politicSets, set => set.activeSelf))
            {
                return;
            }

            ScenarioType politicSet = ResolvePoliticSet(__instance, ScenarioType.USSR1991);
            int setIndex = Array.IndexOf(__instance.politicSetsScenarios, politicSet);
            if (setIndex < 0 || setIndex >= __instance.politicSets.Length)
            {
                return;
            }

            for (int index = 0; index < __instance.politicSets.Length; ++index)
            {
                // Вместо политиков выбранного года всегда показываем политиков 1991 года
                __instance.politicSets[index].SetActive(index == setIndex);
            }
        }

        private static ScenarioType ResolvePoliticSet(NewGameController controller, ScenarioType scenario)
        {
            if (controller.politicSetAliases != null)
            {
                foreach (NewGameController.PoliticSetAlias alias in controller.politicSetAliases)
                {
                    if (alias != null && alias.scenario == scenario)
                    {
                        return alias.useSetOf;
                    }
                }
            }

            return scenario;
        }
    }
}
