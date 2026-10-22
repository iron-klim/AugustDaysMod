using AugustDaysMod.Assets;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;
using HarmonyLib;
using UnityEngine;

namespace AugustDaysMod.Catalog
{
    public static class PlayerCountry
    {
        // Игрок покидает Союз и берет на себя управление другой страной из справочника
        public static void Switch(GameState gameState, Countries country)
        {
            CountryProfile profile = CountryCatalog.Get(country);
            if (profile == null)
            {
                Debug.LogWarning($"[AugustDays] страны {country} нет в справочнике {CountryCatalog.FILE_NAME}, ее показатели останутся союзными");
            }

            if (gameState.playerCountry == Countries.USSR)
            {
                UnionRelations.CaptureFromPlayer(gameState);
            }

            UnitOfWork.StartCreation()
                .AddChangePuppetType(gameState, country, Countries.USSR, Country.PuppetType.None)
                .AddPlayerCountryChanged(gameState, country)
                .Execute(gameState);

            CountryCatalog.ApplyIndicators(gameState, profile, Interceptor.JsonSettingsRef(GameStateManager.inst));
            gameState.Population = CountryUtils.GetPlayersPopulation(gameState);
        }
    }

    // Базовый доход страны игрока берем из справочника: сценарий игры знает только союзный
    [HarmonyPatch(typeof(EconomyUtils), nameof(EconomyUtils.CalculateCurrentIncome))]
    public static class PatchCalculateCurrentIncome
    {
        static void Prefix(GameState gameState, GameScenario gameScenario, out int __state)
        {
            __state = gameScenario.baseIncomeValue;
            int? baseIncome = CountryCatalog.Get(gameState.playerCountry)?.baseIncome;
            if (baseIncome.HasValue)
            {
                gameScenario.baseIncomeValue = baseIncome.Value;
            }
        }

        static void Postfix(GameScenario gameScenario, int __state) => gameScenario.baseIncomeValue = __state;
    }

    // Экономику страны игрока игра считает в союзном эквиваленте: все формулы (инфляция, доходы, влияние населения
    // на показатели) подобраны под население Союза. Поэтому формулам отдаем население, деленное на экономический вес,
    // а бюджет и резерв в справочнике задаются в тех же союзных единицах. Настоящее население остается в регионах
    [HarmonyPatch(typeof(GameState), nameof(GameState.Population), MethodType.Getter)]
    public static class PatchPopulationGetter
    {
        static void Postfix(GameState __instance, ref int __result)
        {
            float scale = CountryCatalog.EconomyScale(__instance);
            if (scale != 1f)
            {
                __result = Mathf.RoundToInt(__result / scale);
            }
        }
    }
}
