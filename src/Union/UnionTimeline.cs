using System;
using System.Collections.Generic;
using AugustDaysMod.Storage;
using AugustDaysMod.Story;
using Backend.Gamedesign;
using Backend.Gamedesign.Utils;
using HarmonyLib;

namespace AugustDaysMod.Union
{
    // Исторические вехи Союза, которые происходят без игрока. Каждая срабатывает один раз
    public static class UnionTimeline
    {
        private class Milestone
        {
            public string key;
            public DateTime date;
            public Func<GameState, bool> condition = gameState => true;
            public Action<GameState> action;
        }

        private static readonly Countries[] Baltic = { Countries.Litva, Countries.Latvia, Countries.Estonia };

        private static readonly List<Milestone> Milestones = new List<Milestone>
        {
            new Milestone
            {
                key = "timeline_coup_failed",
                date = new DateTime(1991, 8, 22),
                action = gameState =>
                {
                    UnionDynamics.ChangeCohesion(gameState, -20f);
                    foreach (Countries republic in Baltic)
                    {
                        UnionRelations.Add(gameState, republic, -20f);
                        gameState.countries[republic].leaderType = Country.LeaderType.Separatist;
                    }

                    foreach (KeyValuePair<Countries, Country> member in UnionDynamics.MemberRepublics(gameState))
                    {
                        UnionRelations.Add(gameState, member.Key, -8f);
                    }

                    UnionNews.Add(gameState, "Путч в Москве провалился. Члены ГКЧП арестованы, Горбачев вернулся из Фороса в другую страну: " +
                                             "деятельность КПСС приостановлена, союзные министерства переходят под контроль российского руководства. " +
                                             "Республики одна за другой заявляют о независимости.");
                }
            },
            new Milestone
            {
                key = "timeline_baltic",
                date = new DateTime(1991, 9, 6),
                action = gameState =>
                {
                    foreach (Countries republic in Baltic)
                    {
                        UnionDynamics.DeclareIndependence(gameState, republic);
                    }

                    UnionNews.Add(gameState, "Государственный Совет СССР признал независимость Литвы, Латвии и Эстонии.");
                }
            },
            new Milestone
            {
                // Когда Украиной управляет игрок, референдум — его сюжетное событие
                key = "timeline_ukraine_referendum",
                date = new DateTime(1991, 12, 1),
                condition = gameState => gameState.playerCountry != Countries.Ukraine && SovietUnion.IsSubject(gameState.countries[Countries.Ukraine]),
                action = gameState =>
                {
                    UnionNews.Add(gameState, "На Украине прошел референдум: за независимость проголосовали более 90% избирателей. " +
                                             "Президентом избран Леонид Кравчук.");
                    UnionDynamics.DeclareIndependence(gameState, Countries.Ukraine);
                }
            },
            new Milestone
            {
                key = "timeline_dissolution",
                date = new DateTime(1991, 12, 26),
                condition = gameState => ModStorage.For(gameState).flags.ContainsKey(StoryFlags.Belovezha),
                action = gameState => UnionDynamics.Dissolve(gameState,
                    ModStorage.For(gameState).Flag(StoryFlags.Belovezha) == 1 ? "agreement" : "russia")
            }
        };

        public static void Daily(GameState gameState)
        {
            UnionData union = SovietUnion.Data(gameState);
            if (!union.active || union.dissolved)
            {
                return;
            }

            ModSaveData data = ModStorage.For(gameState);
            foreach (Milestone milestone in Milestones)
            {
                if (data.flags.ContainsKey(milestone.key) || gameState.date < milestone.date || !milestone.condition(gameState))
                {
                    continue;
                }

                data.SetFlag(milestone.key);
                milestone.action(gameState);
                if (union.dissolved)
                {
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TickUtils), "ProcessTickCore")]
    public static class PatchProcessTickCoreTimeline
    {
        static void Postfix(GameState gameState)
        {
            if (gameState.playerCountry != Countries.USSR)
            {
                UnionTimeline.Daily(gameState);
            }
        }
    }
}
