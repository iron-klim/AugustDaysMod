using System;
using System.Collections.Generic;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;

namespace AugustDaysMod.Events
{
    public class SovietsPalace : AbstractEvent
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
        
        private EventInfo eventInfo = new EventInfo()
        {
            priority = 0,
            startDate = new DateTime(1991, 8, 20),
            endDate = new DateTime(1991, 8, 21),
            countryLimitation = Countries.USSR,
            eventType = EventType.Dialog,
        };
        
        public override EventInfo EventInfo => eventInfo;
        
        private int[] _resultNumbers = Array.Empty<int>();
        
        public override bool CheckConditions(GameState gameState)
        {
            return !gameState.eventsDone.ContainsKey(ID) && gameState.date >= eventInfo.startDate;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            const int optionsCount = 3;
            _resultNumbers = new int[optionsCount];
            string eventTitle = Texts.GetEvent(ID, TITLE);
            string eventDesc = Texts.GetEvent(ID, DESCRIPTION);

            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.Alive)
            {
                eventDesc += Texts.GetEvent(ID, YELTSIN_ALIVE);
            }
            
            Characters minister = gameState.politicPositions[PoliticPosition.MinisterOfInternalAffairs].Value;
            EventUtils.SetupEventDrawInfo(eventDrawInfo, ID, eventInfo, optionsCount, eventTitle, eventDesc, minister);
            List<UnitOfWork[]> descriptionVariants = EventUtils.PrepareDescriptionVariants(3, optionsCount);
            UnitOfWork[] unitOfWorkArray = new UnitOfWork[optionsCount];
        
            for (int index = 0; index < optionsCount; ++index)
            {
                _resultNumbers[index] = 0;
                unitOfWorkArray[index] = UnitOfWork.StartCreation();
                SetupOption(gameState, eventDrawInfo, index, descriptionVariants, unitOfWorkArray[index]);
            }
            
            return unitOfWorkArray;
        }
        
        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            
            switch (choice)
            {
                case ANSWER_1:
                    return Texts.GetEvent(ID, Manager.RESULT_START + ANSWER_1);
                case ANSWER_3:
                    return Texts.GetEvent(ID, Manager.RESULT_START + ANSWER_3);
            }
            
            if (gameState.MilitaryStaffLoyalty >= 60 && gameState.SpecialServicesLoyalty >= 60)
            {
                gameState.RadicalsPower -= 10;
                gameState.countries[Countries.Russia].RelationshipWithPlayer += 5;
                gameState.countries[Countries.Russia].ForeignPolicyVector += 5;
                    
                return Texts.GetEvent(ID, RESULT_2_OK);
            }
            
            gameState.RadicalsPower += 30;
            gameState.countries[Countries.Russia].RelationshipWithPlayer -= 10;
            gameState.countries[Countries.Russia].ForeignPolicyVector -= 10;
            gameState.MilitaryStaffLoyalty -= 10;
            gameState.SpecialServicesLoyalty -= 10;
            
            return Texts.GetEvent(ID, RESULT_2_FAIL);
        }

        private void SetupOption(
            GameState gameState,
            EventDrawInfo eventDrawInfo,
            int optionIndex,
            List<UnitOfWork[]> descriptionVariants,
            UnitOfWork description)
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
        
            eventDrawInfo.optionsNames[optionIndex] = Texts.GetEvent(ID, optionIndex);
            eventDrawInfo.optionsEnabled[optionIndex] = true;
            eventDrawInfo.optionsNamesAlt[optionIndex] = EventUtils.GenerateOptionDescription(descriptionVariants, optionIndex, description);
            EventUtils.FinalizeDescription(descriptionVariants, optionIndex, description, _resultNumbers);

            if (optionIndex == ANSWER_2)
            {
                if (gameState.eventsDone[MiltaryAction.ID].choice == MiltaryAction.ANSWER_3)
                {
                    eventDrawInfo.optionsEnabled[optionIndex] = false;
                }
            }
        }
    }
}