using System;
using AugustDaysMod.Storage;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.Utils.UnitsOfWork;

// Сюжет за Украину: от путча в Москве до досрочных выборов 1994 года
namespace AugustDaysMod.Story.Ukraine
{
    public abstract class UkraineEvent : StoryEvent
    {
        protected UkraineEvent(DateTime startDate, DateTime? endDate = null) : base(Countries.Ukraine, startDate, endDate)
        {
        }

        protected static int Flag(GameState gameState, string key) => ModStorage.For(gameState).Flag(key);

        protected static bool HasFlag(GameState gameState, string key) => ModStorage.For(gameState).flags.ContainsKey(key);

        protected static bool Done(GameState gameState, int id) => gameState.eventsDone.ContainsKey(id);

        protected static bool UnionDissolved(GameState gameState) => SovietUnion.Data(gameState).dissolved;
    }

    // 19.08.1991. В Москве ГКЧП, в Киев прилетает генерал Варенников
    [Event(ID)]
    public class UkraineCoup : UkraineEvent
    {
        public const int ID = 2001;

        public UkraineCoup() : base(new DateTime(1991, 8, 19), new DateTime(1991, 8, 21))
        {
        }

        protected override Characters Speaker(GameState gameState) => Characters.Varennikov;

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddPoliticalPower(10).AddIntelligentsiaLoyalty(-10).AddRadicalsPower(5)
                        .AddCountryRelations(gameState, 5, Countries.USSR).AddFlag(StoryFlags.CoupStance, 0);
                    break;
                case 1:
                    effects.AddIntelligentsiaLoyalty(15).AddUnityLevel(5).AddFreedomLevel(3).AddMilitaryStaffLoyalty(-5)
                        .AddCountryRelations(gameState, -15, Countries.USSR).AddCountryRelations(gameState, 10, Countries.USA)
                        .AddCountryRelations(gameState, 10, Countries.Russia).AddFlag(StoryFlags.CoupStance, 1);
                    break;
                case 2:
                    effects.AddMilitaryStaffLoyalty(10).AddSpecialServicesLoyalty(10).AddIntelligentsiaLoyalty(-25).AddUnityLevel(-10)
                        .AddRadicalsPower(15).AddCountryRelations(gameState, 15, Countries.USSR)
                        .AddCountryRelations(gameState, -20, Countries.USA).AddCountryRelations(gameState, -20, Countries.Russia)
                        .AddUnionCohesion(5).AddFlag(StoryFlags.CoupStance, 2);
                    break;
            }
        }
    }

    // 24.08.1991. Внеочередная сессия Верховного Совета
    [Event(ID)]
    public class UkraineIndependenceAct : UkraineEvent
    {
        public const int ID = 2002;

        public UkraineIndependenceAct() : base(new DateTime(1991, 8, 24))
        {
        }

        protected override Characters Speaker(GameState gameState) => CharacterOrRuler(gameState, "@plyushch");

        protected override bool Condition(GameState gameState) => Done(gameState, UkraineCoup.ID);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddPoliticalPower(15).AddUnityLevel(5).AddIntelligentsiaLoyalty(15).AddMilitaryStaffLoyalty(-5)
                    .AddCountryRelations(gameState, -20, Countries.USSR).AddCountryRelations(gameState, -5, Countries.Russia)
                    .AddUnionCohesion(-10).AddFlag(StoryFlags.Independence, 1);
            }
            else
            {
                effects.AddPoliticalPower(-15).AddIntelligentsiaLoyalty(-15).AddRadicalsPower(10)
                    .AddCountryRelations(gameState, 15, Countries.USSR).AddUnionCohesion(5).AddFlag(StoryFlags.Independence, 0);
            }
        }

        public static Characters CharacterOrRuler(GameState gameState, string reference)
        {
            Characters? character = Catalog.CountryLeadership.Resolve(reference);
            return character.HasValue && gameState.characters.ContainsKey(character.Value) ? character.Value : gameState.gameCharacter;
        }
    }

    // Сентябрь 1991. Войска Киевского, Одесского и Прикарпатского округов на территории республики
    [Event(ID)]
    public class UkraineArmyOath : UkraineEvent
    {
        public const int ID = 2003;

        public UkraineArmyOath() : base(new DateTime(1991, 9, 3))
        {
        }

        protected override Characters Speaker(GameState gameState) => UkraineIndependenceAct.CharacterOrRuler(gameState, "@morozov");

        protected override bool Condition(GameState gameState) => Flag(gameState, StoryFlags.Independence) == 1;

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddArmyQuantityLevel(10).AddMilitaryStaffLoyalty(-10).AddCombatability(-5)
                    .AddCountryRelations(gameState, -15, Countries.USSR).AddCountryRelations(gameState, -10, Countries.Russia);
            }
            else
            {
                effects.AddArmyQuantityLevel(3).AddMilitaryStaffLoyalty(5).AddCountryRelations(gameState, -5, Countries.USSR);
            }
        }
    }

    // 1 декабря 1991. Всеукраинский референдум и выборы президента
    [Event(ID)]
    public class UkraineReferendum : UkraineEvent
    {
        public const int ID = 2004;

        public UkraineReferendum() : base(new DateTime(1991, 12, 1))
        {
        }

        protected override bool Condition(GameState gameState) => Done(gameState, UkraineIndependenceAct.ID);

        protected override string Variant(GameState gameState) => Flag(gameState, StoryFlags.Independence) == 1 ? null : "union";

        protected override object[] DescriptionArgs(GameState gameState)
        {
            float yes = Math.Min(95f, Math.Max(51f, 60f + (gameState.UnityLevel - 50f) * 0.4f + (gameState.IntelligentsiaLoyalty - 50f) * 0.3f + 15f));
            return new object[] { yes.ToString("0.0") };
        }

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (Flag(gameState, StoryFlags.Independence) == 1)
            {
                effects.AddPoliticalPower(25).AddUnityLevel(10).AddFreedomLevel(5).AddLiberalizationLevel(5)
                    .AddCountryRelations(gameState, -10, Countries.Russia).AddCountryRelations(gameState, 10, Countries.USA)
                    .AddCountryRelations(gameState, 15, Countries.Canada).AddCountryRelations(gameState, 15, Countries.Poland)
                    .AddUnionCohesion(-15).AddFlag(StoryFlags.Referendum, 1)
                    .AddIndependence(gameState, gameState.playerCountry);
            }
            else
            {
                effects.AddPoliticalPower(-10).AddUnityLevel(-5).AddIntelligentsiaLoyalty(-10)
                    .AddUnionCohesion(-10).AddFlag(StoryFlags.Referendum, 1).AddFlag(StoryFlags.Independence, 1)
                    .AddIndependence(gameState, gameState.playerCountry);
            }
        }
    }

    // 7–8 декабря 1991. Вискули
    [Event(ID)]
    public class UkraineBelovezha : UkraineEvent
    {
        public const int ID = 2005;

        public UkraineBelovezha() : base(new DateTime(1991, 12, 7))
        {
        }

        protected override bool Condition(GameState gameState) => Flag(gameState, StoryFlags.Referendum) == 1 && !UnionDissolved(gameState);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddPoliticalPower(10).AddCountryRelations(gameState, 10, Countries.Russia)
                    .AddCountryRelations(gameState, 10, Countries.Belorussia).AddCountryRelations(gameState, 5, Countries.USA)
                    .AddCountryRelations(gameState, -30, Countries.USSR).AddFlag(StoryFlags.Belovezha, 1);
            }
            else
            {
                effects.AddIntelligentsiaLoyalty(10).AddRadicalsPower(5).AddCountryRelations(gameState, -15, Countries.Russia)
                    .AddCountryRelations(gameState, -5, Countries.Belorussia).AddCountryRelations(gameState, -20, Countries.USSR)
                    .AddFlag(StoryFlags.Belovezha, 2);
            }
        }
    }

    // Январь 1992. Цены отпущены в России, рубли уходят из Украины
    [Event(ID)]
    public class UkraineCoupons : UkraineEvent
    {
        public const int ID = 2006;

        public UkraineCoupons() : base(new DateTime(1992, 1, 10))
        {
        }

        protected override Characters Speaker(GameState gameState) => UkraineIndependenceAct.CharacterOrRuler(gameState, "@pyatachenko");

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddReserveAction(80).AddLiberalizationLevel(5).AddFirstNeedsGoods(-10).AddCorruptionLevel(5)
                    .AddCountryRelations(gameState, -10, Countries.Russia).AddFlag(StoryFlags.OwnCurrency, 1);
            }
            else
            {
                effects.AddFirstNeedsGoods(5).AddIntelligentsiaLoyalty(-10).AddUnityLevel(-5).AddPoliticalPower(-10)
                    .AddCountryRelations(gameState, 10, Countries.Russia).AddFlag(StoryFlags.OwnCurrency, 0);
            }
        }
    }

    // Весна 1992. Черноморский флот
    [Event(ID)]
    public class UkraineFleet : UkraineEvent
    {
        public const int ID = 2007;

        public UkraineFleet() : base(new DateTime(1992, 4, 5))
        {
        }

        protected override Characters Speaker(GameState gameState) => UkraineIndependenceAct.CharacterOrRuler(gameState, "@morozov");

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddArmyQuantityLevel(3).AddReserveAction(-30).AddCountryRelations(gameState, 5, Countries.Russia)
                        .AddFlag(StoryFlags.Fleet, 0);
                    break;
                case 1:
                    effects.AddArmyQuantityLevel(8).AddMilitaryStaffLoyalty(5).AddRadicalsPower(5)
                        .AddCountryRelations(gameState, -25, Countries.Russia).AddFlag(StoryFlags.Fleet, 1);
                    break;
                case 2:
                    effects.AddReserveAction(60).AddIntelligentsiaLoyalty(-10).AddUnityLevel(-5)
                        .AddCountryRelations(gameState, 15, Countries.Russia).AddFlag(StoryFlags.Fleet, 2);
                    break;
            }
        }
    }

    // Май 1992. Верховный Совет Крыма провозглашает Акт о государственной самостоятельности
    [Event(ID)]
    public class UkraineCrimea : UkraineEvent
    {
        public const int ID = 2008;

        public UkraineCrimea() : base(new DateTime(1992, 5, 5))
        {
        }

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddUnityLevel(5).AddRadicalsPower(5).AddCountryRelations(gameState, -5, Countries.Russia);
                    break;
                case 1:
                    effects.AddUnityLevel(-5).AddIntelligentsiaLoyalty(-10).AddCountryRelations(gameState, 10, Countries.Russia);
                    break;
                case 2:
                    effects.AddOrderLevel(5).AddUnityLevel(-10).AddRadicalsPower(10).AddPoliticalPower(-10)
                        .AddCountryRelations(gameState, -25, Countries.Russia).AddCountryRelations(gameState, -10, Countries.USA);
                    break;
            }
        }
    }

    // 23 мая 1992. Лиссабонский протокол
    [Event(ID)]
    public class UkraineNuclear : UkraineEvent
    {
        public const int ID = 2009;

        public UkraineNuclear() : base(new DateTime(1992, 5, 23))
        {
        }

        protected override Characters Speaker(GameState gameState) => UkraineIndependenceAct.CharacterOrRuler(gameState, "@zlenko");

        protected override bool Condition(GameState gameState) => UnionDissolved(gameState);

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            switch (optionIndex)
            {
                case 0:
                    effects.AddWarheadsQuantity(-25).AddCountryRelations(gameState, 20, Countries.USA)
                        .AddCountryRelations(gameState, 10, Countries.Russia).AddUSLoan(100).AddReserveAction(100);
                    break;
                case 1:
                    effects.AddMilitaryStaffLoyalty(10).AddRadicalsPower(5).AddCountryRelations(gameState, -25, Countries.USA)
                        .AddCountryRelations(gameState, -20, Countries.Russia).AddFlag(StoryFlags.NuclearKept, 1);
                    break;
                case 2:
                    effects.AddCountryRelations(gameState, -5, Countries.USA).AddFlag(StoryFlags.NuclearDelay, 1);
                    break;
            }
        }
    }

    // 14 января 1994. Трехстороннее заявление президентов США, России и Украины
    [Event(ID)]
    public class UkraineTrilateral : UkraineEvent
    {
        public const int ID = 2010;

        public UkraineTrilateral() : base(new DateTime(1994, 1, 14))
        {
        }

        protected override Characters Speaker(GameState gameState) => UkraineIndependenceAct.CharacterOrRuler(gameState, "@zlenko");

        protected override bool Condition(GameState gameState) => Flag(gameState, StoryFlags.NuclearDelay) == 1;

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddWarheadsQuantity(-25).AddCountryRelations(gameState, 20, Countries.USA)
                    .AddCountryRelations(gameState, 10, Countries.Russia).AddUSLoan(150).AddReserveAction(150);
            }
            else
            {
                effects.AddMilitaryStaffLoyalty(5).AddCountryRelations(gameState, -25, Countries.USA)
                    .AddCountryRelations(gameState, -15, Countries.Russia).AddFlag(StoryFlags.NuclearKept, 1);
            }
        }
    }

    // Июнь 1993. Забастовка шахтеров Донбасса
    [Event(ID)]
    public class UkraineMinersStrike : UkraineEvent
    {
        public const int ID = 2011;

        public UkraineMinersStrike() : base(new DateTime(1993, 6, 7))
        {
        }

        protected override Characters Speaker(GameState gameState) => UkraineIndependenceAct.CharacterOrRuler(gameState, "@kuchma");

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            if (optionIndex == 0)
            {
                effects.AddReserveAction(-80).AddUnityLevel(5).AddPoliticalPower(-20).AddFlag(StoryFlags.EarlyElections, 1);
            }
            else
            {
                effects.AddOrderLevel(5).AddUnityLevel(-10).AddRadicalsPower(10).AddHeavyIndustryEffectiveness(-5)
                    .AddIntelligentsiaLoyalty(-5);
            }
        }
    }

    // Лето 1994. Президентские выборы: досрочные после забастовки или очередные
    [Event(ID)]
    public class UkraineElections : UkraineEvent
    {
        public const int ID = 2012;

        public UkraineElections() : base(new DateTime(1994, 6, 26))
        {
        }

        protected override bool Condition(GameState gameState) =>
            Flag(gameState, StoryFlags.EarlyElections) == 1 || gameState.date >= new DateTime(1996, 12, 1);

        public static bool Wins(GameState gameState) =>
            gameState.PoliticalPower + gameState.UnityLevel + gameState.FirstNeedsGoods / 2f + gameState.IntelligentsiaLoyalty / 2f >= 110f;

        protected override string Variant(GameState gameState) => Wins(gameState) ? "win" : "lose";

        protected override void SetupOption(GameState gameState, int optionIndex, UnitOfWork effects)
        {
            effects.AddEnding(Wins(gameState) ? UkraineEndings.Independent.ID : UkraineEndings.Defeated.ID);
        }
    }
}
