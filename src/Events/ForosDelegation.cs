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
    // Игра за Горбачева: делегация ГКЧП в Форосе требует ввести чрезвычайное положение
    [Event(ID)]
    public class ForosDelegation : CoupEvent
    {
        public const int ID = 1010;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int DESCRIPTION = 3;
        public const int TITLE = 4;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;
        public const int RESULT_3 = 7;

        public ForosDelegation() : base(new DateTime(1991, 8, 18))
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
            return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), Characters.Boldin);
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
                case ANSWER_2:
                    // Президент сам вводит чрезвычайное положение: переворот больше не нужен
                    description.AddEndCoup(ModLanguage.Pick("ГКЧП будет действовать от имени президента", "The GKChP will act on behalf of the President"));
                    description.ChangeDoctrineToOnePoint(gameState, DoctrineType.ControlOfMedia, true);
                    description.AddMilitaryStaffLoyalty(10);
                    description.AddSpecialServicesLoyalty(10);
                    description.AddRadicalsPower(15);
                    description.AddCountryRelations(gameState, -20, Countries.USA);
                    break;
                case ANSWER_3:
                    // Заговорщики толкуют уклончивый ответ как молчаливое согласие
                    description.AddCoupPower(10);
                    description.AddMilitaryStaffLoyalty(-5);
                    description.AddPoliticPower(-5, Characters.Gorbachev);
                    break;
            }
        }
    }
}
