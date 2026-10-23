using AugustDaysMod.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using AugustDaysMod.Storage;
using Backend.Gamedesign;
using Backend.Gamedesign.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AugustDaysMod.Union
{
    // Жизнь Союза без игрока: сплоченность центра, отношения республик, выход республик и распад
    public static class UnionDynamics
    {
        public const float INDEPENDENCE_RELATIONS = 30f;
        public const float FORCED_INDEPENDENCE_RELATIONS = 15f;
        public const float INDEPENDENCE_CHANCE = 0.35f;
        public const int DISSOLUTION_SUBJECTS = 3;

        // Союзные республики. Остальные субъекты СССР (Монголия, Куба и т. д.) — сфера влияния, а не состав Союза
        public static readonly HashSet<Countries> Republics = new HashSet<Countries>
        {
            Countries.Russia, Countries.Ukraine, Countries.Belorussia, Countries.Moldavia, Countries.Litva,
            Countries.Latvia, Countries.Estonia, Countries.Kazakhstan, Countries.Uzbekistan, Countries.Kyrgizia,
            Countries.Tajikistan, Countries.Turkmenia, Countries.Armenia, Countries.Azerbaijan, Countries.Georgia
        };

        public static IEnumerable<KeyValuePair<Countries, Country>> MemberRepublics(GameState gameState) =>
            SovietUnion.Subjects(gameState).Where(pair => Republics.Contains(pair.Key) && pair.Value.puppetType < Country.PuppetType.PuppetOtherState);

        public static void ChangeCohesion(GameState gameState, float delta)
        {
            UnionData union = SovietUnion.Data(gameState);
            union.cohesion = Mathf.Clamp(union.cohesion + delta, 0f, 100f);
        }

        // Раз в месяц, после штатного ИИ республик
        public static void Tick(GameState gameState)
        {
            UnionData union = SovietUnion.Data(gameState);
            if (union.dissolved)
            {
                return;
            }

            List<KeyValuePair<Countries, Country>> members = MemberRepublics(gameState).ToList();
            if (members.Count == 0)
            {
                Dissolve(gameState, "republics");
                return;
            }

            foreach (KeyValuePair<Countries, Country> member in members)
            {
                UnionRelations.Add(gameState, member.Key, RelationsDrift(union, member.Value));
            }

            int seceded = 0;
            // Страна игрока выходит из Союза по сюжету, а не по отношениям к центру
            foreach (KeyValuePair<Countries, Country> member in members)
            {
                if (member.Key != gameState.playerCountry && WantsIndependence(gameState, member.Key, member.Value))
                {
                    DeclareIndependence(gameState, member.Key);
                    seceded++;
                    if (member.Key == Countries.Russia)
                    {
                        Dissolve(gameState, "russia");
                        return;
                    }
                }
            }

            float averageRelations = members.Average(member => UnionRelations.Get(gameState, member.Key));
            ChangeCohesion(gameState, -1f + (averageRelations - 50f) / 50f - seceded);

            // Союз держится, пока в нем Россия и хотя бы несколько республик: даже без власти центр формально существует
            if (MemberRepublics(gameState).Count() <= DISSOLUTION_SUBJECTS)
            {
                Dissolve(gameState, "republics");
            }
        }

        private static float RelationsDrift(UnionData union, Country country)
        {
            float leader;
            switch (country.leaderType)
            {
                case Country.LeaderType.Separatist:
                    leader = -1.5f;
                    break;
                case Country.LeaderType.Loyal:
                    leader = 0.5f;
                    break;
                default:
                    leader = -0.3f;
                    break;
            }

            return (union.cohesion - 50f) / 25f + leader + Random.Range(-1f, 1f);
        }

        private static bool WantsIndependence(GameState gameState, Countries key, Country country)
        {
            float relations = UnionRelations.Get(gameState, key);
            if (relations < FORCED_INDEPENDENCE_RELATIONS)
            {
                return true;
            }

            return country.leaderType == Country.LeaderType.Separatist && relations < INDEPENDENCE_RELATIONS &&
                   Random.value < INDEPENDENCE_CHANCE;
        }

        public static void DeclareIndependence(GameState gameState, Countries key)
        {
            Country country = gameState.countries[key];
            if (country.puppetOf != Countries.USSR)
            {
                return;
            }

            country.puppetOf = null;
            country.puppetType = Country.PuppetType.None;
            SovietUnion.Data(gameState).independence[key] = gameState.date;
            string name = CountryUtils.GetCountryName(gameState, key);
            UnionNews.Add(gameState, ModLanguage.Pick($"{name} провозглашает независимость и выходит из состава СССР.",
                $"{name} declares independence and leaves the USSR."));
            // Без России Союза нет
            if (key == Countries.Russia)
            {
                Dissolve(gameState, "russia");
            }
        }

        // Союз прекращает существование: оставшиеся республики становятся независимыми
        public static void Dissolve(GameState gameState, string reason)
        {
            UnionData union = SovietUnion.Data(gameState);
            if (union.dissolved)
            {
                return;
            }

            foreach (KeyValuePair<Countries, Country> member in MemberRepublics(gameState).ToList())
            {
                Country country = member.Value;
                country.puppetOf = null;
                country.puppetType = Country.PuppetType.None;
                union.independence[member.Key] = gameState.date;
            }

            union.dissolved = true;
            union.dissolvedDate = gameState.date;
            union.cohesion = 0f;
            gameState.countries[Countries.USSR].activeInGame = false;
            Debug.Log($"[AugustDays] Союз распущен {gameState.date:dd.MM.yyyy}, причина {reason}");

            string text;
            switch (reason)
            {
                case "russia":
                    text = ModLanguage.Pick(
                        "Россия заявляет о выходе из Союза. Без нее союзный центр теряет всякий смысл: СССР прекращает существование, все республики становятся независимыми государствами.",
                        "Russia announces its withdrawal from the Union. Without it the Union centre loses all meaning: the USSR ceases to exist and all the republics become independent states.");
                    break;
                case "agreement":
                    text = ModLanguage.Pick(
                        "Руководители республик подписали соглашение о прекращении существования СССР как субъекта международного права и геополитической реальности. Над Кремлем спущен красный флаг.",
                        "The leaders of the republics have signed an agreement declaring that the USSR has ceased to exist as a subject of international law and a geopolitical reality. The red flag has been lowered over the Kremlin.");
                    break;
                default:
                    text = ModLanguage.Pick(
                        "Союзный центр больше ничего не контролирует. СССР прекращает существование, оставшиеся республики становятся независимыми государствами.",
                        "The Union centre no longer controls anything. The USSR ceases to exist, and the remaining republics become independent states.");
                    break;
            }

            UnionNews.Add(gameState, text);
        }
    }

    // Очередь новостей о Союзе для игрока. Показываются событием «Вести из Союза»
    public static class UnionNews
    {
        public static void Add(GameState gameState, string text)
        {
            ModStorage.For(gameState).news.Add(text);
            Debug.Log($"[AugustDays] новость: {text}");
        }

        public static string TakeAll(GameState gameState)
        {
            List<string> news = ModStorage.For(gameState).news;
            string text = string.Join("\n\n", news);
            news.Clear();
            return text;
        }
    }
}
