using System;
using Backend.Gamedesign;
using Backend.Gamedesign.ScenarioSettings.Scenarios.GameScenarios;
using HarmonyLib;

namespace AugustDaysMod.Scenarios
{
    [HarmonyPatch(typeof(ScenarioUSSR1985), nameof(ScenarioUSSR1985.SetUpScenario))]
    public static class NewGameControllerSetYearPatch
    {
      private static readonly Characters[] Politburo =
      {
        Characters.Annus, Characters.Burokevicius, Characters.Gorbachev, Characters.Gurenko, Characters.Eremei,
        Characters.Ivashko, Characters.Karimov, Characters.Lucinschi, Characters.Masaliev, Characters.Mahkamov,
        Characters.Mutalibov, Characters.Nazarbaev, Characters.Niyazov, Characters.Polozkov, Characters.Prokofiev,
        Characters.Rubiks, Characters.Stroev, Characters.Frolov, Characters.Shenin
      };

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
        // «Последствия зачистки КПСС 1983» добавляет сценарий 1985 года, к 1991 году они неактуальны
        gameState.activeModificators.Remove(99);
        // Сценарий 1985 года сам отмечает съезд партии, поэтому в state.json его нет: XXVIII съезд прошёл в июле 1990,
        // следующий очередной — через пять лет
        gameState.eventsDone[109] = new EventCompletionState(2, 0, new DateTime(1995, 7, 13), new DateTime(1990, 7, 13));

        var yeltsinTraits = gameState.characters[Characters.Yeltsin].traits;
        yeltsinTraits.Remove(Traits.ProReformist);
        yeltsinTraits.Add(Traits.ProNationalDemocrat);

        AssignPolitburo(gameState);
        AssignCabinet(gameState);


        gameState.MilitaryStaffLoyalty = 70;
        gameState.SpecialServicesLoyalty = 70;
        gameState.RadicalsPower = 50;
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
            gameState.politicPositions[Characters.Pugo] =  PoliticPosition.MinisterOfInternalAffairs;
            gameState.politicPositions[Characters.Lukyanov] =  PoliticPosition.ChairmanOfTheSupremeCouncil;
            gameState.politicPositions[Characters.Baklanov] = PoliticPosition.SecondSecretary;
            
            break;
          case Characters.Lukyanov:
            gameState.politicPositions[Characters.Yanayev] =  PoliticPosition.SecondSecretary;
            gameState.politicPositions[Characters.Pugo] =  PoliticPosition.MinisterOfInternalAffairs;
            gameState.politicPositions[Characters.Baklanov] = PoliticPosition.ChairmanOfTheSupremeCouncil;
            break;
          case Characters.Yanayev:
            gameState.politicPositions[Characters.Pugo] =  PoliticPosition.MinisterOfInternalAffairs;
            gameState.politicPositions[Characters.Lukyanov] =  PoliticPosition.ChairmanOfTheSupremeCouncil;
            gameState.politicPositions[Characters.Baklanov] = PoliticPosition.SecondSecretary;
            break;
          case Characters.Pugo:
            gameState.politicPositions[Characters.Yanayev] =  PoliticPosition.SecondSecretary;
            gameState.politicPositions[Characters.Lukyanov] =  PoliticPosition.ChairmanOfTheSupremeCouncil;
            gameState.politicPositions[Characters.Varennikov] =  PoliticPosition.MinisterOfInternalAffairs;
            break;
          case Characters.Zhirinovsky:
          case Characters.CustomCharacter:
            gameState.politicPositions[Characters.Yanayev] =  PoliticPosition.SecondSecretary;
            gameState.politicPositions[Characters.Pugo] =  PoliticPosition.MinisterOfInternalAffairs;
            gameState.politicPositions[Characters.Lukyanov] =  PoliticPosition.ChairmanOfTheSupremeCouncil;
            break;
        }
      }

      private static void AssignPolitburo(GameState gameState)
      {
        gameState.politBuro.Clear();
        gameState.politBuro.UnionWith(Politburo);
        gameState.membersOfPolitBuro = gameState.politBuro.Count;
      }

      private static void AssignCabinet(GameState gameState)
      {
        gameState.politicPositions[Characters.Pavlov] =  PoliticPosition.ChairmanOfTheCouncilOfMinisters;
        gameState.politicPositions[Characters.Bessmertnykh] = PoliticPosition.ForeignSecretary;
        gameState.politicPositions[Characters.Maslyukov] = PoliticPosition.MinisterOfFinance;
        gameState.politicPositions[Characters.Kryuchkov] =  PoliticPosition.ChairmanOfTheKGB;
        gameState.politicPositions[Characters.Starodubtsev] = PoliticPosition.MinisterOfAgriculture;
        gameState.politicPositions[Characters.Tizyakov] = PoliticPosition.MinisterOfIndustry;
        gameState.politicPositions[Characters.Yagodin] = PoliticPosition.MinisterOfEducation;
        gameState.politicPositions[Characters.Zyukin] = PoliticPosition.MinisterForYouthAffairs;
        gameState.politicPositions[Characters.Yazov] = PoliticPosition.MinisterOfDefense;
        gameState.politicPositions[Characters.Mikhailov] = PoliticPosition.GRU;
      }
    }
}