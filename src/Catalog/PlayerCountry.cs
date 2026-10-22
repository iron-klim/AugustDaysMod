using System;
using System.Collections.Generic;
using AugustDaysMod.Assets;
using AugustDaysMod.Compat;
using AugustDaysMod.DevTools;
using AugustDaysMod.Storage;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.Utils;
using Backend.Managers;
using HarmonyLib;
using UnityEngine;

namespace AugustDaysMod.Catalog
{
    public static class PlayerCountry
    {
        // Новая игра за страну из справочника. Состояние игры к этому моменту — союзное на 16.08.1991:
        // отделяем страну игрока от Союза и передаем Союз под управление ИИ
        public static void SetupStart(GameState gameState)
        {
            Countries country = gameState.playerCountry;
            CountryProfile profile = CountryCatalog.Get(country);
            if (profile == null)
            {
                Debug.LogWarning($"[AugustDays] страны {country} нет в справочнике {CountryCatalog.FILE_NAME}, ее показатели останутся союзными");
            }

            // В стартовом состоянии отношения республик к игроку — это отношения к союзному центру
            UnionRelations.CaptureFromPlayer(gameState);
            UnionData union = SovietUnion.Data(gameState);
            union.active = true;
            // Середина августа 1991: Союз еще держится, но новый союзный договор подписан не будет
            union.cohesion = 45f;

            Country player = gameState.countries[country];
            player.puppetOf = null;
            player.puppetType = Country.PuppetType.None;
            player.leaderType = Country.LeaderType.None;
            gameState.countries[Countries.USSR].RelationshipWithPlayer = 50f;
            foreach (KeyValuePair<Countries, Country> subject in SovietUnion.Subjects(gameState))
            {
                subject.Value.RelationshipWithPlayer = 50f;
            }

            if (profile != null)
            {
                foreach (KeyValuePair<Countries, float> relation in profile.relations ?? new Dictionary<Countries, float>())
                {
                    if (gameState.countries.TryGetValue(relation.Key, out Country other))
                    {
                        other.RelationshipWithPlayer = relation.Value;
                    }
                }

                CountryCatalog.ApplyIndicators(gameState, profile, Interceptor.JsonSettingsRef(GameStateManager.inst));
                CountryLeadership.Apply(gameState, profile);
            }

            gameState.conspiracies.Clear();
            NonUnionPlayer.CleanStartState(gameState);
            CountryUtils.RefreshPlayerGovernmentType(gameState);
            gameState.Population = CountryUtils.GetPlayersPopulation(gameState);
            Debug.Log($"[AugustDays] новая игра за {country}, правитель {gameState.gameCharacter}, СССР под управлением ИИ");
        }
    }

    // Старт за другую страну: игра берет союзный сценарий 16.08.1991, а правителя — из справочника
    [HarmonyPatch(typeof(GameStartUtils), nameof(GameStartUtils.LoadGameStateAndGameRulesFromStartSettings),
        typeof(ScenarioType), typeof(Difficulty), typeof(Characters), typeof(bool), typeof(Countries), typeof(List<Cheats>), typeof(int), typeof(string))]
    public static class PatchLoadGameStateForCountry
    {
        static void Prefix(ref Countries country, ref Characters character)
        {
            Countries? devCountry = DevSettings.StartCountry;
            if (devCountry.HasValue)
            {
                country = devCountry.Value;
            }

            if (country == Countries.USSR)
            {
                return;
            }

            Characters? ruler = CountryLeadership.Resolve(CountryCatalog.Get(country)?.ruler);
            if (ruler.HasValue && Enum.IsDefined(typeof(Characters), ruler.Value))
            {
                character = ruler.Value;
            }
        }
    }

    [HarmonyPatch(typeof(GameStartUtils), nameof(GameStartUtils.MakeFinalScenarioPreparations))]
    public static class PatchMakeFinalScenarioPreparationsForCountry
    {
        static void Postfix()
        {
            GameState gameState = GameStateManager.inst.GameState;
            if (gameState.playerCountry != Countries.USSR)
            {
                PlayerCountry.SetupStart(gameState);
            }
        }
    }

    // Сценарии игры есть только для СССР: страны из справочника играют союзный сценарий мода
    [HarmonyPatch(typeof(GameStartUtils), nameof(GameStartUtils.ScenarioFromYearAndCountry))]
    public static class PatchScenarioFromYearAndCountry
    {
        static bool Prefix(Countries country, int year, ref ScenarioType __result)
        {
            if (country == Countries.USSR || year == 8888 || !CountryCatalog.IsSupported(country))
            {
                return true;
            }

            __result = ScenarioType.USSR1985;
            return false;
        }
    }

    // Название страны игрока и его сокращение — из справочника
    [HarmonyPatch(typeof(CountryUtils), nameof(CountryUtils.GetCountryPlayerName))]
    public static class PatchGetCountryPlayerName
    {
        static void Postfix(GameState gameState, Countries country, ref (string, string) __result)
        {
            if (country != gameState.playerCountry || country == Countries.USSR)
            {
                return;
            }

            CountryProfile profile = CountryCatalog.Get(country);
            if (!string.IsNullOrEmpty(profile?.fullName))
            {
                __result = (profile.fullName, string.IsNullOrEmpty(profile.abbreviation) ? profile.fullName : profile.abbreviation);
            }
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
