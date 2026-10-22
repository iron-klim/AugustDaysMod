using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AugustDaysMod.Catalog;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;
using HarmonyLib;
using UnityEngine;

// Инструменты разработки. Включаются файлами в папке данных игры (%USERPROFILE%\AppData\LocalLow\Nostalgames\CrisisInTheKremlin2):
// august_player_country — в файле имя страны из Countries, за которую начнется новая игра (например, Ukraine);
// august_debug_log — раз в месяц писать в Player.log состояние Союза, республик и показателей игрока
namespace AugustDaysMod.DevTools
{
    public static class DevSettings
    {
        public const string LOG_TAG = "[AugustDays]";

        public static Countries? StartCountry
        {
            get
            {
                string path = Path.Combine(Application.persistentDataPath, "august_player_country");
                if (!File.Exists(path))
                {
                    return null;
                }

                string name = File.ReadAllText(path).Trim();
                if (Enum.TryParse(name, out Countries country))
                {
                    return country;
                }

                Debug.LogWarning($"{LOG_TAG} в august_player_country незнакомая страна «{name}»");
                return null;
            }
        }

        public static bool DebugLog => File.Exists(Path.Combine(Application.persistentDataPath, "august_debug_log"));

        public static void Log(string message) => Debug.Log($"{LOG_TAG} {message}");
    }

    // Переключаем страну игрока уже после всех приготовлений сценария, в том числе нашей Setup
    [HarmonyPatch(typeof(GameStartUtils), nameof(GameStartUtils.MakeFinalScenarioPreparations))]
    public static class PatchStartCountry
    {
        static void Postfix()
        {
            Countries? country = DevSettings.StartCountry;
            GameState gameState = GameStateManager.inst.GameState;
            if (!country.HasValue || country.Value == gameState.playerCountry)
            {
                return;
            }

            PlayerCountry.Switch(gameState, country.Value);
            DevSettings.Log($"игрок начинает за {gameState.playerCountry}, СССР под управлением ИИ");
        }
    }

    [HarmonyPatch(typeof(CountryUtils), nameof(CountryUtils.TickCountries))]
    public static class PatchMonthlyDebugLog
    {
        private static readonly Countries[] Republics =
        {
            Countries.Russia, Countries.Ukraine, Countries.Belorussia, Countries.Moldavia, Countries.Litva,
            Countries.Latvia, Countries.Estonia, Countries.Kazakhstan, Countries.Uzbekistan, Countries.Kyrgizia,
            Countries.Tajikistan, Countries.Turkmenia, Countries.Armenia, Countries.Azerbaijan, Countries.Georgia
        };

        // После союзного тика: так в журнал попадает уже обновленное состояние
        [HarmonyPriority(Priority.Last)]
        static void Postfix(GameState gameState)
        {
            if (!DevSettings.DebugLog)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.Append($"месяц {gameState.date:dd.MM.yyyy}, игрок {gameState.playerCountry}\n");
            builder.Append($"  показатели игрока: резерв={gameState.Reserve} армия={gameState.ArmyQuantityLevel:F1} ");
            builder.Append($"боеголовки={gameState.WarheadsQuantity:F1} ПВ={gameState.PoliticalPower:F0}\n");
            builder.Append("  ").Append(Describe(gameState, Countries.USSR)).Append('\n');
            foreach (Countries republic in Republics)
            {
                builder.Append("  ").Append(Describe(gameState, republic)).Append('\n');
            }

            builder.Append($"  субъектов СССР: {SovietUnion.Subjects(gameState).Count()}, республик игрока: {CountryUtils.GetNumberOfRepublicsOfPlayer(gameState)}");
            DevSettings.Log(builder.ToString());
        }

        private static string Describe(GameState gameState, Countries key)
        {
            if (!gameState.countries.TryGetValue(key, out Country country))
            {
                return $"{key}: нет";
            }

            return $"{key}: puppet={country.puppetType}/{country.puppetOf?.ToString() ?? "-"} gov={country.governmentType} " +
                   $"leader={country.leaderType} party={country.rulingPartyName} кИгроку={country.RelationshipWithPlayer:F0} " +
                   $"кСоюзу={UnionRelations.Get(country):F0} вектор={country.ForeignPolicyVector:F0}";
        }
    }

    // Из чего складывается изменение резерва за месяц
    [HarmonyPatch(typeof(EconomyUtils), nameof(EconomyUtils.TickEconomy))]
    public static class PatchEconomyDebugLog
    {
        static void Postfix(GameState gameState)
        {
            if (!DevSettings.DebugLog)
            {
                return;
            }

            var income = gameState.gameStateHistory.lastIncome;
            string incomeText = string.Join(", ", income.Where(p => p.Value != 0).OrderByDescending(p => Math.Abs(p.Value)).Take(8).Select(p => $"{p.Key}={p.Value}"));
            string expensesText = string.Join(", ", gameState.expenses.Where(p => p.Value != 0).OrderByDescending(p => Math.Abs(p.Value)).Take(8).Select(p => $"{p.Key}={p.Value}"));
            DevSettings.Log($"экономика {gameState.date:dd.MM.yyyy}: резерв={gameState.Reserve} цены={gameState.PriceIndex:F2} " +
                            $"доходы={income.Values.Sum()} расходы={gameState.expenses.Values.Sum()}\n  доходы: {incomeText}\n  расходы: {expensesText}");
        }
    }

    // Какие события доходят до игрока
    [HarmonyPatch]
    public static class PatchEventsDebugLog
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(EventManager), nameof(EventManager.GetNewEvent));
            yield return AccessTools.Method(typeof(EventManager), nameof(EventManager.GetNewCrisisEvent));
            yield return AccessTools.Method(typeof(EventManager), nameof(EventManager.GetNewSuperEvent));
        }

        static void Postfix(GameState gameState, int? __result)
        {
            if (__result.HasValue && DevSettings.DebugLog)
            {
                DevSettings.Log($"событие {__result.Value} {gameState.date:dd.MM.yyyy}");
            }
        }
    }
}
