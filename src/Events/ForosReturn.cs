using System;
using System.Linq;
using AugustDaysMod.Assets;
using AugustDaysMod.Scenarios;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    // Игра за Горбачева: путч провалился, президент возвращается в Москву и меняет руководство
    [Event(ID)]
    public class ForosReturn : CoupEvent
    {
        public const int ID = 1011;
        public const int ANSWER_1 = 0;
        public const int DESCRIPTION = 1;
        public const int TITLE = 2;
        public const int RESULT_1 = 5;

        // Новые руководители силовых ведомств и правительства после путча
        private static readonly (Characters character, PoliticPosition position)[] NewCabinet =
        {
            (Characters.Ivashko, PoliticPosition.SecondSecretary),
            (Characters.Silaev, PoliticPosition.ChairmanOfTheCouncilOfMinisters),
            (Characters.Bakatin, PoliticPosition.ChairmanOfTheKGB),
            (Characters.Barannikov, PoliticPosition.MinisterOfInternalAffairs),
            (Characters.Shaposhnikov, PoliticPosition.MinisterOfDefense),
        };

        public ForosReturn() : base(new DateTime(1991, 8, 22))
        {
        }

        protected override bool IsGorbachevBranch => true;

        public override bool CheckConditions(GameState gameState)
        {
            return base.CheckConditions(gameState)
                && gameState.eventsDone.ContainsKey(ForosIsolation.ID)
                && GkchpConspiracy.Find(gameState) != null;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            return StartOptions(gameState, eventDrawInfo, 1, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), Characters.Rutskoy);
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            // К этому моменту эффекты варианта могли уже убрать заговорщиков, поэтому ищем заговор без проверки состава
            ConspiracyInfo conspiracy = gameState.conspiracies.FirstOrDefault(GkchpConspiracy.IsGkchp);
            if (conspiracy != null)
            {
                ConspiracyUtils.EndConspiracy(gameState, conspiracy, false);
            }

            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, RESULT_START + choice);
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
        {
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

            // Победителем из августа вышел Ельцин, а не союзный президент
            description.AddPoliticPower(15, Characters.Yeltsin);
            description.AddPoliticPower(-10, Characters.Gorbachev);
        }
    }
}
