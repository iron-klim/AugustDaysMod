using System;
using System.Linq;
using Backend.Gamedesign;
using Backend.Gamedesign.ConspiracySystem.Conspiracies.GameConspiracies;
using HarmonyLib;

namespace AugustDaysMod.Scenarios
{
    // ГКЧП в игре за Горбачева: обычный заговор игры «Путч против игрока», но исход путча решают события мода
    public static class GkchpConspiracy
    {
        public const Characters Leader = Characters.Kryuchkov;

        // 5 августа Крючков поручил готовить документы о чрезвычайном положении на объекте АВС
        public static readonly DateTime StartDate = new DateTime(1991, 8, 5);

        public static readonly Characters[] Members =
        {
            Characters.Kryuchkov, Characters.Yanayev, Characters.Yazov, Characters.Pugo, Characters.Pavlov,
            Characters.Baklanov, Characters.Starodubtsev, Characters.Tizyakov, Characters.Shenin, Characters.Boldin,
            Characters.Varennikov
        };

        public static bool IsGorbachevGame(GameState gameState) => gameState.gameCharacter == Characters.Gorbachev;

        public static void Create(GameState gameState)
        {
            var conspiracy = new ConspiracyInfo
            {
                type = ConspiracyType.PutschAgainstThePlayer,
                leader = Leader,
                // Близко к успеху, но до 100 не дотягивает: сам по себе заговор не сработает
                power = 90f,
                dateStart = StartDate,
                // О подготовке переворота Горбачева предупреждали, но он не верил
                discoveryStage = ConspiracyInfo.DiscoveryStage.Rumour
            };
            conspiracy.members.UnionWith(Members);
            gameState.conspiracies.Add(conspiracy);
        }

        // Заговор ГКЧП, если он еще действует: игрок мог его сорвать до 19 августа
        public static ConspiracyInfo Find(GameState gameState)
        {
            return gameState.conspiracies.FirstOrDefault(conspiracy => IsGkchp(conspiracy)
                && conspiracy.isActive
                && conspiracy.members.Count > 0
                && gameState.characters[Leader].status == CharacterInfo.PoliticStatus.Alive);
        }

        public static bool IsGkchp(ConspiracyInfo conspiracy)
        {
            return conspiracy.type == ConspiracyType.PutschAgainstThePlayer
                && conspiracy.leader == Leader
                && conspiracy.dateStart == StartDate;
        }
    }

    // Игровое разрешение путча (событие 15) для ГКЧП не используется: путч наступает по событиям мода
    [HarmonyPatch(typeof(Conspiracy5), nameof(Conspiracy5.GetConditionsToSucced))]
    public static class PatchConspiracy5GetConditionsToSucced
    {
        static void Postfix(ConspiracyInfo conspiracyInfo, ref bool __result)
        {
            if (__result && GkchpConspiracy.IsGkchp(conspiracyInfo))
            {
                __result = false;
            }
        }
    }
}
