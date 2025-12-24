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


            //Судьба опального академика – I
            gameState.eventsDone.Add(42, new EventCompletionState(0, 2, DateTime.MaxValue, new DateTime(1985, 8, 29)));
            //Советская авиация под угрозой
            gameState.eventsDone.Add(358, new EventCompletionState(0, 0, DateTime.MaxValue, new DateTime(1986, 3, 1)));
            //Игры доброй воли
            gameState.eventsDone.Add(225, new EventCompletionState(0, 0, DateTime.MaxValue, new DateTime(1986, 5, 14)));
            //Смерть Ле Зуана 
            gameState.eventsDone.Add(238, new EventCompletionState(3, 1, DateTime.MaxValue, new DateTime(1986, 7, 10)));
            //Реформа армии США
            gameState.eventsDone.Add(359, new EventCompletionState(0, 0, DateTime.MaxValue, new DateTime(1986, 10, 6)));
            //Судьба опального академика – II
            gameState.eventsDone.Add(43, new EventCompletionState(3, 0, DateTime.MaxValue, new DateTime(1986, 12, 1)));
            //Усиление присутствия в Турции
            gameState.eventsDone.Add(360, new EventCompletionState(0, 0, DateTime.MaxValue, new DateTime(1987, 3, 1)));
            //Кризис агромерца
            gameState.eventsDone.Add(256, new EventCompletionState(3, 0, DateTime.MaxValue, new DateTime(1987, 8, 1)));
            //Черный понедельник
            gameState.eventsDone.Add(362, new EventCompletionState(0, 0, DateTime.MaxValue, new DateTime(1987, 10, 19)));
            //Визит в югославию
            gameState.eventsDone.Add(258, new EventCompletionState(3, 0, DateTime.MaxValue, new DateTime(1988, 3, 14)));
            //Политический кризис в Южной Корее
            gameState.eventsDone.Add(297, new EventCompletionState(1, 0, DateTime.MaxValue, new DateTime(1988, 3, 14)));
            //Пиночет
            gameState.eventsDone.Add(292, new EventCompletionState(2, 0, DateTime.MaxValue, new DateTime(1989, 9, 23)));

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