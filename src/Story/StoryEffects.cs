using AugustDaysMod.Storage;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Utils;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Story
{
    // Эффекты сюжетных событий, которых нет в игре: Союз, флаги мода, концовки
    public static class StoryEffects
    {
        private const string BAD = "#FF0000";
        private const string GOOD = "#006400";

        public static UnitOfWork AddUnionCohesion(this UnitOfWork effects, float value)
        {
            return effects.AddAction(
                textFormatter => textFormatter.AddText($"Сплоченность Союза: {value:+#;-#;0}", value > 0 ? GOOD : BAD),
                (gameState, gameRules, gameScenario) => UnionDynamics.ChangeCohesion(gameState, value));
        }

        public static UnitOfWork AddUnionRelations(this UnitOfWork effects, GameState gameState, Countries country, float value)
        {
            return effects.AddAction(
                textFormatter => textFormatter.AddText($"Отношения {CountryUtils.GetCountryName(gameState, country)} к союзному центру: {value:+#;-#;0}", value > 0 ? GOOD : BAD),
                (state, gameRules, gameScenario) => UnionRelations.Add(state, country, value));
        }

        public static UnitOfWork AddIndependence(this UnitOfWork effects, GameState gameState, Countries country)
        {
            return effects.AddAction(
                textFormatter => textFormatter.AddText($"{CountryUtils.GetCountryName(gameState, country)} выходит из состава СССР", GOOD),
                (state, gameRules, gameScenario) => UnionDynamics.DeclareIndependence(state, country));
        }

        public static UnitOfWork AddFlag(this UnitOfWork effects, string key, int value = 1)
        {
            return effects.AddAction(
                textFormatter => { },
                (gameState, gameRules, gameScenario) => ModStorage.For(gameState).SetFlag(key, value));
        }

        public static UnitOfWork AddEnding(this UnitOfWork effects, int endingId)
        {
            return effects.AddAction(
                textFormatter => textFormatter.AddText("Игра будет окончена", BAD),
                (gameState, gameRules, gameScenario) => ModStorage.For(gameState).SetFlag(StoryFlags.PendingEnding, endingId));
        }

        public static void ExecutePendingEnding(GameState gameState)
        {
            ModSaveData data = ModStorage.For(gameState);
            int ending = data.Flag(StoryFlags.PendingEnding);
            if (ending != 0)
            {
                data.flags.Remove(StoryFlags.PendingEnding);
                EndingsManager.inst.ExecuteEnding(ending, gameState);
            }
        }
    }

    public static class StoryFlags
    {
        public const string PendingEnding = "pending_ending";
        public const string CoupStance = "ukr_coup_stance";
        public const string Independence = "ukr_independence";
        public const string Referendum = "ukr_referendum";
        public const string Belovezha = "belovezha";
        public const string NuclearKept = "ukr_nuclear_kept";
        public const string NuclearDelay = "ukr_nuclear_delay";
        public const string EarlyElections = "ukr_early_elections";
        public const string OwnCurrency = "ukr_own_currency";
        public const string Fleet = "ukr_fleet";

        public const string RussiaCoupStance = "ru_coup_stance";
        public const string RussiaCommunists = "ru_communists";
        public const string RussiaReform = "ru_reform";
        public const string RussiaChechnya = "ru_chechnya";
        public const string RussiaVouchers = "ru_vouchers";
        public const string RussiaGaidar = "ru_gaidar";
        public const string RussiaReferendum = "ru_referendum";
        public const string RussiaDecree1400 = "ru_decree_1400";
        public const string RussiaChechenWar = "ru_chechen_war";
    }
}
