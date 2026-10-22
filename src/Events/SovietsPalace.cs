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
    public class SovietsPalace : CoupEvent
    {
        public const int ID = 1006;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int RESULT_1 = 5;
        public const int RESULT_2_OK = 6;
        public const int RESULT_3 = 7;
        public const int RESULT_2_FAIL= 8;
        public const int YELTSIN_ALIVE = 9;

        public SovietsPalace() : base(new DateTime(1991, 8, 20))
        {
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            string eventDesc = Texts.GetEvent(ID, DESCRIPTION);

            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.Alive)
            {
                eventDesc += Texts.GetEvent(ID, YELTSIN_ALIVE);
            }

            Characters minister = gameState.politicPositions[PoliticPosition.MinisterOfInternalAffairs].Value;
            return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), eventDesc, minister);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);

            switch (choice)
            {
                case ANSWER_1:
                    return Texts.GetEvent(ID, RESULT_START + ANSWER_1);
                case ANSWER_3:
                    return Texts.GetEvent(ID, RESULT_START + ANSWER_3);
            }

            Country russia = gameState.countries[Countries.Russia];
            if (gameState.MilitaryStaffLoyalty >= 60 && gameState.SpecialServicesLoyalty >= 60)
            {
                gameState.RadicalsPower -= 10;
                russia.RelationshipWithPlayer += 5;
                russia.ForeignPolicyVector += 5;

                return Texts.GetEvent(ID, RESULT_2_OK);
            }

            gameState.RadicalsPower += 30;
            russia.RelationshipWithPlayer -= 10;
            russia.ForeignPolicyVector -= 10;
            gameState.MilitaryStaffLoyalty -= 10;
            gameState.SpecialServicesLoyalty -= 10;

            return Texts.GetEvent(ID, RESULT_2_FAIL);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            switch (optionIndex)
            {
                case ANSWER_1:
                    description.AddRadicalsPower(-5);
                    break;
                case ANSWER_2:
                    TextFormatter textFormatter = TextFormatter.StartCreation();
                    textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "success"]);
                    textFormatter.AddRadicalsPower(-10);
                    textFormatter.AddCountryRelations(gameState, 5, Countries.Russia);
                    textFormatter.AddDiplomacyVector(gameState, 5, Countries.Russia);
                    textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "lost"]);
                    textFormatter.AddRadicalsPower(30);
                    textFormatter.AddCountryRelations(gameState, -10, Countries.Russia);
                    textFormatter.AddDiplomacyVector(gameState, -10, Countries.Russia);
                    textFormatter.AddMilitaryStaffLoyalty(-10);
                    textFormatter.AddSpecialServicesLoyalty(-10);
                    description.AddText(textFormatter.Finalize());
                    break;
                case ANSWER_3:
                    description.AddRadicalsPower(25);
                    break;
            }
        }

        // Второй вариант недоступен, если в событии MiltaryAction был выбран третий
        protected override bool IsOptionEnabled(GameState gameState, int optionIndex)
        {
            return optionIndex != ANSWER_2
                   || !gameState.eventsDone.TryGetValue(MiltaryAction.ID, out var militaryAction)
                   || militaryAction.choice != MiltaryAction.ANSWER_3;
        }
    }
}
