using System;
using AugustDaysMod.Assets;
using AugustDaysMod.Scenarios;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    // Игра за Горбачева: ГКЧП изолировал президента в Форосе
    [Event(ID)]
    public class ForosIsolation : CoupEvent
    {
        public const int ID = 1010;
        public const int ANSWER_1 = 0;
        public const int DESCRIPTION = 1;
        public const int TITLE = 2;
        public const int RESULT_1 = 5;

        public ForosIsolation() : base(new DateTime(1991, 8, 19))
        {
        }

        protected override bool IsGorbachevBranch => true;

        public override bool CheckConditions(GameState gameState)
        {
            // Если игрок успел сорвать заговор, путча не будет
            return base.CheckConditions(gameState) && GkchpConspiracy.Find(gameState) != null;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            return StartOptions(gameState, eventDrawInfo, 1, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), Characters.Boldin);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, RESULT_START + choice);
        }
    }
}
