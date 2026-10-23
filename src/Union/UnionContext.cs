using System;
using System.Collections.Generic;
using AugustDaysMod.Storage;
using Backend.Gamedesign;
using HarmonyLib;

namespace AugustDaysMod.Union
{
    // Союз без игрока. Когда игрок управляет другой страной, СССР остается отдельной страной под управлением ИИ
    public static class SovietUnion
    {
        public static bool IsSeparateFromPlayer(GameState gameState) =>
            gameState.playerCountry != Countries.USSR && gameState.countries.TryGetValue(Countries.USSR, out Country ussr) && ussr.activeInGame;

        public static bool IsSubject(Country country) =>
            country.activeInGame && country.puppetOf == Countries.USSR;

        public static IEnumerable<KeyValuePair<Countries, Country>> Subjects(GameState gameState)
        {
            foreach (KeyValuePair<Countries, Country> pair in gameState.countries)
            {
                if (IsSubject(pair.Value))
                {
                    yield return pair;
                }
            }
        }

        public static UnionData Data(GameState gameState) => ModStorage.For(gameState).union;
    }

    // У страны в игре одно поле отношений — к игроку. Отношения республик к союзному центру хранит мод
    public static class UnionRelations
    {
        public const float DEFAULT = 50f;

        public static float Get(GameState gameState, Countries country) =>
            ModStorage.For(gameState).unionRelations.TryGetValue(country, out float value) ? value : DEFAULT;

        public static void Set(GameState gameState, Countries country, float value) =>
            ModStorage.For(gameState).unionRelations[country] = Math.Max(0f, Math.Min(100f, value));

        public static void Add(GameState gameState, Countries country, float delta) =>
            Set(gameState, country, Get(gameState, country) + delta);

        // Пока игрок управлял Союзом, отношения республик к нему и были отношениями к центру
        public static void CaptureFromPlayer(GameState gameState)
        {
            foreach (KeyValuePair<Countries, Country> subject in SovietUnion.Subjects(gameState))
            {
                Set(gameState, subject.Key, subject.Value.RelationshipWithPlayer);
            }
        }
    }

    // Временно делает СССР страной игрока, чтобы прогнать штатный ИИ республик от его лица.
    // Отношения республик к игроку на это время подменяются отношениями к центру. Страну игрока, если она еще
    // в Союзе, на это время из него убираем: ее лидером и правительством распоряжается игрок, а не ИИ центра
    public sealed class UnionContext : IDisposable
    {
        private static readonly AccessTools.FieldRef<Country, float> RelationshipWithPlayer =
            AccessTools.FieldRefAccess<Country, float>("relationshipWithPlayer");

        public static bool Active { get; private set; }

        private readonly GameState gameState;
        private readonly Countries player;
        private readonly List<(Countries key, Country country, float playerRelations)> swapped =
            new List<(Countries, Country, float)>();

        private readonly Countries? playerPuppetOf;

        private UnionContext(GameState gameState)
        {
            this.gameState = gameState;
            player = gameState.playerCountry;
            playerPuppetOf = gameState.countries[player].puppetOf;
            gameState.countries[player].puppetOf = null;
            foreach (KeyValuePair<Countries, Country> subject in SovietUnion.Subjects(gameState))
            {
                swapped.Add((subject.Key, subject.Value, RelationshipWithPlayer(subject.Value)));
                RelationshipWithPlayer(subject.Value) = UnionRelations.Get(gameState, subject.Key);
            }

            gameState.playerCountry = Countries.USSR;
            Active = true;
        }

        public static UnionContext Enter(GameState gameState) => new UnionContext(gameState);

        public void Dispose()
        {
            gameState.playerCountry = player;
            gameState.countries[player].puppetOf = playerPuppetOf;
            foreach ((Countries key, Country country, float playerRelations) in swapped)
            {
                UnionRelations.Set(gameState, key, RelationshipWithPlayer(country));
                RelationshipWithPlayer(country) = playerRelations;
            }

            Active = false;
        }
    }
}
