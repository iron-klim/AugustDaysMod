using System;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.DecisionsSystem;
using Backend.Gamedesign.Utils.ConditionsFormatter;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;

// Решения за Россию: деньги, армия, регионы, отношения с соседями и Западом
namespace AugustDaysMod.Story.Russia
{
    public abstract class RussiaDecision : StoryDecision
    {
        protected override Countries Country => Countries.Russia;

        protected static bool UnionDissolved(GameState gameState) => SovietUnion.Data(gameState).dissolved;

        protected static void RelationsAtLeast(GameState gameState, ConditionsFormatter conditions, Countries country, int value) =>
            conditions.AddCondition($"Отношения с {Backend.Gamedesign.Utils.CountryUtils.GetCountryName(gameState, country)} не ниже {value}",
                gameState.countries[country].RelationshipWithPlayer >= value);
    }

    [Decision(ID)]
    public class RussiaImfLoan : RussiaDecision
    {
        public const int ID = 3101;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 20;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddConditions(GameState gameState, ConditionsFormatter conditions) =>
            RelationsAtLeast(gameState, conditions, Countries.USA, 50);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddIMFLoan(300).AddReserveAction(300).AddLiberalizationLevel(5).AddFirstNeedsGoods(-5).AddRadicalsPower(3);
    }

    [Decision(ID)]
    public class RussiaOilExport : RussiaDecision
    {
        public const int ID = 3102;
        protected override int CooldownMonths => 6;
        protected override int PoliticalPowerCost => 10;
        protected override DecisionType Type => DecisionType.DiplomaticDecision;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddReserveAction(120).AddExport(2).AddCorruptionLevel(3).AddHeavyIndustryEffectiveness(-2);
    }

    [Decision(ID)]
    public class RussiaLoansForShares : RussiaDecision
    {
        public const int ID = 3103;
        protected override int PoliticalPowerCost => 20;

        protected override bool Available(GameState gameState) => gameState.date >= new DateTime(1995, 3, 1);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddReserveAction(250).AddCorruptionLevel(15).AddUnityLevel(-10).AddLiberalizationLevel(5).AddPoliticalPower(30);
    }

    [Decision(ID)]
    public class RussiaCentralBankCredits : RussiaDecision
    {
        public const int ID = 3104;
        protected override int CooldownMonths => 3;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects)
        {
            effects.AddReserveAction(200).AddHeavyIndustryEffectiveness(2).AddIntelligentsiaLoyalty(-5).AddAction(
                textFormatter => textFormatter.AddText("Цены: +10%", "#FF0000"),
                (state, gameRules, gameScenario) => state.PriceIndex *= 1.1f);
        }
    }

    [Decision(ID)]
    public class RussiaArmedForces : RussiaDecision
    {
        public const int ID = 3105;
        protected override int PoliticalPowerCost => 25;
        protected override DecisionType Type => DecisionType.ArmyDecision;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddArmyQuantityLevel(10).AddWarheadsQuantity(20).AddMilitaryStaffLoyalty(15).AddCombatability(5).AddReserveAction(-100);
    }

    [Decision(ID)]
    public class RussiaConversion : RussiaDecision
    {
        public const int ID = 3106;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 15;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddLightIndustryEffectiveness(5).AddArmyIndustryEffectiveness(-5).AddReserveAction(60).AddMilitaryStaffLoyalty(-5);
    }

    [Decision(ID)]
    public class RussiaDemocraticRussia : RussiaDecision
    {
        public const int ID = 3107;
        protected override int CooldownMonths => 6;
        protected override DecisionType Type => DecisionType.PoliticalDecision;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddPoliticalPower(15).AddIntelligentsiaLoyalty(10).AddRadicalsPower(3).AddUnityLevel(-2);
    }

    [Decision(ID)]
    public class RussiaBelarusUnion : RussiaDecision
    {
        public const int ID = 3108;
        protected override int PoliticalPowerCost => 25;
        protected override DecisionType Type => DecisionType.DiplomaticDecision;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState) && gameState.date >= new DateTime(1995, 1, 1);

        protected override void AddConditions(GameState gameState, ConditionsFormatter conditions) =>
            RelationsAtLeast(gameState, conditions, Countries.Belorussia, 60);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddCountryRelations(gameState, 20, Countries.Belorussia).AddUnityLevel(5).AddRadicalsPower(-5).AddReserveAction(-80);
    }

    [Decision(ID)]
    public class RussiaFarmers : RussiaDecision
    {
        public const int ID = 3109;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 10;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddAgroEffectiveness(5).AddFirstNeedsGoods(5).AddReserveAction(-100);
    }

    [Decision(ID)]
    public class RussiaTaxPolice : RussiaDecision
    {
        public const int ID = 3110;
        protected override int CooldownMonths => 12;
        protected override int PoliticalPowerCost => 15;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddReserveAction(100).AddCorruptionLevel(-5).AddOrderLevel(3).AddIntelligentsiaLoyalty(-3);
    }

    [Decision(ID)]
    public class RussiaAddressToNation : RussiaDecision
    {
        public const int ID = 3111;
        protected override int CooldownMonths => 3;
        protected override DecisionType Type => DecisionType.PoliticalDecision;

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddPoliticalPower(25).AddUnityLevel(2).AddIntelligentsiaLoyalty(-3);
    }

    [Decision(ID)]
    public class RussiaCivicUnion : RussiaDecision
    {
        public const int ID = 3112;
        protected override int CooldownMonths => 12;
        protected override DecisionType Type => DecisionType.PoliticalDecision;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddPoliticalPower(40).AddUnityLevel(5).AddRadicalsPower(-5).AddLiberalizationLevel(-3).AddIntelligentsiaLoyalty(-5);
    }

    [Decision(ID)]
    public class RussiaTroopsWithdrawal : RussiaDecision
    {
        public const int ID = 3113;
        protected override int PoliticalPowerCost => 20;
        protected override DecisionType Type => DecisionType.DiplomaticDecision;

        protected override bool Available(GameState gameState) => UnionDissolved(gameState);

        protected override void AddEffects(GameState gameState, UnitOfWork effects) =>
            effects.AddArmyQuantityLevel(-5).AddReserveAction(150).AddMilitaryStaffLoyalty(-10)
                .AddCountryRelations(gameState, 20, Countries.UnitedGermany).AddCountryRelations(gameState, 15, Countries.Litva)
                .AddCountryRelations(gameState, 15, Countries.Latvia).AddCountryRelations(gameState, 15, Countries.Estonia)
                .AddCountryRelations(gameState, 10, Countries.USA);
    }
}
