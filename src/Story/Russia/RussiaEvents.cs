using AugustDaysMod.Assets;
using System;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.Utils.TextFormatter;
using Backend.Gamedesign.Utils.UnitsOfWork;

// Сюжет за Россию: от путча в Москве до президентских выборов 1996 года
namespace AugustDaysMod.Story.Russia
{
    public abstract class RussiaEvent : StoryEvent
    {
        protected RussiaEvent(DateTime startDate, DateTime? endDate = null) : base(Countries.Russia, startDate, endDate)
        {
        }

        // Назначение по сюжету, если политик еще в игре и свободен от другого поста
        protected static UnitOfWork Appoint(UnitOfWork effects, GameState gameState, Characters character, PoliticPosition position)
        {
            if (gameState.characters.TryGetValue(character, out CharacterInfo info) && info.status == CharacterInfo.PoliticStatus.Alive &&
                character != gameState.gameCharacter)
            {
                effects.AddCharacterTakesPost(gameState, character, position, true);
            }

            return effects;
        }

        protected static UnitOfWork MultiplyPrices(UnitOfWork effects, float factor)
        {
            return effects.AddAction(
                textFormatter => textFormatter.AddText(ModLanguage.Pick($"Цены: +{(factor - 1f) * 100f:0}%", $"Prices: +{(factor - 1f) * 100f:0}%"), "#FF0000"),
                (state, gameRules, gameScenario) => state.PriceIndex *= factor);
        }
    }

    // 19.08.1991. ГКЧП объявляет чрезвычайное положение, к Белому дому идут танки
    [Event(ID)]
    public class RussiaCoup : RussiaEvent
    {
        public const int ID = 2101;

        public RussiaCoup() : base(new DateTime(1991, 8, 19), new DateTime(1991, 8, 21))
        {
        }

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Yanayev");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddPoliticalPower(20).AddIntelligentsiaLoyalty(20).AddUnityLevel(5).AddFreedomLevel(5)
                        .AddMilitaryStaffLoyalty(-5).AddCountryRelations(gameState, -20, Countries.USSR)
                        .AddCountryRelations(gameState, 15, Countries.USA).AddUnionCohesion(-5).AddFlag(StoryFlags.RussiaCoupStance, 0);
                    break;
                case 1:
                    effects.AddPoliticalPower(-15).AddIntelligentsiaLoyalty(-20).AddMilitaryStaffLoyalty(10)
                        .AddCountryRelations(gameState, 10, Countries.USSR).AddCountryRelations(gameState, -10, Countries.USA)
                        .AddFlag(StoryFlags.RussiaCoupStance, 1);
                    break;
                case 2:
                    effects.AddPoliticalPower(-5).AddOrderLevel(-5).AddIntelligentsiaLoyalty(5).AddCountryRelations(gameState, -10, Countries.USSR)
                        .AddFlag(StoryFlags.RussiaCoupStance, 2);
                    break;
            }
        }
    }

    // 23.08.1991. Путч провалился. Горбачев на сессии Верховного Совета РСФСР
    [Event(ID)]
    public class RussiaVictory : RussiaEvent
    {
        public const int ID = 2102;

        public RussiaVictory() : base(new DateTime(1991, 8, 23))
        {
        }

        protected override bool Condition(GameState gameState) => Done(gameState, RussiaCoup.ID);

        protected override string Variant(GameState gameState) => Flag(gameState, StoryFlags.RussiaCoupStance) == 0 ? null : "late";

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Khasbulatov");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddPoliticalPower(15).AddIntelligentsiaLoyalty(10).AddRadicalsPower(10).AddFreedomLevel(-3)
                        .AddCountryRelations(gameState, -20, Countries.USSR).AddUnionCohesion(-15).AddReserveAction(100)
                        .AddFlag(StoryFlags.RussiaCommunists, 0);
                    break;
                case 1:
                    effects.AddPoliticalPower(5).AddIntelligentsiaLoyalty(5).AddRadicalsPower(5)
                        .AddCountryRelations(gameState, -10, Countries.USSR).AddUnionCohesion(-10).AddFlag(StoryFlags.RussiaCommunists, 1);
                    break;
                case 2:
                    effects.AddPoliticalPower(-10).AddIntelligentsiaLoyalty(-10).AddUnityLevel(5).AddFreedomLevel(5)
                        .AddCountryRelations(gameState, 10, Countries.USSR).AddFlag(StoryFlags.RussiaCommunists, 2);
                    break;
            }
        }
    }

    // 28.10.1991. V Съезд народных депутатов РСФСР: программа экономической реформы
    [Event(ID)]
    public class RussiaReform : RussiaEvent
    {
        public const int ID = 2103;

        public RussiaReform() : base(new DateTime(1991, 10, 28))
        {
        }

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "@burbulis");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddPoliticalPower(20).AddLiberalizationLevel(10).AddIntelligentsiaLoyalty(5).AddRadicalsPower(5)
                        .AddCountryRelations(gameState, 10, Countries.USA).AddFlag(StoryFlags.RussiaReform, 1);
                    Appoint(effects, gameState, Characters.Gaidar, PoliticPosition.MinisterOfFinance);
                    break;
                case 1:
                    effects.AddLiberalizationLevel(5).AddUnityLevel(3).AddFlag(StoryFlags.RussiaReform, 2);
                    Appoint(effects, gameState, Characters.Yavlinskiy, PoliticPosition.MinisterOfFinance);
                    break;
                case 2:
                    effects.AddPoliticalPower(-10).AddFirstNeedsGoods(-10).AddIntelligentsiaLoyalty(-10).AddUnityLevel(5)
                        .AddFlag(StoryFlags.RussiaReform, 0);
                    break;
            }
        }
    }

    // 2.11.1991. Дудаев провозглашает независимость Чечни
    [Event(ID)]
    public class RussiaChechnya : RussiaEvent
    {
        public const int ID = 2104;

        public RussiaChechnya() : base(new DateTime(1991, 11, 2))
        {
        }

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Rutskoy");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddOrderLevel(-5).AddUnityLevel(-5).AddMilitaryStaffLoyalty(-5).AddFlag(StoryFlags.RussiaChechnya, 0);
                    break;
                case 1:
                    effects.AddPoliticalPower(-20).AddMilitaryStaffLoyalty(-10).AddIntelligentsiaLoyalty(-10).AddRadicalsPower(10)
                        .AddOrderLevel(-5).AddFlag(StoryFlags.RussiaChechnya, 1);
                    break;
                case 2:
                    effects.AddReserveAction(-50).AddOrderLevel(-3).AddFlag(StoryFlags.RussiaChechnya, 2);
                    break;
            }
        }
    }

    // 8.12.1991. Вискули
    [Event(ID)]
    public class RussiaBelovezha : RussiaEvent
    {
        public const int ID = 2105;

        public RussiaBelovezha() : base(new DateTime(1991, 12, 8))
        {
        }

        protected override bool Condition(GameState gameState) => !UnionDissolved(gameState);

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "@burbulis");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddPoliticalPower(15).AddCountryRelations(gameState, 15, Countries.Ukraine)
                    .AddCountryRelations(gameState, 15, Countries.Belorussia).AddCountryRelations(gameState, 10, Countries.USA)
                    .AddCountryRelations(gameState, -30, Countries.USSR).AddUnityLevel(-5).AddFlag(StoryFlags.Belovezha, 1);
            }
            else
            {
                effects.AddPoliticalPower(5).AddRadicalsPower(10).AddUnityLevel(-10).AddCountryRelations(gameState, -15, Countries.Ukraine)
                    .AddCountryRelations(gameState, -10, Countries.Kazakhstan).AddCountryRelations(gameState, -40, Countries.USSR)
                    .AddFlag(StoryFlags.Belovezha, 2);
            }
        }
    }

    // 2.01.1992. Либерализация цен
    [Event(ID)]
    public class RussiaPrices : RussiaEvent
    {
        public const int ID = 2106;

        public RussiaPrices() : base(new DateTime(1992, 1, 2))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override string Variant(GameState gameState) => Flag(gameState, StoryFlags.RussiaReform) == 1 ? null : "late";

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Gaidar");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                MultiplyPrices(effects, 1.3f).AddFirstNeedsGoods(20).AddReserveAction(200).AddLiberalizationLevel(10)
                    .AddUnityLevel(-10).AddIntelligentsiaLoyalty(-10).AddRadicalsPower(10).AddCountryRelations(gameState, 10, Countries.USA);
            }
            else
            {
                effects.AddFirstNeedsGoods(-15).AddReserveAction(-150).AddPoliticalPower(-15).AddUnityLevel(-5);
            }
        }
    }

    // 31.03.1992. Федеративный договор. Татарстан на референдуме проголосовал за суверенитет
    [Event(ID)]
    public class RussiaFederativeTreaty : RussiaEvent
    {
        public const int ID = 2107;

        public RussiaFederativeTreaty() : base(new DateTime(1992, 3, 31))
        {
        }

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "@shakhrai");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddUnityLevel(10).AddPoliticalPower(10).AddReserveAction(-60).AddOrderLevel(3);
            }
            else
            {
                effects.AddOrderLevel(5).AddUnityLevel(-10).AddRadicalsPower(10).AddMilitaryStaffLoyalty(5);
            }
        }
    }

    // Июнь 1992. Бои в Бендерах, 14-я армия генерала Лебедя
    [Event(ID)]
    public class RussiaTransnistria : RussiaEvent
    {
        public const int ID = 2108;

        public RussiaTransnistria() : base(new DateTime(1992, 6, 23))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Lebed");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddMilitaryStaffLoyalty(10).AddRadicalsPower(5).AddCountryRelations(gameState, -25, Countries.Moldavia)
                        .AddCountryRelations(gameState, -10, Countries.Romania).AddCountryRelations(gameState, -5, Countries.USA);
                    break;
                case 1:
                    effects.AddPoliticalPower(5).AddMilitaryStaffLoyalty(5).AddReserveAction(-50)
                        .AddCountryRelations(gameState, -5, Countries.Moldavia);
                    break;
                case 2:
                    effects.AddMilitaryStaffLoyalty(-15).AddRadicalsPower(10).AddPoliticalPower(-10)
                        .AddCountryRelations(gameState, 15, Countries.Moldavia).AddCountryRelations(gameState, 10, Countries.USA);
                    break;
            }
        }
    }

    // 1.10.1992. Приватизационные чеки
    [Event(ID)]
    public class RussiaVouchers : RussiaEvent
    {
        public const int ID = 2109;

        public RussiaVouchers() : base(new DateTime(1992, 10, 1))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Chubais");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddLiberalizationLevel(10).AddCorruptionLevel(10).AddReserveAction(50).AddIntelligentsiaLoyalty(-5)
                        .AddLightIndustryEffectiveness(3).AddFlag(StoryFlags.RussiaVouchers, 1);
                    break;
                case 1:
                    effects.AddReserveAction(200).AddCorruptionLevel(15).AddUnityLevel(-10).AddLiberalizationLevel(5)
                        .AddFlag(StoryFlags.RussiaVouchers, 2);
                    break;
                case 2:
                    effects.AddPoliticalPower(-10).AddLiberalizationLevel(-5).AddCountryRelations(gameState, -10, Countries.USA)
                        .AddFlag(StoryFlags.RussiaVouchers, 0);
                    break;
            }
        }
    }

    // Декабрь 1992. VII Съезд народных депутатов не утверждает Гайдара
    [Event(ID)]
    public class RussiaSeventhCongress : RussiaEvent
    {
        public const int ID = 2110;

        public RussiaSeventhCongress() : base(new DateTime(1992, 12, 10))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Khasbulatov");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddPoliticalPower(-25).AddIntelligentsiaLoyalty(10).AddRadicalsPower(10).AddFlag(StoryFlags.RussiaGaidar, 1);
            }
            else
            {
                effects.AddPoliticalPower(15).AddLiberalizationLevel(-5).AddHeavyIndustryEffectiveness(5).AddIntelligentsiaLoyalty(-5)
                    .AddRadicalsPower(-5).AddFlag(StoryFlags.RussiaGaidar, 0);
                Appoint(effects, gameState, Characters.Chernomyrdin, PoliticPosition.ChairmanOfTheCouncilOfMinisters);
            }
        }
    }

    // 25.04.1993. Референдум о доверии: «Да — да — нет — да»
    [Event(ID)]
    public class RussiaReferendum : RussiaEvent
    {
        public const int ID = 2111;

        public RussiaReferendum() : base(new DateTime(1993, 4, 25))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        private static float Trust(GameState gameState) =>
            Math.Max(35f, Math.Min(70f, 30f + gameState.PoliticalPower * 0.2f + gameState.IntelligentsiaLoyalty * 0.2f + gameState.FirstNeedsGoods * 0.15f));

        public static bool Wins(GameState gameState) => Trust(gameState) >= 50f;

        protected override string Variant(GameState gameState) => Wins(gameState) ? null : "lose";

        protected override object[] DescriptionArgs(GameState gameState) => new object[] { Trust(gameState).ToString("0.0") };

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (Wins(gameState))
            {
                effects.AddPoliticalPower(25).AddUnityLevel(5).AddRadicalsPower(-5).AddFlag(StoryFlags.RussiaReferendum, 1);
            }
            else
            {
                effects.AddPoliticalPower(-25).AddRadicalsPower(10).AddUnityLevel(-5).AddFlag(StoryFlags.RussiaReferendum, 0);
            }
        }
    }

    // 21.09.1993. Указ № 1400 о поэтапной конституционной реформе
    [Event(ID)]
    public class RussiaDecree1400 : RussiaEvent
    {
        public const int ID = 2112;

        public RussiaDecree1400() : base(new DateTime(1993, 9, 21))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "@filatov");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddPoliticalPower(10).AddFreedomLevel(-5).AddRadicalsPower(15).AddCountryRelations(gameState, 5, Countries.USA)
                        .AddFlag(StoryFlags.RussiaDecree1400, 1);
                    break;
                case 1:
                    effects.AddPoliticalPower(-20).AddFreedomLevel(5).AddUnityLevel(5).AddFlag(StoryFlags.RussiaDecree1400, 2);
                    break;
                case 2:
                    effects.AddEnding(RussiaEndings.Impeached.ID);
                    break;
            }
        }
    }

    // 3–4.10.1993. Мэрия, Останкино, танки у Белого дома
    [Event(ID)]
    public class RussiaOctober : RussiaEvent
    {
        public const int ID = 2113;

        public RussiaOctober() : base(new DateTime(1993, 10, 3))
        {
        }

        protected override bool Condition(GameState gameState) => Flag(gameState, StoryFlags.RussiaDecree1400) == 1;

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Grachev");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddOrderLevel(15).AddRadicalsPower(-20).AddPoliticalPower(20).AddFreedomLevel(-10).AddIntelligentsiaLoyalty(-10)
                    .AddMilitaryStaffLoyalty(5).AddUnityLevel(-5);
            }
            else
            {
                effects.AddPoliticalPower(-30).AddRadicalsPower(10).AddOrderLevel(-10).AddFreedomLevel(5).AddUnityLevel(5)
                    .AddFlag(StoryFlags.RussiaDecree1400, 2);
            }
        }
    }

    // 12.12.1993. Референдум о Конституции и выборы в Государственную Думу
    [Event(ID)]
    public class RussiaConstitution : RussiaEvent
    {
        public const int ID = 2114;

        public RussiaConstitution() : base(new DateTime(1993, 12, 12))
        {
        }

        protected override bool Condition(GameState gameState) => HasFlag(gameState, StoryFlags.RussiaDecree1400);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddPoliticalPower(25).AddOrderLevel(5).AddFreedomLevel(-5).AddRadicalsPower(5);
            }
            else
            {
                effects.AddPoliticalPower(-10).AddFreedomLevel(10).AddIntelligentsiaLoyalty(10).AddUnityLevel(5);
            }
        }
    }

    // 11.12.1994. Ввод войск в Чечню
    [Event(ID)]
    public class RussiaChechenWar : RussiaEvent
    {
        public const int ID = 2115;

        public RussiaChechenWar() : base(new DateTime(1994, 12, 11))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "Grachev");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddReserveAction(-150).AddCombatability(-10).AddMilitaryStaffLoyalty(-10).AddIntelligentsiaLoyalty(-15)
                        .AddUnityLevel(-10).AddOrderLevel(5).AddCountryRelations(gameState, -10, Countries.USA)
                        .AddFlag(StoryFlags.RussiaChechenWar, 1);
                    break;
                case 1:
                    effects.AddPoliticalPower(-15).AddOrderLevel(-5).AddRadicalsPower(5).AddFlag(StoryFlags.RussiaChechenWar, 0);
                    break;
                case 2:
                    effects.AddReserveAction(-50).AddOrderLevel(-3).AddMilitaryStaffLoyalty(-5).AddFlag(StoryFlags.RussiaChechenWar, 0);
                    break;
            }
        }
    }

    // 16.06.1996. Президентские выборы
    [Event(ID)]
    public class RussiaElections : RussiaEvent
    {
        public const int ID = 2116;

        public RussiaElections() : base(new DateTime(1996, 6, 16))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        public static bool Wins(GameState gameState)
        {
            float score = gameState.PoliticalPower + gameState.UnityLevel + gameState.FirstNeedsGoods / 2f + gameState.IntelligentsiaLoyalty / 2f;
            if (Flag(gameState, StoryFlags.RussiaChechenWar) == 1)
            {
                score -= 15f;
            }

            return score >= 110f;
        }

        protected override string Variant(GameState gameState) => Wins(gameState) ? "win" : "lose";

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            effects.AddEnding(Wins(gameState) ? RussiaEndings.Reelected.ID : RussiaEndings.CommunistRevenge.ID);
        }
    }
}
