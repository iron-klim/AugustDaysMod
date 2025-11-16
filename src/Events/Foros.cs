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
    public class Foros : AbstractEvent
    {
        public const int ID = 1007;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int RESULT_1_OK = 5;
        public const int RESULT_1_FAIL = 6;
        public const int RESULT_2 = 7;
        
        private EventInfo eventInfo = new EventInfo()
        {
            priority = 0,
            startDate = new DateTime(1991, 8, 21),
            endDate = new DateTime(1991, 8, 22),
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
            const int optionsCount = 2;
            _resultNumbers = new int[optionsCount];
            string eventTitle = Texts.GetEvent(ID, TITLE);
            string eventDesc = Texts.GetEvent(ID, DESCRIPTION);
            Characters minister = gameState.politicPositions[PoliticPosition.SecondSecretary].Value;
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
                    TextFormatter textFormatter = TextFormatter.StartCreation();
                    textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "success"]);
                    textFormatter.AddMilitaryStaffLoyalty(20);
                    textFormatter.AddSpecialServicesLoyalty(20);
                    textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "lost"]);
                    textFormatter.AddMilitaryStaffLoyalty(-20);
                    textFormatter.AddSpecialServicesLoyalty(-20);
                    description.AddText(textFormatter.Finalize());
                    break;
            }
        
            eventDrawInfo.optionsNames[optionIndex] = Texts.GetEvent(ID, optionIndex);
            eventDrawInfo.optionsEnabled[optionIndex] = true;
            eventDrawInfo.optionsNamesAlt[optionIndex] = EventUtils.GenerateOptionDescription(descriptionVariants, optionIndex, description);
        
            EventUtils.FinalizeDescription(descriptionVariants, optionIndex, description, _resultNumbers);
        }
    }
}