using System;
using System.Collections.Generic;
using AugustDaysMod.Assets;
using AugustDaysMod.Endings;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Utils;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;

namespace AugustDaysMod.Events
{
    [Event(ID)]
    public class Final : CoupEvent
    {
        public const int ID = 1008;
        public const int TITLE = 0;
        public const int DESCRIPTION_OK = 1;
        public const int DESCRIPTION_FAIL = 2;
        public const int ANSWER_1 = 3;
        public const int RESULT_1 = 4;
        public const int YELTSIN_ALIVE = 5;
        public const int YELTSIN_INACTIVE = 6;

        private static readonly Countries[] LoyalAfterYeltsinDeath =
        {
            Countries.Russia, Countries.Ukraine, Countries.Armenia, Countries.Georgia, Countries.Belorussia, Countries.Moldavia
        };

        public Final() : base(new DateTime(1991, 8, 22))
        {
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            const int optionsCount = 1;
            string eventTitle = Texts.GetEvent(ID, TITLE);
            string eventDesc;
            if (CalculateWin(gameState))
            {
                eventDesc = Texts.GetEvent(ID, DESCRIPTION_OK);
            }
            else
            {
                eventDesc = string.Format(Texts.GetEvent(ID, DESCRIPTION_FAIL), PoliticUtils.GetPoliticFullName(gameState,  gameState.politicPositions[PoliticPosition.President].Value));
            }

            Characters minister = gameState.politicPositions[PoliticPosition.SecondSecretary].Value;
            EventUtils.SetupEventDrawInfo(eventDrawInfo, ID, EventInfo, optionsCount, eventTitle, eventDesc, minister);
            eventDrawInfo.optionsNames[0] = Texts.GetEvent(ID, ANSWER_1);
            eventDrawInfo.optionsEnabled[0] = true;

            TextFormatter textFormatter = TextFormatter.StartCreation();
            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.NotPresented)
            {
                textFormatter.AddCharacter(gameState, Characters.Yeltsin, false);
            }
            textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "success"]);
            textFormatter.AddDeleteCharacter(gameState, Characters.Gorbachev, true);
            foreach (var change in GetLeaderChanges(gameState))
            {
                textFormatter.AddChangeLeaderType(gameState, change.Key, change.Value);
            }

            textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "lost"]);
            textFormatter.AddText(ModLanguage.Pick("Игра будет окончена", "The game will end"));

            eventDrawInfo.optionsNamesAlt[0] = textFormatter.Finalize();

            return new UnitOfWork[optionsCount];
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            if (!CalculateWin(gameState))
            {
                EndingsManager.inst.ExecuteEnding(LiberalVictory.ID, gameState);
            }

            EventUtils.ExecuteEventEffects(gameState, ID, choice);

            PoliticUtils.RemovePolitician(gameState, Characters.Gorbachev);

            foreach (var change in GetLeaderChanges(gameState))
            {
                gameState.countries[change.Key].leaderType = change.Value;
            }

            var text = Texts.GetEvent(ID, RESULT_1) + GetYeltsinText(gameState);

            CharacterInfo yeltsin = gameState.characters[Characters.Yeltsin];
            if (yeltsin.status == CharacterInfo.PoliticStatus.NotPresented)
            {
                yeltsin.status = CharacterInfo.PoliticStatus.Alive;
            }

            return text;
        }

        // Смена лидеров республик по итогам путча. Используется и для описания варианта, и для его применения
        private static IEnumerable<KeyValuePair<Countries, Country.LeaderType>> GetLeaderChanges(GameState gameState)
        {
            CharacterInfo.PoliticStatus yeltsinStatus = gameState.characters[Characters.Yeltsin].status;

            if (yeltsinStatus != CharacterInfo.PoliticStatus.Alive && gameState.RadicalsPower > 40)
            {
                yield return new KeyValuePair<Countries, Country.LeaderType>(Countries.Russia, Country.LeaderType.Moderate);
            }

            if (yeltsinStatus == CharacterInfo.PoliticStatus.Dead && gameState.RadicalsPower < 40)
            {
                foreach (Countries country in LoyalAfterYeltsinDeath)
                {
                    yield return new KeyValuePair<Countries, Country.LeaderType>(country, Country.LeaderType.Loyal);
                }

                foreach (Countries country in BalticStates)
                {
                    Country state = gameState.countries[country];
                    if (state.puppetType == Country.PuppetType.InnerState && state.leaderType != Country.LeaderType.Separatist)
                    {
                        yield return new KeyValuePair<Countries, Country.LeaderType>(country, Country.LeaderType.Loyal);
                    }
                }
            }
        }

        private static bool CalculateWin(GameState gameState)
        {
            return gameState.RadicalsPower < 50 &&  gameState.MilitaryStaffLoyalty >= 60 && gameState.SpecialServicesLoyalty >= 60;
        }

        private static string GetYeltsinText(GameState gameState)
        {
            switch (gameState.characters[Characters.Yeltsin].status)
            {
                case CharacterInfo.PoliticStatus.Alive:
                    return Texts.GetEvent(ID, YELTSIN_ALIVE);
                case CharacterInfo.PoliticStatus.NotPresented:
                    return Texts.GetEvent(ID, YELTSIN_INACTIVE);
                default:
                    return "";
            }
        }
    }
}
