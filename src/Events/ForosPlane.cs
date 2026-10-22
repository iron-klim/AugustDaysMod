using System;
using AugustDaysMod.Assets;
using AugustDaysMod.Endings;
using AugustDaysMod.Scenarios;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Utils;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    // Игра за Горбачева, 21 августа: в Форос летят и заговорщики, и спасатели.
    // Если путч берет верх, вместо самолета Руцкого президенту предъявляют ультиматум
    [Event(ID)]
    public class ForosPlane : CoupEvent
    {
        public const int ID = 1014;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int DESCRIPTION = 3;
        public const int TITLE = 4;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;

        // Тексты варианта с ультиматумом сдвинуты на ULTIMATUM
        public const int ULTIMATUM = 10;

        // Вариант запоминается при показе: эффекты выбранного ответа игра применяет раньше FinishEvent
        private int shownOffset;

        public ForosPlane() : base(new DateTime(1991, 8, 21))
        {
        }

        protected override bool IsGorbachevBranch => true;

        public override bool CheckConditions(GameState gameState)
        {
            return base.CheckConditions(gameState)
                && gameState.eventsDone.ContainsKey(WhiteHouseDefense.ID)
                && GkchpConspiracy.Find(gameState) != null;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            shownOffset = GkchpConspiracy.IsWinning(gameState) ? ULTIMATUM : 0;
            return StartOptions(gameState, eventDrawInfo, 2, Texts.GetEvent(ID, shownOffset + TITLE), Texts.GetEvent(ID, shownOffset + DESCRIPTION),
                Characters.Kryuchkov, shownOffset);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            if (shownOffset == ULTIMATUM && choice == ANSWER_1)
            {
                EndingsManager.inst.ExecuteEnding(GorbachevResignation.ID, gameState);
            }

            return Texts.GetEvent(ID, shownOffset + RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            if (shownOffset == ULTIMATUM)
            {
                if (optionIndex == ANSWER_1)
                {
                    description.AddText("Игра будет окончена", "#FF0000");
                }
                else
                {
                    description.AddCoupPower(-5);
                }

                return;
            }

            switch (optionIndex)
            {
                case ANSWER_1:
                    // В Москву президента привозит российское руководство
                    description.AddPoliticPower(15, Characters.Yeltsin);
                    description.AddPoliticPower(-10, Characters.Gorbachev);
                    break;
                case ANSWER_2:
                    // Президент сам принимает капитуляцию заговорщиков
                    description.AddPoliticPower(5, Characters.Yeltsin);
                    description.AddPoliticPower(5, Characters.Gorbachev);
                    description.AddMilitaryStaffLoyalty(5);
                    break;
            }
        }
    }
}
