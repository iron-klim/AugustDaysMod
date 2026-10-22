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
    // Игра за Горбачева, 22 августа: если путч провалился, президент возвращается в Москву и меняет руководство,
    // иначе — уходит в отставку «по состоянию здоровья»
    [Event(ID)]
    public class ForosReturn : CoupEvent
    {
        public const int ID = 1011;
        public const int ANSWER_1 = 0;
        public const int DESCRIPTION = 1;
        public const int TITLE = 2;
        public const int RESULT_1 = 5;

        // Тексты варианта с победой путча сдвинуты на RESIGNATION
        public const int RESIGNATION = 10;

        // Новые руководители силовых ведомств и правительства после путча
        private static readonly (Characters character, PoliticPosition position)[] NewCabinet =
        {
            (Characters.Ivashko, PoliticPosition.SecondSecretary),
            (Characters.Silaev, PoliticPosition.ChairmanOfTheCouncilOfMinisters),
            (Characters.Bakatin, PoliticPosition.ChairmanOfTheKGB),
            (Characters.Barannikov, PoliticPosition.MinisterOfInternalAffairs),
            (Characters.Shaposhnikov, PoliticPosition.MinisterOfDefense),
            // Министерства до конца года ведет Комитет по оперативному управлению народным хозяйством
            (Characters.Luzhkov, PoliticPosition.MinisterOfAgriculture),
            (Characters.Volsky, PoliticPosition.MinisterOfIndustry),
        };

        // Вариант запоминается при показе: эффекты выбранного ответа игра применяет раньше FinishEvent
        private int shownOffset;

        public ForosReturn() : base(new DateTime(1991, 8, 22))
        {
        }

        protected override bool IsGorbachevBranch => true;

        public override bool CheckConditions(GameState gameState)
        {
            return base.CheckConditions(gameState)
                && gameState.eventsDone.ContainsKey(ForosPlane.ID)
                && GkchpConspiracy.Find(gameState) != null;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            shownOffset = GkchpConspiracy.IsWinning(gameState) ? RESIGNATION : 0;
            Characters minister = shownOffset == RESIGNATION ? Characters.Yanayev : Characters.Rutskoy;
            return StartOptions(gameState, eventDrawInfo, 1, Texts.GetEvent(ID, shownOffset + TITLE), Texts.GetEvent(ID, shownOffset + DESCRIPTION),
                minister, shownOffset);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            if (shownOffset == RESIGNATION)
            {
                EndingsManager.inst.ExecuteEnding(GorbachevResignation.ID, gameState);
            }
            else
            {
                GkchpConspiracy.End(gameState);
            }

            return Texts.GetEvent(ID, shownOffset + RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
            if (shownOffset == RESIGNATION)
            {
                description.AddText("Игра будет окончена", "#FF0000");
                return;
            }

            // Члены ГКЧП арестованы, Пуго застрелился
            foreach (Characters member in GkchpConspiracy.Members)
            {
                description.AddDeleteCharacter(gameState, member, member == Characters.Pugo);
            }

            foreach (var (character, position) in NewCabinet)
            {
                if (gameState.characters[character].status != CharacterInfo.PoliticStatus.Alive)
                {
                    description.AddCharacter(gameState, character);
                }

                description.AddCharacterTakesPost(gameState, character, position, true);
            }
        }
    }
}
