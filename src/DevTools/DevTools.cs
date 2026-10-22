using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AugustDaysMod.Catalog;
using AugustDaysMod.Story;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Gamedesign.Utils;
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

        // События мода решаются сами первым вариантом — для долгих проверочных прогонов
        public static bool AutoPlay => File.Exists(Path.Combine(Application.persistentDataPath, "august_autoplay"));

        public static void Log(string message) => Debug.Log($"{LOG_TAG} {message}");
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

            builder.Append($"  субъектов СССР: {SovietUnion.Subjects(gameState).Count()}, республик игрока: {CountryUtils.GetNumberOfRepublicsOfPlayer(gameState)}\n");
            var alive = gameState.characters.Where(c => c.Value.status == CharacterInfo.PoliticStatus.Alive).ToList();
            var foreign = alive.Where(c => CharacterUtils.GetHomeLandCountry(gameState, c.Value) != gameState.playerCountry).Select(c => c.Key.ToString()).ToList();
            builder.Append($"  политиков: {alive.Count}, не из страны игрока: {foreign.Count} {string.Join(" ", foreign.Take(10))}\n");
            List<int> decisions = Backend.Gamedesign.DecisionsSystem.Utils.DecisionsManager.inst.GetDecisionsIdsForCountry(gameState);
            builder.Append($"  решения игрока: {string.Join(", ", decisions)}");
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
                   $"кСоюзу={UnionRelations.Get(gameState, key):F0} вектор={country.ForeignPolicyVector:F0}";
        }
    }

    // В автопрогоне вакансии заполняются сами: самым влиятельным свободным политиком
    [HarmonyPatch(typeof(TickUtils), "ProcessTickCore")]
    public static class PatchAutoPlayVacancies
    {
        static void Postfix(GameState gameState)
        {
            if (!DevSettings.AutoPlay)
            {
                return;
            }

            foreach (PoliticPosition position in Enum.GetValues(typeof(PoliticPosition)))
            {
                if (gameState.politicPositions[position].HasValue || !gameState.IfPositionPossible(position))
                {
                    continue;
                }

                KeyValuePair<Characters, CharacterInfo> candidate = gameState.characters
                    .Where(c => c.Value.status == CharacterInfo.PoliticStatus.Alive && c.Key != gameState.gameCharacter &&
                                !gameState.politicPositions[c.Key].HasValue)
                    .OrderByDescending(c => c.Value.Power)
                    .FirstOrDefault();
                if (candidate.Value == null)
                {
                    DevSettings.Log($"автоназначение: на пост {position} некого назначить");
                    continue;
                }

                CountryLeadership.Assign(gameState, candidate.Key, position);
                DevSettings.Log($"автоназначение {gameState.date:dd.MM.yyyy}: {PoliticUtils.GetPoliticFullName(gameState, candidate.Key)} — {position}");
            }
        }
    }

    // Из чего складывается изменение политической власти за месяц
    [HarmonyPatch(typeof(TickUtils), "ProcessTickCore")]
    public static class PatchPoliticalPowerDebugLog
    {
        static void Postfix(GameState gameState)
        {
            if (gameState.date.Day != 1 || !DevSettings.DebugLog)
            {
                return;
            }

            GameStateHistory history = gameState.gameStateHistory;
            string sources = history.lastValuesChangesSources.TryGetValue("PoliticalPower", out var bySource)
                ? string.Join(", ", bySource.Where(p => Math.Abs(p.Value) >= 0.5f).OrderBy(p => p.Value).Select(p => $"{p.Key}={p.Value:F0}"))
                : "-";
            string inputs = history.lastValuesChangesByInput.TryGetValue("PoliticalPower", out var byInput)
                ? string.Join(", ", byInput.Where(p => Math.Abs(p.Value) >= 0.5f).OrderBy(p => p.Value).Take(14).Select(p => $"{p.Key}={p.Value:F0}"))
                : "-";
            string ministers = history.lastValuesChangesByEntity.TryGetValue("PoliticalPower", out var byEntity) &&
                               byEntity.TryGetValue(StatisticImpactSourceType.Ministers, out var byMinister)
                ? string.Join(", ", byMinister.Where(p => Math.Abs(p.Value) >= 0.5f).OrderBy(p => p.Value).Select(p => $"{p.Key}={p.Value:F0}"))
                : "-";
            DevSettings.Log($"власть {gameState.date:dd.MM.yyyy}: {gameState.PoliticalPower:F0}\n  по источникам: {sources}\n  по входам: {inputs}\n  министры: {ministers}");
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

        private static readonly AccessTools.FieldRef<EventManager, Dictionary<int, AbstractEvent>> Events =
            AccessTools.FieldRefAccess<EventManager, Dictionary<int, AbstractEvent>>("events");

        private static readonly AccessTools.FieldRef<EventManager, Dictionary<int, AbstractEvent>> CrisisEvents =
            AccessTools.FieldRefAccess<EventManager, Dictionary<int, AbstractEvent>>("crisisEvents");

        static void Postfix(EventManager __instance, GameState gameState, ref int? __result)
        {
            if (!__result.HasValue)
            {
                return;
            }

            if (DevSettings.DebugLog)
            {
                DevSettings.Log($"событие {__result.Value} {gameState.date:dd.MM.yyyy}");
            }

            if (!DevSettings.AutoPlay ||
                !(Events(__instance).TryGetValue(__result.Value, out AbstractEvent story) || CrisisEvents(__instance).TryGetValue(__result.Value, out story)) ||
                story.GetType().Assembly != typeof(DevSettings).Assembly)
            {
                return;
            }

            UnitOfWork[] effects = story.StartEvent(gameState, new EventDrawInfo());
            effects[0].Execute(gameState);
            string result = story.FinishEvent(gameState, 0);
            DevSettings.Log($"автовыбор {__result.Value}: {result}");
            __result = null;
        }
    }
}
