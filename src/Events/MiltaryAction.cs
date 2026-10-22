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
    public class MiltaryAction : CoupEvent
    {
        public const int ID = 1002;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;
        public const int RESULT_3 = 7;

        public MiltaryAction() : base(new DateTime(1991, 8, 19))
        {
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            Characters minister = gameState.politicPositions[PoliticPosition.MinisterOfDefense].Value;
            return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), minister);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            // Запускаем следующий ивент сразу, потому что обычным способом он появится только на следующий день, а нам нужно в тот же самый
            EventManager.inst.ProccessEventImmediately(gameState, YeltsinProblem.ID, false);
            return Texts.GetEvent(ID, RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            switch (optionIndex)
            {
                case ANSWER_1:
                    description.AddRadicalsPower(10);
                    break;
                case ANSWER_2:
                    description.AddChangeLeaderType(gameState, Countries.Ukraine, Country.LeaderType.Moderate);

                    foreach (Countries country in BalticStates)
                    {
                        description.AddChangePuppetType(gameState, country, Countries.USSR, Country.PuppetType.InnerState);
                    }
                    description.AddRadicalsPower(-10);
                    description.AddMilitaryStaffLoyalty(-10);
                    break;
                case ANSWER_3:
                    description.AddCombatability(-10);
                    break;
            }
        }
    }
}
