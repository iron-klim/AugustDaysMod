using System;
using AugustDaysMod.Assets;
using AugustDaysMod.Endings;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Utils;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Managers;

namespace AugustDaysMod.Events
{
    public class Final : AbstractEvent
    {
        public const int ID = 1008;
        public const int TITLE = 0;
        public const int DESCRIPTION_OK = 1;
        public const int DESCRIPTION_FAIL = 2;
        public const int ANSWER_1 = 3;
        public const int RESULT_1 = 4;
        public const int YELTSIN_ALIVE = 5;
        public const int YELTSIN_INACTIVE = 6;
        
        private EventInfo eventInfo = new EventInfo()
        {
            priority = 0,
            startDate = new DateTime(1991, 8, 22),
            endDate = new DateTime(1991, 8, 23),
            countryLimitation = Countries.USSR,
            eventType = EventType.Dialog,
        };
        
        public override EventInfo EventInfo => eventInfo;
        
        public override bool CheckConditions(GameState gameState)
        {
            return !gameState.eventsDone.ContainsKey(ID) && gameState.date >= eventInfo.startDate;
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
            EventUtils.SetupEventDrawInfo(eventDrawInfo, ID, eventInfo, optionsCount, eventTitle, eventDesc, minister);
            UnitOfWork[] unitOfWorkArray = new UnitOfWork[optionsCount];
            eventDrawInfo.optionsNames[0] = Texts.GetEvent(ID, ANSWER_1);
            eventDrawInfo.optionsEnabled[0] = true;
            
            TextFormatter textFormatter = TextFormatter.StartCreation();
            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.NotPresented)
            {
                textFormatter.AddCharacter(gameState, Characters.Yeltsin, false);
            }
            textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "success"]);
            textFormatter.AddDeleteCharacter(gameState, Characters.Gorbachev, true);
            if (gameState.characters[Characters.Yeltsin].status != CharacterInfo.PoliticStatus.Alive && gameState.RadicalsPower > 40)
            {
                textFormatter.AddChangeLeaderType(gameState, Countries.Russia, Country.LeaderType.Moderate);
            }

            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.Dead && gameState.RadicalsPower < 40)
            {
                textFormatter.AddChangeLeaderType(gameState, Countries.Russia, Country.LeaderType.Loyal);
                textFormatter.AddChangeLeaderType(gameState, Countries.Ukraine, Country.LeaderType.Loyal);
                textFormatter.AddChangeLeaderType(gameState, Countries.Armenia, Country.LeaderType.Loyal);
                textFormatter.AddChangeLeaderType(gameState, Countries.Georgia, Country.LeaderType.Loyal);
                textFormatter.AddChangeLeaderType(gameState, Countries.Belorussia, Country.LeaderType.Loyal);
                textFormatter.AddChangeLeaderType(gameState, Countries.Moldavia, Country.LeaderType.Loyal);
                if (gameState.countries[Countries.Estonia].puppetType == Country.PuppetType.InnerState && gameState.countries[Countries.Estonia].leaderType != Country.LeaderType.Separatist)
                {
                    textFormatter.AddChangeLeaderType(gameState, Countries.Estonia, Country.LeaderType.Loyal);
                }
                if (gameState.countries[Countries.Latvia].puppetType == Country.PuppetType.InnerState && gameState.countries[Countries.Latvia].leaderType != Country.LeaderType.Separatist)
                {
                    textFormatter.AddChangeLeaderType(gameState, Countries.Latvia, Country.LeaderType.Loyal);
                }
                if (gameState.countries[Countries.Litva].puppetType == Country.PuppetType.InnerState && gameState.countries[Countries.Litva].leaderType != Country.LeaderType.Separatist)
                {
                    textFormatter.AddChangeLeaderType(gameState, Countries.Litva, Country.LeaderType.Loyal);
                }
            }
            
            textFormatter.AddText(LanguageManager.inst["conspiracy_desc", "lost"]);
            textFormatter.AddText("Игра будет окончена");

            eventDrawInfo.optionsNamesAlt[0] = textFormatter.Finalize();
            
            return unitOfWorkArray;
        }
        
        public override string FinishEvent(GameState gameState, int choice)
        {
            if (!CalculateWin(gameState))
            {
                EndingsManager.inst.ExecuteEnding(LiberalVictory.ID, gameState);
            }

            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            
            PoliticUtils.RemovePolitician(gameState, Characters.Gorbachev);
            
            if (gameState.characters[Characters.Yeltsin].status != CharacterInfo.PoliticStatus.Alive && gameState.RadicalsPower > 40)
            {
                gameState.countries[Countries.Russia].leaderType = Country.LeaderType.Moderate;
            }
            
            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.Dead && gameState.RadicalsPower < 40)
            {
                gameState.countries[Countries.Russia].leaderType = Country.LeaderType.Loyal;
                gameState.countries[Countries.Ukraine].leaderType = Country.LeaderType.Loyal;
                gameState.countries[Countries.Armenia].leaderType = Country.LeaderType.Loyal;
                gameState.countries[Countries.Georgia].leaderType = Country.LeaderType.Loyal;
                gameState.countries[Countries.Belorussia].leaderType = Country.LeaderType.Loyal;
                gameState.countries[Countries.Moldavia].leaderType = Country.LeaderType.Loyal;
                if (gameState.countries[Countries.Estonia].puppetType == Country.PuppetType.InnerState && gameState.countries[Countries.Estonia].leaderType != Country.LeaderType.Separatist)
                {
                    gameState.countries[Countries.Estonia].leaderType = Country.LeaderType.Loyal;
                }
                if (gameState.countries[Countries.Latvia].puppetType == Country.PuppetType.InnerState && gameState.countries[Countries.Latvia].leaderType != Country.LeaderType.Separatist)
                {
                    gameState.countries[Countries.Latvia].leaderType = Country.LeaderType.Loyal;
                }
                if (gameState.countries[Countries.Litva].puppetType == Country.PuppetType.InnerState && gameState.countries[Countries.Litva].leaderType != Country.LeaderType.Separatist)
                {
                    gameState.countries[Countries.Litva].leaderType = Country.LeaderType.Loyal;
                }
            }
            
            var text = Texts.GetEvent(ID, RESULT_1) + GetYeltsinText(gameState);

            if (gameState.characters[Characters.Yeltsin].status == CharacterInfo.PoliticStatus.NotPresented)
            {
                gameState.characters[Characters.Yeltsin].status = CharacterInfo.PoliticStatus.Alive;
            }

            return text;
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
                    break;
            }
            
            return "";
        }
    }
}