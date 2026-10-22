using System;
using System.Collections.Generic;
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
    }

    // У страны в игре одно поле отношений — к игроку. Отношения республик к союзному центру храним в штатном словаре
    // влияния великих держав под ключом СССР: у союзных республик он не используется, а в сохранение попадает сам
    public static class UnionRelations
    {
        public static float Get(Country country) =>
            country.pointOfInfluence != null && country.pointOfInfluence.TryGetValue(Countries.USSR, out float value) ? value : 50f;

        public static void Set(Country country, float value)
        {
            if (country.pointOfInfluence == null)
            {
                country.pointOfInfluence = new Dictionary<Countries, float>();
            }

            country.pointOfInfluence[Countries.USSR] = Math.Max(0f, Math.Min(100f, value));
        }

        // Пока игрок управлял Союзом, отношения республик к нему и были отношениями к центру
        public static void CaptureFromPlayer(GameState gameState)
        {
            foreach (KeyValuePair<Countries, Country> subject in SovietUnion.Subjects(gameState))
            {
                Set(subject.Value, subject.Value.RelationshipWithPlayer);
            }
        }
    }

    // Временно делает СССР страной игрока, чтобы прогнать штатный ИИ республик от его лица.
    // Отношения республик к игроку на это время подменяются отношениями к центру
    public sealed class UnionContext : IDisposable
    {
        private static readonly AccessTools.FieldRef<Country, float> RelationshipWithPlayer =
            AccessTools.FieldRefAccess<Country, float>("relationshipWithPlayer");

        public static bool Active { get; private set; }

        private readonly GameState gameState;
        private readonly Countries player;
        private readonly List<KeyValuePair<Country, float>> swapped = new List<KeyValuePair<Country, float>>();

        private UnionContext(GameState gameState)
        {
            this.gameState = gameState;
            player = gameState.playerCountry;
            foreach (KeyValuePair<Countries, Country> subject in SovietUnion.Subjects(gameState))
            {
                Country country = subject.Value;
                swapped.Add(new KeyValuePair<Country, float>(country, RelationshipWithPlayer(country)));
                RelationshipWithPlayer(country) = UnionRelations.Get(country);
            }

            gameState.playerCountry = Countries.USSR;
            Active = true;
        }

        public static UnionContext Enter(GameState gameState) => new UnionContext(gameState);

        public void Dispose()
        {
            gameState.playerCountry = player;
            foreach (KeyValuePair<Country, float> pair in swapped)
            {
                UnionRelations.Set(pair.Key, RelationshipWithPlayer(pair.Key));
                RelationshipWithPlayer(pair.Key) = pair.Value;
            }

            Active = false;
        }
    }
}
