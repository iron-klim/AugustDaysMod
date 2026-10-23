using AugustDaysMod.Assets;
using AugustDaysMod.Storage;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.DecisionsSystem;
using Backend.Gamedesign.Utils.ConditionsFormatter;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;

// Решения за Украину: деньги, армия, отношения с Россией и Западом
namespace AugustDaysMod.Story.Ukraine
{
    public abstract class UkraineDecision : StoryDecision
    {
        protected override Countries Country => Countries.Ukraine;

        protected static bool Independent(GameState gameState) =>
            ModStorage.For(gameState).Flag(StoryFlags.Independence) == 1 || SovietUnion.Data(gameState).dissolved;

        protected static void RelationsAtLeast(GameState gameState, ConditionsFormatter conditions, Countries country, int value) =>
            conditions.AddCondition(ModLanguage.Pick($"Отношения с {Backend.Gamedesign.Utils.CountryUtils.GetCountryName(gameState, country)} не ниже {value}",
                    $"Relations with {Backend.Gamedesign.Utils.CountryUtils.GetCountryName(gameState, country)} at least {value}"),
                gameState.countries[country].RelationshipWithPlayer >= value);
    }

    [Decision(ID)]
    public class UkraineImfLoan : UkraineDecision
    {
        public const int ID = 3001;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 20;

        protected override bool Available(GameState gameState) => SovietUnion.Data(gameState).dissolved;

        protected override void AddConditions(GameState gameState, ConditionsFormatter conditions) =>
            RelationsAtLeast(gameState, conditions, Countries.USA, 45);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddIMFLoan(200).AddReserveAction(200).AddLiberalizationLevel(5).AddFirstNeedsGoods(-5);
    }

    [Decision(ID)]
    public class UkraineGasDeal : UkraineDecision
    {
        public const int ID = 3002;
        protected override int CooldownMonths => 6;
        protected override int PoliticalPowerCost => 10;
        protected override DecisionType Type => DecisionType.DiplomaticDecision;

        protected override bool Available(GameState gameState) => Independent(gameState);

        protected override void AddConditions(GameState gameState, ConditionsFormatter conditions) =>
            RelationsAtLeast(gameState, conditions, Countries.Russia, 40);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddReserveAction(100).AddHeavyIndustryEffectiveness(3).AddIntelligentsiaLoyalty(-5)
                .AddCountryRelations(gameState, 5, Countries.Russia);
    }

    [Decision(ID)]
    public class UkrainePrivatization : UkraineDecision
    {
        public const int ID = 3003;
        protected override int PoliticalPowerCost => 30;

        protected override bool Available(GameState gameState) => SovietUnion.Data(gameState).dissolved;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddLiberalizationLevel(10).AddLightIndustryEffectiveness(5).AddReserveAction(80).AddCorruptionLevel(5)
                .AddCountryRelations(gameState, 10, Countries.USA);
    }

    [Decision(ID)]
    public class UkraineMoneyPrinting : UkraineDecision
    {
        public const int ID = 3004;
        protected override int CooldownMonths => 3;

        protected override bool Available(GameState gameState) => ModStorage.For(gameState).Flag(StoryFlags.OwnCurrency) == 1;

        protected override void AddEffects(GameState gameState, UnitOfWork effects)
        {
            effects.AddReserveAction(150).AddFirstNeedsGoods(-5).AddIntelligentsiaLoyalty(-5).AddAction(
                textFormatter => textFormatter.AddText(ModLanguage.Pick("Цены: +15%", "Prices: +15%"), "#FF0000"),
                (state, gameRules, gameScenario) => state.PriceIndex *= 1.15f);
        }
    }

    [Decision(ID)]
    public class UkraineNationalGuard : UkraineDecision
    {
        public const int ID = 3005;
        protected override int PoliticalPowerCost => 20;
        protected override DecisionType Type => DecisionType.ArmyDecision;

        protected override bool Available(GameState gameState) => Independent(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddOrderLevel(10).AddArmyQuantityLevel(3).AddMilitaryStaffLoyalty(5).AddReserveAction(-100);
    }

    [Decision(ID)]
    public class UkraineConversion : UkraineDecision
    {
        public const int ID = 3006;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 15;

        protected override bool Available(GameState gameState) => SovietUnion.Data(gameState).dissolved;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddLightIndustryEffectiveness(5).AddArmyIndustryEffectiveness(-5).AddReserveAction(50).AddMilitaryStaffLoyalty(-5);
    }

    [Decision(ID)]
    public class UkraineRukhSupport : UkraineDecision
    {
        public const int ID = 3007;
        protected override int CooldownMonths => 6;
        protected override DecisionType Type => DecisionType.PoliticalDecision;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddPoliticalPower(15).AddIntelligentsiaLoyalty(10).AddUnityLevel(-3).AddRadicalsPower(3)
                .AddCountryRelations(gameState, -5, Countries.Russia);
    }

    [Decision(ID)]
    public class UkraineRussiaTreaty : UkraineDecision
    {
        public const int ID = 3008;
        protected override int PoliticalPowerCost => 25;
        protected override DecisionType Type => DecisionType.DiplomaticDecision;

        protected override bool Available(GameState gameState) => SovietUnion.Data(gameState).dissolved;

        protected override void AddConditions(GameState gameState, ConditionsFormatter conditions) =>
            RelationsAtLeast(gameState, conditions, Countries.Russia, 50);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddCountryRelations(gameState, 15, Countries.Russia).AddUnityLevel(5).AddIntelligentsiaLoyalty(-10);
    }

    [Decision(ID)]
    public class UkraineAgriculture : UkraineDecision
    {
        public const int ID = 3009;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 10;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddAgroEffectiveness(5).AddFirstNeedsGoods(5).AddReserveAction(-80);
    }

    [Decision(ID)]
    public class UkraineArmyCut : UkraineDecision
    {
        public const int ID = 3010;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 10;
        protected override DecisionType Type => DecisionType.ArmyDecision;

        protected override bool Available(GameState gameState) => Independent(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddArmyQuantityLevel(-10).AddReserveAction(100).AddMilitaryStaffLoyalty(-10);
    }

    [Decision(ID)]
    public class UkraineAddressToNation : UkraineDecision
    {
        public const int ID = 3011;
        protected override int CooldownMonths => 3;
        protected override DecisionType Type => DecisionType.PoliticalDecision;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddPoliticalPower(25).AddUnityLevel(2).AddIntelligentsiaLoyalty(-3);
    }

    [Decision(ID)]
    public class UkraineCoalitionGovernment : UkraineDecision
    {
        public const int ID = 3012;
        protected override int CooldownMonths => 12;
        protected override DecisionType Type => DecisionType.PoliticalDecision;

        protected override bool Available(GameState gameState) => Independent(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddPoliticalPower(40).AddUnityLevel(5).AddRadicalsPower(-5).AddIntelligentsiaLoyalty(5).AddMilitaryStaffLoyalty(-5);
    }
}
