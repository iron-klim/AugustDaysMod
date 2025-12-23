using Backend.Gamedesign;
using Backend.Gamedesign.ScenarioSettings.Scenarios.GameScenarios;
using HarmonyLib;
using System;

namespace AugustDaysMod.Scenarios
{
    [HarmonyPatch(typeof(ScenarioUSSR1985), nameof(ScenarioUSSR1985.SetUpScenario))]
    public static class NewGameControllerSetYearPatch
    {
        static void Postfix(
          GameState gameState,
          GameScenario gameScenario,
          GameRules gameRules)
        {
            Characters? president = gameState.politicPositions[PoliticPosition.President];
            if (!president.HasValue)
            {
                return;
            }

            gameState.characters[president.Value].Power += 10;

            AssignByPresident(gameState, president.Value);

            ModificatorState modificatorState = new ModificatorState()
            {
                endingDate = gameState.date.AddMonths(3),
                level = 1
            };
            gameState.activeModificators[91] = modificatorState;

            gameState.characters[Characters.Yeltsin].traits.Remove(Traits.ProReformist);
            gameState.characters[Characters.Yeltsin].traits.Add(Traits.ProNationalDemocrat);

            AssignPolitburo(gameState);
            AssignCabinet(gameState);


            gameState.MilitaryStaffLoyalty = 70;
            gameState.SpecialServicesLoyalty = 70;
            gameState.RadicalsPower = 50;

            //Наджибулла у власти
            gameState.eventsDone.Add(153, new EventCompletionState(1, 0, DateTime.MaxValue, new DateTime(1986, 5, 4)));

            //Отмена призыва студентов в армию
            var descion = new Backend.Gamedesign.DecisionsSystem.Decisions.GameDecisions.Decision118();
            descion.DoDecisionAfterTransitionEnded(gameState);

            gameState.endedWars.Add(Wars.EthiopianWar, new WarEndingInfo() { endingNum = 2, warResultStatus=WarInfo.WarResultStatus.SecondSideWin});
            gameState.endedWars.Add(Wars.SomaliWar, new WarEndingInfo() { endingNum = 0, warResultStatus = WarInfo.WarResultStatus.SecondSideWin });

        }

        private static void AssignByPresident(GameState gameState, Characters president)
        {
            switch (president)
            {
                case Characters.Gorbachev:
                    // gameState.politicPositions[Characters.Yanayev] =  PoliticPosition.SecondSecretary;
                    // gameState.politicPositions[Characters.Pugo] =  PoliticPosition.MinisterOfInternalAffairs;
                    // gameState.politicPositions[Characters.Lukyanov] =  PoliticPosition.ChairmanOfTheSupremeCouncil;

                    gameState.politicPositions[Characters.Yanayev] = PoliticPosition.President;
                    gameState.politicPositions[Characters.Pugo] = PoliticPosition.MinisterOfInternalAffairs;
                    gameState.politicPositions[Characters.Lukyanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
                    gameState.politicPositions[Characters.Baklanov] = PoliticPosition.SecondSecretary;

                    break;
                case Characters.Lukyanov:
                    gameState.politicPositions[Characters.Yanayev] = PoliticPosition.SecondSecretary;
                    gameState.politicPositions[Characters.Pugo] = PoliticPosition.MinisterOfInternalAffairs;
                    gameState.politicPositions[Characters.Baklanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
                    break;
                case Characters.Yanayev:
                    gameState.politicPositions[Characters.Pugo] = PoliticPosition.MinisterOfInternalAffairs;
                    gameState.politicPositions[Characters.Lukyanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
                    gameState.politicPositions[Characters.Baklanov] = PoliticPosition.SecondSecretary;
                    break;
                case Characters.Pugo:
                    gameState.politicPositions[Characters.Yanayev] = PoliticPosition.SecondSecretary;
                    gameState.politicPositions[Characters.Lukyanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
                    gameState.politicPositions[Characters.Varennikov] = PoliticPosition.MinisterOfInternalAffairs;
                    break;
                case Characters.Zhirinovsky:
                    gameState.politicPositions[Characters.Yanayev] = PoliticPosition.SecondSecretary;
                    gameState.politicPositions[Characters.Pugo] = PoliticPosition.MinisterOfInternalAffairs;
                    gameState.politicPositions[Characters.Lukyanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
                    break;
                case Characters.CustomCharacter:
                    gameState.politicPositions[Characters.Yanayev] = PoliticPosition.SecondSecretary;
                    gameState.politicPositions[Characters.Pugo] = PoliticPosition.MinisterOfInternalAffairs;
                    gameState.politicPositions[Characters.Lukyanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
                    break;
            }
        }

        private static void AssignPolitburo(GameState gameState)
        {
            gameState.politBuro.Clear();
            gameState.politBuro.Add(Characters.Annus);
            gameState.politBuro.Add(Characters.Burokevicius);
            gameState.politBuro.Add(Characters.Gorbachev);
            gameState.politBuro.Add(Characters.Gurenko);
            gameState.politBuro.Add(Characters.Eremei);
            gameState.politBuro.Add(Characters.Ivashko);
            gameState.politBuro.Add(Characters.Karimov);
            gameState.politBuro.Add(Characters.Lucinschi);
            gameState.politBuro.Add(Characters.Masaliev);
            gameState.politBuro.Add(Characters.Mahkamov);
            gameState.politBuro.Add(Characters.Mutalibov);
            gameState.politBuro.Add(Characters.Nazarbaev);
            gameState.politBuro.Add(Characters.Niyazov);
            gameState.politBuro.Add(Characters.Polozkov);
            gameState.politBuro.Add(Characters.Prokofiev);
            gameState.politBuro.Add(Characters.Rubiks);
            gameState.politBuro.Add(Characters.Stroev);
            gameState.politBuro.Add(Characters.Frolov);
            gameState.politBuro.Add(Characters.Shenin);

            gameState.membersOfPolitBuro = gameState.politBuro.Count;
        }

        private static void AssignCabinet(GameState gameState)
        {
            gameState.politicPositions[Characters.Pavlov] = PoliticPosition.ChairmanOfTheCouncilOfMinisters;
            gameState.politicPositions[Characters.Bessmertnykh] = PoliticPosition.ForeignSecretary;
            gameState.politicPositions[Characters.Maslyukov] = PoliticPosition.MinisterOfFinance;
            gameState.politicPositions[Characters.Kryuchkov] = PoliticPosition.ChairmanOfTheKGB;
            gameState.politicPositions[Characters.Starodubtsev] = PoliticPosition.MinisterOfAgriculture;
            gameState.politicPositions[Characters.Tizyakov] = PoliticPosition.MinisterOfIndustry;
            gameState.politicPositions[Characters.Yagodin] = PoliticPosition.MinisterOfEducation;
            gameState.politicPositions[Characters.Zyukin] = PoliticPosition.MinisterForYouthAffairs;
            gameState.politicPositions[Characters.Yazov] = PoliticPosition.MinisterOfDefense;
            gameState.politicPositions[Characters.Mikhailov] = PoliticPosition.GRU;
        }
    }
}