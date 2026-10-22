using System.Collections.Generic;
using Backend.Gamedesign;
using Backend.Gamedesign.Utils;
using HarmonyLib;

namespace AugustDaysMod.Scenarios
{
    // В августе 1991 советские войска еще стоят в Польше (Северная группа войск) и Германии (Западная группа войск).
    // Игра каждый тик снимает советскую базу в стране вне соцлагеря, а в стране НАТО заменяет ее американской.
    // Сохраняем эти гарнизоны, пока игрок сам не выведет войска
    [HarmonyPatch(typeof(CountryUtils), nameof(CountryUtils.TickCountries))]
    public static class PatchCountryUtilsTickCountries
    {
        private static readonly Countries[] Garrisons = { Countries.Poland, Countries.UnitedGermany };

        static void Prefix(GameState gameState, out List<Countries> __state)
        {
            __state = new List<Countries>(Garrisons.Length);
            foreach (Countries country in Garrisons)
            {
                if (gameState.countries.TryGetValue(country, out Country value) && value.hasBase == Country.BasesType.Soviet)
                {
                    __state.Add(country);
                }
            }
        }

        static void Postfix(GameState gameState, List<Countries> __state)
        {
            foreach (Countries country in __state)
            {
                Country value = gameState.countries[country];
                if (value.activeInGame)
                {
                    value.hasBase = Country.BasesType.Soviet;
                }
            }
        }
    }
}
