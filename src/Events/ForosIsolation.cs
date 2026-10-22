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
    // Игра за Горбачева: связь с дачей отключена, ГКЧП объявил о болезни президента
    [Event(ID)]
    public class ForosIsolation : CoupEvent
    {
        public const int ID = 1012;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int DESCRIPTION = 3;
        public const int TITLE = 4;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;
        public const int RESULT_3 = 7;

        public ForosIsolation() : base(new DateTime(1991, 8, 19))
        {
        }

        protected override bool IsGorbachevBranch => true;

        public override bool CheckConditions(GameState gameState)
        {
            return base.CheckConditions(gameState)
                && gameState.eventsDone.ContainsKey(ForosDelegation.ID)
                && GkchpConspiracy.Find(gameState) != null;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), Characters.Yanayev);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            switch (optionIndex)
            {
                case ANSWER_1:
                    description.AddCoupPower(-5);
                    break;
                case ANSWER_2:
                    // Охрана президента против пограничников КГБ
                    description.AddCoupPower(-10);
                    description.AddSpecialServicesLoyalty(-10);
                    break;
                case ANSWER_3:
                    description.AddCoupPower(10);
                    break;
            }
        }
    }
}
