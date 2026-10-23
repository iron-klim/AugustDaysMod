using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.EndingsSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;
using Frontend.MapWindow;
using Frontend.NewGame;
using Frontend.SceneLoadManagement;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

// Проверочный прогон партии без интерфейса для баланса. Включается файлом august_simulate в папке данных игры:
// в нем число месяцев, по желанию слово manage и страны (USSR Russia Ukraine). Со списком стран игра сама начинает
// партию за каждую по очереди, без списка прогон идет в партии, начатой вручную. Как только откроется карта, время идет само,
// события решаются первым доступным вариантом, а раз в месяц показатели пишутся в august_sim_<страна>_<режим>.csv.
// В режиме manage за игрока действует простой разумный правитель: сводит бюджет и двигает ставку против инфляции
namespace AugustDaysMod.DevTools
{
    public static class Simulation
    {
        private const int DAYS_PER_FRAME = 10;

        private static readonly HashSet<int> EndingChoices = new HashSet<int> { 27, 28 };

        private static GameState simulated;

        public static bool Running { get; private set; }

        public static int? Ending { get; set; }

        // Сколько стран из списка уже прогнано за этот запуск игры
        private static int finished;

        private static (int months, bool manage, List<Countries> countries)? Settings
        {
            get
            {
                string path = Path.Combine(Application.persistentDataPath, "august_simulate");
                if (!File.Exists(path))
                {
                    return null;
                }

                string[] words = File.ReadAllText(path).Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int months = words.Length > 0 && int.TryParse(words[0], out int value) ? value : 24;
                var countries = new List<Countries>();
                foreach (string word in words)
                {
                    if (!int.TryParse(word, out _) && Enum.TryParse(word, out Countries country))
                    {
                        countries.Add(country);
                    }
                }

                return (months, words.Contains("manage"), countries);
            }
        }

        public static void TryStart(MonoBehaviour host)
        {
            GameState gameState = GameStateManager.inst?.GameState;
            var settings = Settings;
            if (settings == null || gameState == null || Running || ReferenceEquals(simulated, gameState))
            {
                return;
            }

            simulated = gameState;
            host.StartCoroutine(Run(gameState, settings.Value.months, settings.Value.manage));
        }

        private static IEnumerator Run(GameState gameState, int months, bool manage)
        {
            Running = true;
            Ending = null;
            GameScenario scenario = GameStateManager.inst.GameScenario;
            GameRules rules = GameStateManager.inst.GameRules;
            DateTime end = gameState.date.AddMonths(months);
            string mode = manage ? "manage" : "idle";
            string path = Path.Combine(Application.persistentDataPath, $"august_sim_{gameState.playerCountry}_{mode}.csv");
            var csv = new StringBuilder();
            csv.AppendLine("date;puppetOf;gov;economy;reserve;income;expenses;priceIndex;rate;pp;private;unity;radicals;freedom;liberalization;forgery;order;firstNeeds;corruption;ipi;unionSubjects;events");
            int events = 0;
            int errors = 0;
            float lastPrice = gameState.PriceIndex;
            DevSettings.Log($"симуляция {gameState.playerCountry} ({mode}) на {months} мес. с {gameState.date:dd.MM.yyyy}");
            while (gameState.date < end && !Ending.HasValue)
            {
                for (int day = 0; day < DAYS_PER_FRAME && gameState.date < end && !Ending.HasValue; day++)
                {
                    try
                    {
                        if (gameState.date.Day == 1)
                        {
                            if (manage)
                            {
                                Steward.Tick(gameState, rules, lastPrice);
                            }

                            lastPrice = gameState.PriceIndex;
                        }

                        TickUtils.ProcessTick(gameState, scenario, rules);
                        events += ResolveEvents(gameState);
                        if (gameState.date.Day == 1)
                        {
                            csv.AppendLine(Row(gameState, events));
                        }
                    }
                    catch (Exception exception)
                    {
                        errors++;
                        if (errors <= 20)
                        {
                            Debug.LogError($"[AugustDays] симуляция {gameState.date:dd.MM.yyyy}: {exception}");
                        }
                    }
                }

                yield return null;
            }

            csv.AppendLine(Row(gameState, events));
            File.WriteAllText(path, csv.ToString());
            Running = false;
            DevSettings.Log($"симуляция окончена {gameState.date:dd.MM.yyyy}: событий {events}, ошибок {errors}, концовка {Ending?.ToString() ?? "-"}, итог в {path}");
            finished++;
            // Сцена карты могла еще не закончить загрузку: пока она идет, переход игнорируется
            int mapScene = SceneManager.GetActiveScene().buildIndex;
            for (int attempt = 0; attempt < 60 && NextCountry.HasValue && SceneManager.GetActiveScene().buildIndex == mapScene; attempt++)
            {
                SceneLoadManager.inst.LoadScene(1, 0f, displayAnimation: false);
                yield return new WaitForSeconds(1f);
            }
        }

        // Следующая страна из списка, за которую надо начать партию
        public static Countries? NextCountry
        {
            get
            {
                var settings = Settings;
                return settings != null && finished < settings.Value.countries.Count ? settings.Value.countries[finished] : (Countries?)null;
            }
        }

        public static int NewGameScene()
        {
            for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
            {
                if (SceneUtility.GetScenePathByBuildIndex(index).Contains("NewGame"))
                {
                    return index;
                }
            }

            return -1;
        }

        // Меню новой игры само выбирает страну из списка и начинает партию
        public static IEnumerator AutoStart(NewGameController controller, Countries country)
        {
            yield return new WaitForSeconds(2f);
            controller.SetCountry(country);
            if (country == Countries.USSR)
            {
                controller.SetPolitic(Characters.Gorbachev);
            }

            yield return null;
            DevSettings.Log($"симуляция: новая игра за {country}");
            controller.NextScene();
        }

        // Очередь событий разбирается так же, как ее разбирает окно события: эффект выбранного варианта и завершение
        private static int ResolveEvents(GameState gameState)
        {
            int resolved = 0;
            var candidates = new List<int>();
            int? super = EventManager.inst.GetNewSuperEvent(gameState);
            if (super.HasValue)
            {
                candidates.Add(super.Value);
            }

            for (int guard = 0; guard < 50 && (gameState.currentEventsIds.Count > 0 || candidates.Count > 0); guard++)
            {
                int id;
                if (candidates.Count > 0)
                {
                    id = candidates[0];
                    candidates.RemoveAt(0);
                }
                else
                {
                    EventUtils.ClearInvalidEvents(gameState);
                    if (gameState.currentEventsIds.Count == 0)
                    {
                        break;
                    }

                    id = gameState.currentEventsIds.Pop().id;
                }

                var drawInfo = new EventDrawInfo();
                (EventType type, UnitOfWork[] options) = EventManager.inst.StartEvent(id, gameState, drawInfo);
                resolved++;
                if (type == EventType.Message || options == null || options.Length == 0)
                {
                    continue;
                }

                // В кризисах 27 и 28 первый вариант — отставка и конец игры: прогон берет следующий доступный
                int first = EndingChoices.Contains(id) ? 1 : 0;
                int choice = first;
                if (drawInfo.optionsEnabled != null)
                {
                    int enabled = Array.FindIndex(drawInfo.optionsEnabled, first, value => value);
                    choice = enabled >= 0 ? enabled : first;
                }

                choice = Math.Min(choice, options.Length - 1);
                options[choice]?.Execute(gameState, GameStateManager.inst.GameRules, GameStateManager.inst.GameScenario);
                EventManager.inst.FinishEvent(id, gameState, choice);
            }

            return resolved;
        }

        private static string Row(GameState gameState, int events)
        {
            Country country = gameState.countries[gameState.playerCountry];
            int income = gameState.gameStateHistory.lastIncome.Values.Sum();
            int expenses = gameState.expenses.Values.Sum();
            string F(float value) => value.ToString("F2", CultureInfo.InvariantCulture);
            return string.Join(";", gameState.date.ToString("yyyy-MM-dd"), country.puppetOf?.ToString() ?? "-", country.governmentType?.ToString() ?? "-",
                DoctrineUtils.GetEconomyType(gameState), gameState.Reserve, income, expenses, F(gameState.PriceIndex), gameState.refinancingRate,
                F(gameState.PoliticalPower), F(gameState.privateSectorPower), F(gameState.UnityLevel), F(gameState.RadicalsPower), F(gameState.FreedomLevel),
                F(gameState.LiberalizationLevel), F(gameState.ForgeryLevel), F(gameState.OrderLevel), F(gameState.FirstNeedsGoods), F(gameState.CorruptionLevel),
                F(StatisticUtils.GetIPI(gameState)), SovietUnion.Subjects(gameState).Count(), events);
        }
    }

    // Разумный правитель для прогона: раз в месяц подгоняет расходы под доходы (с небольшим дефицитом, пока есть резерв)
    // и поднимает ставку рефинансирования, когда цены растут быстрее 1,5% в месяц
    public static class Steward
    {
        public static void Tick(GameState gameState, GameRules rules, float lastPrice)
        {
            // Статьи, от которых напрямую зависит власть, правитель не трогает
            HashSet<ExpenseType> fixedExpenses = new HashSet<ExpenseType>(rules.expenseGroups[ExpenseGroup.Fixed])
            {
                ExpenseType.BribesToGenerals, ExpenseType.BribesToSecretServices, ExpenseType.BribesToIntellectuals,
                ExpenseType.BribesToOfficials, ExpenseType.Propaganda, ExpenseType.SalariesOfCivilServants
            };
            int income = gameState.gameStateHistory.lastIncome.Values.Sum();
            if (income > 0)
            {
                int fixedSum = gameState.expenses.Where(e => fixedExpenses.Contains(e.Key)).Sum(e => e.Value);
                int adjustable = gameState.expenses.Where(e => !fixedExpenses.Contains(e.Key)).Sum(e => e.Value);
                int target = income + Mathf.Max(0, gameState.Reserve) / 24 - fixedSum;
                if (adjustable > 0)
                {
                    float factor = Mathf.Clamp((float)target / adjustable, 0.9f, 1.05f);
                    foreach (ExpenseType type in gameState.expenses.Keys.Where(t => !fixedExpenses.Contains(t)).ToList())
                    {
                        gameState.expenses[type] = Mathf.RoundToInt(gameState.expenses[type] * factor);
                    }

                    EconomyUtils.ClampExpenses(gameState, rules);
                }
            }

            if (DoctrineUtils.IsPlannedEconomy(gameState) || lastPrice <= 0f)
            {
                return;
            }

            float inflation = gameState.PriceIndex / lastPrice - 1f;
            if (inflation > 0.015f)
            {
                gameState.refinancingRate = Mathf.Min(gameState.refinancingRate + 2, 40);
            }
            else if (inflation < 0.003f && gameState.refinancingRate > 10)
            {
                gameState.refinancingRate--;
            }
        }
    }

    [HarmonyPatch(typeof(MapWindowController), "Start")]
    public static class PatchSimulationStart
    {
        static void Postfix(MapWindowController __instance) => Simulation.TryStart(__instance);
    }

    // Вместо главного меню — сразу меню новой игры, пока в списке прогона есть страны
    [HarmonyPatch(typeof(SceneLoadManager), nameof(SceneLoadManager.LoadScene))]
    public static class PatchSimulationMenu
    {
        static void Prefix(ref int newScene)
        {
            if (newScene != 1 || !Simulation.NextCountry.HasValue)
            {
                return;
            }

            int scene = Simulation.NewGameScene();
            if (scene >= 0)
            {
                newScene = scene;
            }
        }
    }

    [HarmonyPatch(typeof(NewGameController), "Awake")]
    public static class PatchSimulationNewGame
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(NewGameController __instance)
        {
            Countries? country = Simulation.NextCountry;
            if (country.HasValue)
            {
                __instance.StartCoroutine(Simulation.AutoStart(__instance, country.Value));
            }
        }
    }

    // Концовка в прогоне не открывает свой экран: прогон просто заканчивается
    [HarmonyPatch(typeof(EndingsManager), nameof(EndingsManager.ExecuteEnding))]
    public static class PatchSimulationEnding
    {
        static bool Prefix(int number)
        {
            if (!Simulation.Running)
            {
                return true;
            }

            Simulation.Ending = number;
            return false;
        }
    }
}
