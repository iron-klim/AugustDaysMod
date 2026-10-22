using System;
using System.Collections.Generic;
using System.Linq;
using AugustDaysMod.Catalog;
using Backend.Gamedesign;
using Backend.Gamedesign.DecisionsSystem.Decisions;
using Backend.Gamedesign.DecisionsSystem.Utils;
using Backend.Gamedesign.DiplomacyButtonsSystem.Utils;
using Backend.Gamedesign.DiplomacyIconsSystem.DiplomacyIcons.GameDiplomacyIcons;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Managers;
using Backend.Mod;
using HarmonyLib;
using UnityEngine;

// Игра написана под игрока-СССР. Когда игрок управляет другой страной, союзные механики к нему неприменимы:
// здесь они отключаются или пересчитываются для его страны
namespace AugustDaysMod.Compat
{
    public static class NonUnionPlayer
    {
        public static bool Active(GameState gameState) => gameState != null && gameState.playerCountry != Countries.USSR;

        public static GameState Current => GameStateManager.inst != null ? GameStateManager.inst.GameState : null;

        // Кнопки дипломатии, которые действуют от лица СССР: СЭВ, ОВД, советские базы, оккупации, войны на стороне СССР
        public static readonly HashSet<int> UnionDiplomacyButtons = new HashSet<int>
        {
            0, 1, 3, 17, 35, 40, 41, 45, 47, 60, 103, 138, 146, 147, 149, 179
        };

        // Модификаторы о союзном центре, Политбюро и республиках
        public static readonly HashSet<int> UnionModificators = new HashSet<int>
        {
            11, 27, 34, 37, 85, 92, 93, 95, 96, 103, 104, 113
        };

        // Решения мода для страны игрока. Встроенные решения игры рассчитаны на СССР
        public static readonly HashSet<int> AllowedDecisions = new HashSet<int>();

        // Убирает союзное наследство из состояния новой игры за другую страну
        public static void CleanStartState(GameState gameState)
        {
            foreach (int id in UnionModificators)
            {
                gameState.activeModificators.Remove(id);
            }

            gameState.subscribedDecisions.RemoveWhere(id => !AllowedDecisions.Contains(id));
            gameState.autoAcceptedDecisions.RemoveWhere(id => !AllowedDecisions.Contains(id));
            gameState.tutorialEnabled = false;
        }
    }

    // События с привязкой к стране игра проверяет только при обычном выборе событий, а вброшенные напрямую — нет.
    // Так до игрока доходили союзные события, ведущие к союзным концовкам
    [HarmonyPatch(typeof(EventManager), nameof(EventManager.ProccessEventImmediately))]
    public static class PatchProccessEventImmediately
    {
        static bool Prefix(EventManager __instance, GameState gameState, int Id) =>
            !NonUnionPlayer.Active(gameState) || EventFilter.Fits(__instance, gameState, Id);
    }

    public static class EventFilter
    {
        private static readonly AccessTools.FieldRef<EventManager, Dictionary<int, AbstractEvent>> Events =
            AccessTools.FieldRefAccess<EventManager, Dictionary<int, AbstractEvent>>("events");

        private static readonly AccessTools.FieldRef<EventManager, Dictionary<int, AbstractEvent>> CrisisEvents =
            AccessTools.FieldRefAccess<EventManager, Dictionary<int, AbstractEvent>>("crisisEvents");

        public static bool Fits(EventManager manager, GameState gameState, int id)
        {
            if (!Events(manager).TryGetValue(id, out AbstractEvent abstractEvent) && !CrisisEvents(manager).TryGetValue(id, out abstractEvent))
            {
                return true;
            }

            Countries? limitation = abstractEvent.EventInfo?.countryLimitation;
            return !limitation.HasValue || limitation.Value == gameState.playerCountry;
        }

        // Убирает из очереди события, вброшенные в обход проверки страны
        public static void Sweep(GameState gameState)
        {
            if (EventManager.inst == null || gameState.currentEventsIds.Count == 0)
            {
                return;
            }

            List<(int id, bool flag)> queue = gameState.currentEventsIds.Reverse().ToList();
            if (queue.All(e => Fits(EventManager.inst, gameState, e.id)))
            {
                return;
            }

            gameState.currentEventsIds.Clear();
            foreach ((int id, bool flag) in queue)
            {
                if (Fits(EventManager.inst, gameState, id))
                {
                    gameState.currentEventsIds.Push((id, flag));
                }
                else
                {
                    Debug.Log($"[AugustDays] событие {id} не для {gameState.playerCountry}, убрано");
                }
            }
        }
    }

    [HarmonyPatch(typeof(TickUtils), "ProcessTickCore")]
    public static class PatchProcessTickCoreSweep
    {
        static void Prefix(GameState gameState)
        {
            if (NonUnionPlayer.Active(gameState))
            {
                EventFilter.Sweep(gameState);
            }
        }

        static void Postfix(GameState gameState)
        {
            if (NonUnionPlayer.Active(gameState))
            {
                EventFilter.Sweep(gameState);
            }
        }
    }

    // Встроенные решения игры рассчитаны на СССР: для другой страны показываем только решения мода
    [HarmonyPatch(typeof(DecisionsManager), "IsDecisionShown")]
    public static class PatchIsDecisionShown
    {
        static void Postfix(int decisionId, AbstractDecision decision, GameState gameState, ref bool __result)
        {
            // Решения мода сами проверяют страну игрока
            bool modDecision = decision != null && decision.GetType().Assembly == typeof(NonUnionPlayer).Assembly;
            if (__result && NonUnionPlayer.Active(gameState) && !modDecision && !NonUnionPlayer.AllowedDecisions.Contains(decisionId))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(DiplomacyButtonsManager), "IsButtonAllowed")]
    public static class PatchIsButtonAllowed
    {
        static void Postfix(int id, ref bool __result)
        {
            if (__result && NonUnionPlayer.UnionDiplomacyButtons.Contains(id) && NonUnionPlayer.Active(NonUnionPlayer.Current))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(BuiltInContentRules), nameof(BuiltInContentRules.IsBuiltInModificatorAllowed))]
    public static class PatchIsBuiltInModificatorAllowed
    {
        static void Postfix(int modificatorId, GameState gameState, ref bool __result)
        {
            if (__result && NonUnionPlayer.UnionModificators.Contains(modificatorId) && NonUnionPlayer.Active(gameState))
            {
                __result = false;
            }
        }
    }

    // Обязательны только посты, которые есть у страны игрока в справочнике
    [HarmonyPatch(typeof(PoliticUtils), nameof(PoliticUtils.IfPositionPossible))]
    public static class PatchIfPositionPossible
    {
        static void Postfix(GameState gameState, PoliticPosition position, ref bool __result)
        {
            if (!__result || !NonUnionPlayer.Active(gameState) || position == PoliticPosition.President)
            {
                return;
            }

            CountryProfile profile = CountryCatalog.Get(gameState.playerCountry);
            if (profile?.posts != null && !profile.posts.ContainsKey(position))
            {
                __result = false;
            }
        }
    }

    // Политическая власть получает слагаемое «лояльность республик». У страны без республик его заменяет единство страны
    [HarmonyPatch(typeof(VariablesInfluenceUtils), "SetPolitPower")]
    public static class PatchSetPolitPower
    {
        static void Postfix(GameState gameState)
        {
            if (!NonUnionPlayer.Active(gameState) || CountryUtils.GetNumberOfRepublicsOfPlayer(gameState) > 0)
            {
                return;
            }

            float multiplier = 1f + gameState.FreedomLevel / 50f + (DoctrineUtils.IsOneParty(gameState) ? 0f : 1f);
            gameState.PoliticalPowerNotClamped += (int)(gameState.UnityLevel * gameState.FreedomLevel / 100f) * multiplier;
            gameState.PoliticalPowerNotClamped += CountryCatalog.Get(gameState.playerCountry)?.politicalPowerBonus ?? 0f;
        }
    }

    // Значки ядерного оружия, лидеров и субъектов штрафуют игрока за его собственную страну и за чужие союзные республики
    [HarmonyPatch]
    public static class PatchUnionDiplomacyIcons
    {
        static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(DiplomacyIconNuclear), nameof(DiplomacyIconNuclear.Tick));
            yield return AccessTools.Method(typeof(DiplomacyIconLeader), nameof(DiplomacyIconLeader.Tick));
            yield return AccessTools.Method(typeof(DiplomacyIconPuppet), nameof(DiplomacyIconPuppet.Tick));
        }

        static bool Prefix(GameState gameState, Countries country)
        {
            if (!NonUnionPlayer.Active(gameState))
            {
                return true;
            }

            return country != gameState.playerCountry && gameState.countries[country].puppetOf != Countries.USSR;
        }
    }
}
