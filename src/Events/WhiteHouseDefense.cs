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
    // Игра за Горбачева: исход противостояния в Москве зависит от Ельцина, армии и КГБ, а не от президента
    [Event(ID)]
    public class WhiteHouseDefense : CoupEvent
    {
        public const int ID = 1013;
        public const int ANSWER_1 = 0;
        public const int DESCRIPTION = 1;
        public const int TITLE = 2;
        public const int RESULT_1 = 5;

        // Ниже этой лояльности генералитет и спецслужбы готовы выполнять приказы ГКЧП
        private const float LoyaltyThreshold = 70f;

        public WhiteHouseDefense() : base(new DateTime(1991, 8, 20))
        {
        }

        protected override bool IsGorbachevBranch => true;

        public override bool CheckConditions(GameState gameState)
        {
            return base.CheckConditions(gameState)
                && gameState.eventsDone.ContainsKey(ForosIsolation.ID)
                && GkchpConspiracy.Find(gameState) != null;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            return StartOptions(gameState, eventDrawInfo, 1, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), Characters.Yeltsin);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.Alive)
            {
                description.AddText("Ельцин возглавил оборону Белого дома");
                description.AddCoupPower(-10);
            }

            bool armyLoyal = gameState.MilitaryStaffLoyalty >= LoyaltyThreshold;
            description.AddText(armyLoyal ? "Генералитет отказывается штурмовать Белый дом" : "Генералитет готов выполнить приказ о штурме");
            description.AddCoupPower(armyLoyal ? -5 : 5);

            bool specialServicesLoyal = gameState.SpecialServicesLoyalty >= LoyaltyThreshold;
            description.AddText(specialServicesLoyal ? "Группа «Альфа» не выполнит приказ" : "Спецназ КГБ готов к операции");
            description.AddCoupPower(specialServicesLoyal ? -5 : 5);
        }
    }
}
