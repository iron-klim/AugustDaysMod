using System;
using System.Collections.Generic;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    public class YeltsinProblem : AbstractEvent
    {
        public const int ID = 1003;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;
        public const int RESULT_3 = 7;
        
        private EventInfo eventInfo = new EventInfo()
        {
            priority = 0,
            startDate = new DateTime(1991, 8, 19),
            endDate = new DateTime(1991, 8, 20),
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
            Characters minister = gameState.politicPositions[PoliticPosition.ChairmanOfTheKGB].Value;
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
            EventManager.inst.ProccessEventImmediately(gameState, Broadcast.ID, false);
            return Texts.GetEvent(ID, Manager.RESULT_START + choice);
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
                    description.AddDeleteCharacter(gameState, Characters.Yeltsin, false);
                    break;
                case ANSWER_2:
                    description.AddDeleteCharacter(gameState, Characters.Yeltsin, true);
                    description.AddSpecialServicesLoyalty(-5);
                    break;
                case ANSWER_3:
                    break;
            }
        
            eventDrawInfo.optionsNames[optionIndex] = Texts.GetEvent(ID, optionIndex);
            eventDrawInfo.optionsEnabled[optionIndex] = true;
            eventDrawInfo.optionsNamesAlt[optionIndex] = EventUtils.GenerateOptionDescription(descriptionVariants, optionIndex, description);
        
            EventUtils.FinalizeDescription(descriptionVariants, optionIndex, description, _resultNumbers);
        }
    }
}