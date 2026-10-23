using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Endings.GameEndings;
using HarmonyLib;

namespace AugustDaysMod.Catalog
{
    // «Военное поражение» наступает, когда у сверхдержавы почти не остается армии или боеголовок,
    // и заканчивается интервенцией НАТО. Для зависимой страны из справочника такой исход не имеет смысла
    [HarmonyPatch(typeof(Ending15), nameof(Ending15.TestConditions))]
    public static class PatchMilitaryDefeatForDependentCountries
    {
        static void Postfix(GameState gameState, ref bool __result)
        {
            if (__result && CountryCatalog.IsDependent(gameState))
            {
                __result = false;
            }
        }
    }
}
