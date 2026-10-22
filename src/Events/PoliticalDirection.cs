using System;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.DoctrinesSystem.Doctrines;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    [Event(ID)]
    public class PoliticalDirection : CoupEvent
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

        public PoliticalDirection() : base(new DateTime(1991, 8, 19))
        {
        }

        // Вызывается только из Broadcast
        public override bool CanBeAddedOnlyManually => true;

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            Characters minister = gameState.politicPositions[PoliticPosition.SecondSecretary].Value;
            return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), minister);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);

            SpeedUpTransition(gameState, DoctrineType.TypeOfEconomy);
            SpeedUpTransition(gameState, DoctrineType.TypeOfGovernment);

            Characters president = gameState.politicPositions[PoliticPosition.President].Value;
            Characters mvd = gameState.politicPositions[PoliticPosition.MinisterOfInternalAffairs].Value;

            switch (choice)
            {
                case ANSWER_1:
                    string presidentSurname = PoliticUtils.GetSurname(gameState, president);
                    return string.Format(Texts.GetEvent(ID, RESULT_START + ANSWER_1), presidentSurname, presidentSurname, PoliticUtils.GetPoliticFullName(gameState, president));
                case ANSWER_2:
                    return string.Format(Texts.GetEvent(ID, RESULT_START + ANSWER_2), PoliticUtils.GetSurname(gameState, president), PoliticUtils.GetSurname(gameState, mvd));
            }

            return Texts.GetEvent(ID, RESULT_START + choice);
        }

        // Сокращаем оставшееся время перехода доктрины в 6 раз
        private static void SpeedUpTransition(GameState gameState, DoctrineType doctrine)
        {
            if (gameState.doctrineTransitions.TryGetValue(doctrine, out var transition))
            {
                TimeSpan reducedDiff = TimeSpan.FromTicks((transition.transitionEndDate - gameState.date).Ticks / 6);
                transition.transitionEndDate = gameState.date.Add(reducedDiff);
            }
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
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
        }
    }
}
