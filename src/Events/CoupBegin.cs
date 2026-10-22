using System;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    [Event(ID)]
    public class CoupBegin : CoupEvent
    {
      public const int ID = 1001;
      public const int TITLE = 4;
      public const int DESCRIPTION = 3;
      public const int ANSWER_1 = 0;
      public const int ANSWER_2 = 1;
      public const int ANSWER_3 = 2;
      public const int RESULT_1 = 5;
      public const int RESULT_2 = 6;
      public const int RESULT_3 = 7;

      public CoupBegin() : base(new DateTime(1991, 8, 18))
      {
      }

      public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
      {
        Characters minister = gameState.politicPositions[PoliticPosition.ChairmanOfTheKGB].Value;
        return StartOptions(gameState, eventDrawInfo, 3, Texts.GetEvent(ID, TITLE), Texts.GetEvent(ID, DESCRIPTION), minister);
      }

      public override string FinishEvent(GameState gameState, int choice)
      {
        EventUtils.ExecuteEventEffects(gameState, ID, choice);
        return Texts.GetEvent(ID, RESULT_START + choice);
      }

      protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork description)
      {
        switch (optionIndex)
        {
          case ANSWER_1:
            description.AddSpecialServices(5);
            break;
          case ANSWER_2:
            description.ChangeDoctrineToOnePoint(gameState, DoctrineType.ControlOfMedia, true);
            description.AddSpecialServices(-5);
            description.AddSpecialServicesLoyalty(-10);
            break;
          case ANSWER_3:
            foreach (Countries country in BalticStates)
            {
              description.AddCountryRelations(gameState, 15, country);
              description.AddDiplomacyVector(gameState, 15, country);
            }

            foreach (Countries country in BalticStates)
            {
              description.AddChangeLeaderType(gameState, country, Country.LeaderType.Moderate);
            }
            break;
        }
      }
    }
}
