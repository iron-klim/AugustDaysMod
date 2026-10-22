using System;
using System.Collections.Generic;
using AugustDaysMod.Assets;
using AugustDaysMod.Scenarios;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    // Общая часть событий путча. Наследники помечаются атрибутом [Event(ID)] — по нему игра сама регистрирует событие
    public abstract class CoupEvent : AbstractEvent
    {
        public const int RESULT_START = 5;

        protected static readonly Countries[] BalticStates = { Countries.Estonia, Countries.Latvia, Countries.Litva };

        // Варианты описаний у наших событий не используются, поэтому передаём в утилиты игры пустой список
        private static readonly List<UnitOfWork[]> NoDescriptionVariants = new List<UnitOfWork[]>();

        private readonly EventInfo eventInfo;

        protected CoupEvent(DateTime startDate)
        {
            // Делаем событие кризисным, потому что обычные проверяются только в определенные дни, а нам нужно каждый день
            eventInfo = new EventInfo()
            {
                isCrisis = true,
                priority = 0,
                startDate = startDate,
                endDate = startDate.AddDays(1),
                countryLimitation = Countries.USSR,
                eventType = EventType.Dialog,
            };
        }

        public override EventInfo EventInfo => eventInfo;

        // События путча разделены на ветку ГКЧП и ветку Горбачева
        protected virtual bool IsGorbachevBranch => false;

        public override bool CheckConditions(GameState gameState)
        {
            // Используем кастомное условие потому, что у кризисных событий не проверяются даты и факт выполнения
            return GkchpConspiracy.IsGorbachevGame(gameState) == IsGorbachevBranch
                && !gameState.eventsDone.ContainsKey(Id)
                && gameState.date >= eventInfo.startDate;
        }

        // Заполняет окно события вариантами ответа, эффекты которых задает SetupOption
        protected UnitOfWork[] StartOptions(
            GameState gameState,
            EventDrawInfo eventDrawInfo,
            int optionsCount,
            string eventTitle,
            string eventDesc,
            Characters minister,
            int optionsTextOffset = 0)
        {
            EventUtils.SetupEventDrawInfo(eventDrawInfo, Id, eventInfo, optionsCount, eventTitle, eventDesc, minister);
            UnitOfWork[] effects = new UnitOfWork[optionsCount];

            for (int index = 0; index < optionsCount; ++index)
            {
                effects[index] = UnitOfWork.StartCreation();
                SetupOption(gameState, index, effects[index]);

                eventDrawInfo.optionsNames[index] = Texts.GetEvent(Id, optionsTextOffset + index);
                eventDrawInfo.optionsEnabled[index] = IsOptionEnabled(gameState, index);
                eventDrawInfo.optionsNamesAlt[index] = EventUtils.GenerateOptionDescription(NoDescriptionVariants, index, effects[index]);
            }

            return effects;
        }

        protected virtual void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
        }

        protected virtual bool IsOptionEnabled(GameState gameState, int optionIndex) => true;
    }
}
