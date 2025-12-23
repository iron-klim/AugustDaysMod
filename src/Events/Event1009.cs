using AugustDaysMod.Assets;
using AugustDaysMod.Events;
using Backend.Gamedesign;
using Backend.Gamedesign.DoctrinesSystem.Doctrines;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.EventSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.ConditionsFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;
using Backend.Gamedesign.WarButtonsSystem;
using Backend.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugustDaysMod.src.Events
{
    //Прекращение помощи Афганистану
    [Event(1009)]
    public class Event1009 : AbstractEvent
    {
        public const int ID = 1009;
        public const int TITLE = 4;
        public const int DESCRIPTION = 3;
        public const int ANSWER_1 = 0;
        public const int ANSWER_2 = 1;
        public const int ANSWER_3 = 2;
        public const int RESULT_1 = 5;
        public const int RESULT_2 = 6;
        public const int RESULT_3 = 7;

        private EventInfo eventInfo = new EventInfo()
        {
            priority = 20,
            startDate = new DateTime(1991, 11, 1),
            endDate = new DateTime(1992, 12, 31),
            countryLimitation = Countries.USSR,
            eventType = EventType.Dialog,
        };

        public override EventInfo EventInfo => eventInfo;

        private int[] _resultNumbers = Array.Empty<int>();

        Wars warId;

        public override bool CheckConditions(GameState gameState)
        {
            if (!IfThisEventWasEverDone(gameState) && gameState.currentWars.ContainsKey(Wars.AfganWar))
            {
                return PoliticUtils.CheckAllMinisters(gameState);
            }

            return false;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            const int optionsCount = 3;
            warId = Wars.AfganWar;
            _resultNumbers = new int[optionsCount];
            string eventTitle = Texts.GetEvent(ID, TITLE);
            string eventDesc = Texts.GetEvent(ID, DESCRIPTION);
            Characters minister = gameState.politicPositions[PoliticPosition.ForeignSecretary].Value;
            EventUtils.SetupEventDrawInfo(eventDrawInfo, ID, eventInfo, optionsCount, eventTitle, eventDesc, minister);
            List<UnitOfWork[]> descriptionVariants = EventUtils.PrepareDescriptionVariants(3, optionsCount);
            UnitOfWork[] unitOfWorkArray = new UnitOfWork[optionsCount];

            for (int index = 0; index < optionsCount; ++index)
            {
                _resultNumbers[index] = 0;
                unitOfWorkArray[index] = UnitOfWork.StartCreation();
                SetupOption(gameState, eventDrawInfo, index, descriptionVariants, unitOfWorkArray[index]);
            }

            return unitOfWorkArray;
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            MarkEventAsDone(gameState, choice);
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, Manager.RESULT_START + choice);
        }

        private void SetupOption(
            GameState gameState,
            EventDrawInfo eventDrawInfo,
            int optionIndex,
            List<UnitOfWork[]> descriptionVariants,
            UnitOfWork description)
        {
            ConditionsFormatter conditionsFormatter = ConditionsFormatter.StartCreation(ConditionsFormatter.MainConditionOperator.AND);
            switch (optionIndex)
            {
                //Прекращение поддержки
                case ANSWER_1:
                    description
                        .AddBlockWarButton(gameState, warId, WarButtonsTypes.OpenAgreement, 1)
                        .AddBlockWarButton(gameState, warId, WarButtonsTypes.SecretSupport, 1)
                        .AddBlockWarButton(gameState, warId, WarButtonsTypes.SendSpecialsts, 1)
                        .AddBlockWarButton(gameState, warId, WarButtonsTypes.HumanitarianAid, 1)
                        .AddBlockWarButton(gameState, warId, WarButtonsTypes.IntroduceTroops, 1)
                        .AddWarMoraleToOurSide(gameState, -10, warId)
                        .AddWarControlToAllRegions(gameState, 3, warId, 2);
                    break;
                //Сохранение поддержки
                case ANSWER_2:
                    description
                        .AddFactionLoyalNumByList(gameState, -5, new List<Factions> { Factions.Reformist, Factions.LiberalDemocracy })
                        .AddMilitaryStaffLoyalty(-5)
                        .AddWarMoraleToOurSide(gameState, 5, warId);
                    break;
                //Расширение поддержки
                case ANSWER_3:
                    conditionsFormatter
                        .AddGeneralsLoyalCondition(gameState, 50, ConditionsFormatter.СomparisonOperator.GreaterOE)
                        .AddCombatabilityCondition(gameState, 50, ConditionsFormatter.СomparisonOperator.GreaterOE)
                        .AddReserveCondition(gameState, 10, ConditionsFormatter.СomparisonOperator.GreaterOE);
                    description
                        .AddFactionLoyalNumByList(gameState, -10, new List<Factions> { Factions.Reformist, Factions.LiberalDemocracy })
                        .AddMilitaryStaffLoyalty(-10)
                        .AddWarMoraleToOurSide(gameState, 10, warId)
                        .AddWarControlToAllRegions(gameState, 5, warId, 1)
                        .AddReserveAction(-10)
                        .AddCombatability(-5f);
                    break;
            }

            eventDrawInfo.optionsNames[optionIndex] = Texts.GetEvent(ID, optionIndex);
            eventDrawInfo.optionsConditions[optionIndex] = EventUtils.GenerateConditions(conditionsFormatter);
            eventDrawInfo.optionsEnabled[optionIndex] = EventUtils.CheckConditions(conditionsFormatter);
            eventDrawInfo.optionsNamesAlt[optionIndex] = EventUtils.GenerateOptionDescription(descriptionVariants, optionIndex, description);

            EventUtils.FinalizeDescription(descriptionVariants, optionIndex, description, _resultNumbers);
        }
    }

}

