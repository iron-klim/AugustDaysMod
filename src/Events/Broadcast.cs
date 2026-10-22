using System;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    [Event(ID)]
    public class Broadcast : CoupEvent
    {
        public const int ID = 1004;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;
        public const int RESULT_3 = 7;

        public Broadcast() : base(new DateTime(1991, 8, 19))
        {
        }

        // Вызывается только из YeltsinProblem
        public override bool CanBeAddedOnlyManually => true;

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            Characters minister = gameState.politicPositions[PoliticPosition.ChairmanOfTheSupremeCouncil].Value;
            return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), minister);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            EventManager.inst.ProccessEventImmediately(gameState, PoliticalDirection.ID, false);
            return Texts.GetEvent(ID, RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            if (optionIndex == ANSWER_2)
            {
                description.AddRadicalsPower(-5);
                description.AddPoliticPower(5, Characters.Andreeva);
                description.AddFactionPowerNum(gameState, Factions.NeoStalinist, 5);
                description.AddMilitaryStaffLoyalty(-5);
                description.AddSpecialServicesLoyalty(-5);
            }
        }
    }
}
