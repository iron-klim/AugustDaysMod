using System;
using System.Collections.Generic;
using System.IO;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Справочник поддерживаемых стран (assets/countries.json): собственные показатели страны, которые
// игра хранит одними на страну игрока. Применяются, когда игрок переходит в эту страну
namespace AugustDaysMod.Catalog
{
    public class CountryProfile
    {
        [JsonProperty] public string name;

        // Экономический вес относительно СССР. Доходы отраслей и инфляция в игре рассчитаны на масштаб Союза
        [JsonProperty] public float economyScale = 1f;

        // Базовый доход. В игре он задан сценарием и в сохранение не попадает, поэтому живет здесь
        [JsonProperty] public int? baseIncome;

        // Ежемесячная прибавка политической власти: у СССР ее дает лояльность республик, у страны без республик —
        // легитимность собственной власти
        [JsonProperty] public float politicalPowerBonus;

        // Зависимая страна: бывшая союзная республика, армия и ядерный арсенал которой достались ей от Союза.
        // Концовки, рассчитанные на великую державу, к ней не применяются
        [JsonProperty] public bool dependent;

        [JsonProperty] public string comment;

        // Название страны игрока и его сокращение в текстах игры
        [JsonProperty] public string fullName;
        [JsonProperty] public string abbreviation;

        // Картинка флага в папке images мода
        [JsonProperty] public string flagImage;

        // Описание старта в меню новой игры
        [JsonProperty] public string startDescription;

        // Руководство. Персонажи задаются именем из Characters или @ключом нового персонажа из newCharacters
        [JsonProperty] public string ruler;
        [JsonProperty] public Dictionary<PoliticPosition, string> posts;
        [JsonProperty] public List<string> politicians;
        [JsonProperty] public List<string> council;
        [JsonProperty] public List<NewCharacter> newCharacters;

        // Отношения стран к стране игрока на старте
        [JsonProperty] public Dictionary<Countries, float> relations;

        // Поля GameState под теми же именами, что в state.json
        [JsonProperty] public JObject indicators;
    }

    // Политик, которого нет в игре. Портрет — нейтральный силуэт из папки portraits мода, если не задан свой
    public class NewCharacter
    {
        [JsonProperty] public string key;

        // Персонаж игры, у которого нет записи в состоянии: имя и портрет остаются из игры
        [JsonProperty] public string character;

        [JsonProperty] public string name;
        [JsonProperty] public string surname;
        [JsonProperty] public int age = 50;
        [JsonProperty] public bool isWoman;
        [JsonProperty] public Backend.Gamedesign.Regions homeLand = Backend.Gamedesign.Regions.MainUkraine;
        [JsonProperty] public List<Traits> traits = new List<Traits>();
        [JsonProperty] public int power = 30;
        [JsonProperty] public int diplomacy = 3;
        [JsonProperty] public int intrigue = 3;
        [JsonProperty] public int thrift = 3;
        [JsonProperty] public string portrait = "silhouette.png";
    }

    public static class CountryCatalog
    {
        public const string FILE_NAME = "countries.json";

        // Показатели, которые в GameState принадлежат стране игрока. Остальное (страны, политики, мир) справочник не трогает
        private static readonly HashSet<string> PlayerIndicators = new HashSet<string>
        {
            "playerCountryCustomName", "reserve", "usLoan", "fraLoan", "imfLoan", "export", "priceIndex", "politicalPower",
            "militaryStaffLoyalty", "armyStaffLoyalty", "specialServicesLoyalty", "specialServices", "radicalsPower",
            "freedomLevel", "liberalizationLevel", "unityLevel", "intelligentsiaLoyalty", "spiritualContentment", "orderLevel",
            "healthCare", "education", "ecology", "educationAccess", "healthCareAccess", "selfFulfillment", "luxuryGoodsLevel",
            "firstNeedsGoods", "housingLevel", "employmentLevel", "agroEffectiveness", "servicesEffectiveness",
            "lightIndustryEffectiveness", "heavyIndustryEffectiveness", "armyIndustryEffectiveness", "competitivenessLevel",
            "corruptionLevel", "forgeryLevel", "armyQuantityLevel", "warheadsQuantity", "combatability", "expenses"
        };

        private class CatalogFile
        {
            [JsonProperty] public Dictionary<Countries, CountryProfile> countries =
                new Dictionary<Countries, CountryProfile>();
        }

        private static Dictionary<Countries, CountryProfile> profiles;

        public static IReadOnlyDictionary<Countries, CountryProfile> Profiles => profiles ?? (profiles = Load());

        public static bool IsSupported(Countries country) => Profiles.ContainsKey(country);

        public static CountryProfile Get(Countries country) =>
            Profiles.TryGetValue(country, out CountryProfile profile) ? profile : null;

        public static bool IsDependent(GameState gameState) => Get(gameState.playerCountry)?.dependent == true;

        public static float EconomyScale(GameState gameState)
        {
            CountryProfile profile = Get(gameState.playerCountry);
            return profile != null && profile.economyScale > 0f ? profile.economyScale : 1f;
        }

        // Записывает показатели страны в GameState поверх текущих
        public static void ApplyIndicators(GameState gameState, CountryProfile profile, JsonSerializerSettings settings)
        {
            if (profile?.indicators == null)
            {
                return;
            }

            JsonConvert.PopulateObject(profile.indicators.ToString(), gameState, settings);
        }

        private static Dictionary<Countries, CountryProfile> Load()
        {
            string path = Interceptor.GetPath(FILE_NAME);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[AugustDays] нет справочника стран {path}");
                return new Dictionary<Countries, CountryProfile>();
            }

            CatalogFile file;
            try
            {
                file = JsonConvert.DeserializeObject<CatalogFile>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[AugustDays] справочник стран не читается: {exception.Message}");
                return new Dictionary<Countries, CountryProfile>();
            }

            foreach (KeyValuePair<Countries, CountryProfile> pair in file.countries)
            {
                Validate(pair.Key, pair.Value);
                foreach (NewCharacter newCharacter in pair.Value.newCharacters ?? new List<NewCharacter>())
                {
                    if (string.IsNullOrEmpty(newCharacter.character))
                    {
                        NewCharacterPortraits.Register(CountryLeadership.IdOf(newCharacter), newCharacter.portrait);
                    }
                }
            }

            Debug.Log($"[AugustDays] справочник стран: {string.Join(", ", file.countries.Keys)}");
            return file.countries;
        }

        // Неизвестные поля убираем: опечатка в имени не должна молча затирать что-то другое в состоянии игры
        private static void Validate(Countries country, CountryProfile profile)
        {
            if (profile.indicators == null)
            {
                return;
            }

            foreach (JProperty property in new List<JProperty>(profile.indicators.Properties()))
            {
                if (!PlayerIndicators.Contains(property.Name))
                {
                    Debug.LogWarning($"[AugustDays] справочник стран, {country}: показатель «{property.Name}» не поддерживается и пропущен");
                    property.Remove();
                }
            }
        }
    }
}
