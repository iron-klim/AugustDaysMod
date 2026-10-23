using AugustDaysMod.Assets;
using System;
using System.Collections.Generic;
using AugustDaysMod.Storage;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Story
{
    // Сюжетное событие мода для конкретной страны игрока. Тексты берутся из assets/story.json по номеру события,
    // код задает только сроки, условия и эффекты. Наследники помечаются атрибутом [Event(ID)]
    public abstract class StoryEvent : AbstractEvent
    {
        private static readonly List<UnitOfWork[]> NoDescriptionVariants = new List<UnitOfWork[]>();

        private readonly EventInfo eventInfo;
        private readonly DateTime startDate;
        private readonly DateTime endDate;

        // Вариант текста, выбранный при показе: эффекты выбранного ответа игра применяет раньше FinishEvent
        protected string ShownVariant { get; private set; }

        protected StoryEvent(Countries? country, DateTime startDate, DateTime? endDate = null, EventType type = EventType.Dialog)
        {
            this.startDate = startDate;
            this.endDate = endDate ?? DateTime.MaxValue;
            // Кризисные события проверяются каждый день, обычные — только в отдельные дни
            eventInfo = new EventInfo
            {
                isCrisis = true,
                priority = 0,
                startDate = startDate,
                endDate = this.endDate,
                countryLimitation = country,
                eventType = type,
            };
        }

        public override EventInfo EventInfo => eventInfo;

        protected static int Flag(GameState gameState, string key) => ModStorage.For(gameState).Flag(key);

        protected static bool HasFlag(GameState gameState, string key) => ModStorage.For(gameState).flags.ContainsKey(key);

        protected static bool Done(GameState gameState, int id) => gameState.eventsDone.ContainsKey(id);

        protected static bool UnionDissolved(GameState gameState) => SovietUnion.Data(gameState).dissolved;

        // Персонаж из справочника, а если его уже нет в игре — правитель
        public static Characters CharacterOrRuler(GameState gameState, string reference)
        {
            Characters? character = Catalog.CountryLeadership.Resolve(reference);
            return character.HasValue && gameState.characters.TryGetValue(character.Value, out CharacterInfo info) &&
                   info.status == CharacterInfo.PoliticStatus.Alive
                ? character.Value
                : gameState.gameCharacter;
        }

        protected virtual int OptionsCount(GameState gameState) => StoryTexts.Event(Id, Variant(gameState)).options.Count;

        // Вариант текста события, если их несколько
        protected virtual string Variant(GameState gameState) => null;

        protected virtual object[] DescriptionArgs(GameState gameState) => Array.Empty<object>();

        protected virtual Characters Speaker(GameState gameState) => gameState.gameCharacter;

        protected virtual bool Condition(GameState gameState) => true;

        protected virtual void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
        }

        protected virtual bool IsOptionEnabled(GameState gameState, int optionIndex) => true;

        // Дополнительные действия после выбора, когда эффекты уже применены
        protected virtual void AfterChoice(GameState gameState, int choice)
        {
        }

        public override bool CheckConditions(GameState gameState)
        {
            return (!eventInfo.countryLimitation.HasValue || gameState.playerCountry == eventInfo.countryLimitation.Value)
                && !gameState.eventsDone.ContainsKey(Id)
                && gameState.date >= startDate
                && gameState.date <= endDate
                && Condition(gameState);
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            ShownVariant = Variant(gameState);
            StoryEventText text = StoryTexts.Event(Id, ShownVariant);
            int count = Math.Max(1, text.options.Count);
            object[] args = DescriptionArgs(gameState);
            string description = args.Length > 0 ? string.Format(text.description, args) : text.description;

            EventUtils.SetupEventDrawInfo(eventDrawInfo, Id, eventInfo, count, text.title, description, Speaker(gameState));
            var effects = new UnitOfWork[count];
            for (int index = 0; index < count; ++index)
            {
                effects[index] = UnitOfWork.StartCreation();
                SetupOption(gameState, index, effects[index]);
                eventDrawInfo.optionsNames[index] = index < text.options.Count ? text.options[index].name : ModLanguage.Pick("Далее", "Next");
                eventDrawInfo.optionsEnabled[index] = IsOptionEnabled(gameState, index);
                eventDrawInfo.optionsNamesAlt[index] = EventUtils.GenerateOptionDescription(NoDescriptionVariants, index, effects[index]);
            }

            return effects;
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, Id, choice);
            AfterChoice(gameState, choice);
            StoryEffects.ExecutePendingEnding(gameState);
            StoryEventText text = StoryTexts.Event(Id, ShownVariant);
            return choice < text.options.Count ? text.options[choice].result : "";
        }
    }
}
