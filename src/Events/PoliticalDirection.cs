using System;
using System.Collections.Generic;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.DoctrinesSystem.Doctrines;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    public class PoliticalDirection : AbstractEvent
    {
        public const int ID = 1005;
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
            
            if (gameState.doctrineTransitions.ContainsKey(DoctrineType.TypeOfEconomy))
            {
                DateTime date1 = gameState.doctrineTransitions[DoctrineType.TypeOfEconomy].transitionEndDate;
                TimeSpan reducedDiff = TimeSpan.FromTicks((date1 - gameState.date).Ticks / 6);
                gameState.doctrineTransitions[DoctrineType.TypeOfEconomy].transitionEndDate = gameState.date.Add(reducedDiff);
            }
            
            if (gameState.doctrineTransitions.ContainsKey(DoctrineType.TypeOfGovernment))
            {
                DateTime date1 = gameState.doctrineTransitions[DoctrineType.TypeOfGovernment].transitionEndDate;
                TimeSpan reducedDiff = TimeSpan.FromTicks((date1 - gameState.date).Ticks / 6);
                gameState.doctrineTransitions[DoctrineType.TypeOfGovernment].transitionEndDate = gameState.date.Add(reducedDiff);
            }

            
            Characters president = gameState.politicPositions[PoliticPosition.President].Value;
            Characters mvd = gameState.politicPositions[PoliticPosition.MinisterOfInternalAffairs].Value;
            
            switch (choice)
            {
                case ANSWER_1:
                    return string.Format(Texts.GetEvent(ID, Manager.RESULT_START + ANSWER_1), PoliticUtils.GetSurname(president), PoliticUtils.GetSurname(president), PoliticUtils.GetPoliticFullName(gameState, president));
                case ANSWER_2:
                    return string.Format(Texts.GetEvent(ID, Manager.RESULT_START + ANSWER_2), PoliticUtils.GetSurname(president), PoliticUtils.GetSurname(mvd));
            }
            
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
                    description.AddIntelligentsiaLoyalty(10);
                    break;
                case ANSWER_2:
                    description.AddChangeDoctrine(gameState, DoctrineType.TypeOfGovernment, AbstractDoctrine.DoctrineBranchType.Central, 0);
                    break;
                case ANSWER_3:
                    description.AddChangeDoctrine(gameState, DoctrineType.TypeOfGovernment, AbstractDoctrine.DoctrineBranchType.TopLeft, 0, 100);
                    description.AddChangeDoctrine(gameState, DoctrineType.AppointmentOfHeadOfState, AbstractDoctrine.DoctrineBranchType.Central, 0);
                    description.AddChangeDoctrine(gameState, DoctrineType.ChambersOfParliament, AbstractDoctrine.DoctrineBranchType.Central, 0);
                    description.AddChangeDoctrine(gameState, DoctrineType.TypeOfEconomy, AbstractDoctrine.DoctrineBranchType.Bottom, 0, 100);
                    description.AddSpecialServicesLoyalty(-10);
                    description.AddMilitaryStaffLoyalty(-10);
                    description.AddIntelligentsiaLoyalty(-10);
                    description.AddRadicalsPower(10);
                    break;
            }
        
            eventDrawInfo.optionsNames[optionIndex] = Texts.GetEvent(ID, optionIndex);
            eventDrawInfo.optionsEnabled[optionIndex] = true;
            eventDrawInfo.optionsNamesAlt[optionIndex] = EventUtils.GenerateOptionDescription(descriptionVariants, optionIndex, description);
        
            EventUtils.FinalizeDescription(descriptionVariants, optionIndex, description, _resultNumbers);
        }
    }
}