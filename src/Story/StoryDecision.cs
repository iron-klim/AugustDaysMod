using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.DecisionsSystem;
using Backend.Gamedesign.DecisionsSystem.Decisions;
using Backend.Gamedesign.Utils.ConditionsFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Story
{
    // Решение мода для конкретной страны игрока. Тексты — в assets/story.json, раздел decisions.
    // Разовое решение принимается один раз, периодическое снова доступно после перерыва
    public abstract class StoryDecision : AbstractDecision
    {
        protected abstract Countries Country { get; }

        // Перерыв в месяцах. Ноль — разовое решение
        protected virtual int CooldownMonths => 0;

        protected virtual int PoliticalPowerCost => 0;

        protected virtual DecisionType Type => DecisionType.EconomicDecision;

        public override DecisionClass Class => CooldownMonths > 0 ? DecisionClass.Periodic : DecisionClass.Accepted;

        public override int SortingOrder => Id;

        protected virtual bool Available(GameState gameState) => true;

        protected virtual void AddConditions(GameState gameState, ConditionsFormatter conditions)
        {
        }

        protected abstract void AddEffects(GameState gameState, UnitOfWork effects);

        public override bool CheckDisplayConditions(GameState gameState) =>
            gameState.playerCountry == Country && Available(gameState) && !(Class == DecisionClass.Accepted && IsAccepted(gameState));

        private UnitOfWork BuildAllEffects(GameState gameState)
        {
            UnitOfWork effects = UnitOfWork.StartCreation();
            AddEffects(gameState, effects);
            if (PoliticalPowerCost > 0)
            {
                effects.AddPoliticalPower(-PoliticalPowerCost);
            }

            return effects;
        }

        public override UnitOfWork BuildEffects(GameState gameState, DecisionAction action = DecisionAction.OneTimeEffect) => BuildAllEffects(gameState);

        public override void DoDecision(GameState gameState)
        {
            BuildAllEffects(gameState).Execute(gameState);
            ActivateDecision(gameState, Class, CooldownMonths);
        }

        public override DecisionDrawInfo GetDecisionDrawInfo(GameState gameState, GameRules gameRules)
        {
            StoryEventText text = StoryTexts.Decision(Id);
            ConditionsFormatter conditions = ConditionsFormatter.StartCreation(ConditionsFormatter.MainConditionOperator.AND);
            if (PoliticalPowerCost > 0)
            {
                conditions.AddPoliticalPowerCondition(gameState, PoliticalPowerCost, ConditionsFormatter.СomparisonOperator.GreaterOE);
            }

            if (gameState.activeDecisions.TryGetValue(Id, out DecisionState state) && state.cooldown.HasValue)
            {
                conditions.AddTimeCondition(gameState, state);
            }

            AddConditions(gameState, conditions);
            (bool met, string conditionsText) = conditions.Finalize();
            return new DecisionDrawInfo
            {
                Id = Id,
                title = text.title,
                description = text.description,
                result = BuildAllEffects(gameState).NewFinalizeInColor() +
                         (CooldownMonths > 0 ? ModLanguage.Pick($"\nСнова доступно через {CooldownMonths} мес.", $"\nAvailable again in {CooldownMonths} mo.") : ""),
                conditions = conditionsText,
                conditionsMet = met,
                decisionType = Type
            };
        }
    }
}
