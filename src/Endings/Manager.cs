using System.Collections.Generic;
using Backend.Gamedesign.EndingsSystem.Endings;
using Backend.Gamedesign.EndingsSystem.Utils;
using HarmonyLib;

namespace AugustDaysMod.Endings
{
    [HarmonyPatch(typeof(EndingsManager), "OrganizeEndings")]
    public static class EndingsManagerOrganizeEndings
    {
        static void Postfix(EndingsManager __instance)
        {
            var endingsField = AccessTools.Field(typeof(EndingsManager), "endings");
            var endingsDict = (Dictionary<int, AbstractEnding>)endingsField.GetValue(__instance);

            endingsDict.Add(LiberalVictory.ID, new LiberalVictory());
        }
    }
}