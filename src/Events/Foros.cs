using System;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;

namespace AugustDaysMod.Events
{
    [Event(ID)]
    public class Foros : CoupEvent
    {
        public const int ID = 1007;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int RESULT_1_OK = 5;
        public const int RESULT_1_FAIL = 6;
        public const int RESULT_2 = 7;

        public Foros() : base(new DateTime(1991, 8, 21))
        {
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            Characters minister = gameState.politicPositions[PoliticPosition.SecondSecretary].Value;
            return StartOptions(gameState, eventDrawInfo, 2, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), minister);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);

            if (choice == ANSWER_2)
            {
                return Texts.GetEvent(ID, RESULT_2);
            }

            if (gameState.MilitaryStaffLoyalty >= 60 && gameState.SpecialServicesLoyalty >= 60 &&
                gameState.RadicalsPower < 50)
            {
                gameState.MilitaryStaffLoyalty += 20;
                gameState.SpecialServicesLoyalty += 20;
                return Texts.GetEvent(ID, RESULT_1_OK);
            }

            gameState.MilitaryStaffLoyalty -= 20;
            gameState.SpecialServicesLoyalty -= 20;
            return Texts.GetEvent(ID, RESULT_1_FAIL);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            if (optionIndex == ANSWER_1)
            {
                TextFormatter textFormatter = TextFormatter.StartCreation();
                textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "success"]);
                textFormatter.AddMilitaryStaffLoyalty(20);
                textFormatter.AddSpecialServicesLoyalty(20);
                textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "lost"]);
                textFormatter.AddMilitaryStaffLoyalty(-20);
                textFormatter.AddSpecialServicesLoyalty(-20);
                description.AddText(textFormatter.Finalize());
            }
        }
    }
}
