using System;
using System.Collections.Generic;
using System.IO;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Managers;
using HarmonyLib;
using UnityEngine;

namespace AugustDaysMod.Catalog
{
    // Руководство страны игрока из справочника: правитель, посты, политики, совет и новые персонажи
    public static class CountryLeadership
    {
        // Новые персонажи получают постоянные номера из своего ключа, вне диапазонов игры и модов игры
        public static Characters NewCharacterId(string key)
        {
            uint hash = 2166136261;
            foreach (char symbol in key)
            {
                hash = (hash ^ symbol) * 16777619;
            }

            return (Characters)(-(600000 + (int)(hash % 300000)));
        }

        public static Characters? Resolve(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                return null;
            }

            if (reference.StartsWith("@"))
            {
                return NewCharacterId(reference.Substring(1));
            }

            if (Enum.TryParse(reference, out Characters character))
            {
                return character;
            }

            Debug.LogWarning($"[AugustDays] справочник стран: незнакомый политик «{reference}»");
            return null;
        }

        public static Characters IdOf(NewCharacter newCharacter) =>
            !string.IsNullOrEmpty(newCharacter.character) && Enum.TryParse(newCharacter.character, out Characters existing)
                ? existing
                : NewCharacterId(newCharacter.key);

        public static void EnsureNewCharacters(GameState gameState, CountryProfile profile)
        {
            if (profile.newCharacters == null)
            {
                return;
            }

            foreach (NewCharacter newCharacter in profile.newCharacters)
            {
                Characters id = IdOf(newCharacter);
                _ = gameState.politicPositions[id];
                if (gameState.characters.ContainsKey(id))
                {
                    continue;
                }

                var info = new CharacterInfo
                {
                    status = CharacterInfo.PoliticStatus.NotPresented,
                    birthDay = gameState.date.AddYears(-Mathf.Clamp(newCharacter.age, 18, 100)),
                    isWoman = newCharacter.isWoman,
                    homeLand = newCharacter.homeLand,
                };

                // У персонажа игры без записи в состоянии остаются его имя и портрет из игры
                if (string.IsNullOrEmpty(newCharacter.character))
                {
                    info.customCharacterInfo = new CustomCharacterInfo
                    {
                        politicName = newCharacter.name,
                        politicSurname = newCharacter.surname,
                        pictureNumber = (int)id,
                        wasEnoughPoints = true
                    };
                }

                info.Power = Mathf.Clamp(newCharacter.power, 0, 100);
                info.charLevel[CharacterInfo.PoliticCharacteristic.Diplomacy] = Mathf.Clamp(newCharacter.diplomacy, 0, 10);
                info.charLevel[CharacterInfo.PoliticCharacteristic.Intrigue] = Mathf.Clamp(newCharacter.intrigue, 0, 10);
                info.charLevel[CharacterInfo.PoliticCharacteristic.Thrift] = Mathf.Clamp(newCharacter.thrift, 0, 10);
                info.traits.UnionWith(newCharacter.traits);
                gameState.characters[id] = info;
            }
        }

        // Политическая система страны игрока: только ее политики, ее посты и ее совет
        public static void Apply(GameState gameState, CountryProfile profile)
        {
            EnsureNewCharacters(gameState, profile);

            Characters? ruler = Resolve(profile.ruler) ?? gameState.gameCharacter;
            var allowed = new HashSet<Characters> { ruler.Value };
            var posts = new Dictionary<PoliticPosition, Characters>();
            if (profile.posts != null)
            {
                foreach (KeyValuePair<PoliticPosition, string> post in profile.posts)
                {
                    Characters? holder = Resolve(post.Value);
                    if (holder.HasValue)
                    {
                        posts[post.Key] = holder.Value;
                        allowed.Add(holder.Value);
                    }
                }
            }

            var council = new HashSet<Characters>();
            foreach (string reference in profile.council ?? new List<string>())
            {
                Characters? member = Resolve(reference);
                if (member.HasValue)
                {
                    council.Add(member.Value);
                    allowed.Add(member.Value);
                }
            }

            foreach (string reference in profile.politicians ?? new List<string>())
            {
                Characters? politician = Resolve(reference);
                if (politician.HasValue)
                {
                    allowed.Add(politician.Value);
                }
            }

            foreach (KeyValuePair<Characters, CharacterInfo> character in gameState.characters)
            {
                if (allowed.Contains(character.Key))
                {
                    if (character.Value.status != CharacterInfo.PoliticStatus.Dead)
                    {
                        character.Value.status = CharacterInfo.PoliticStatus.Alive;
                    }
                }
                else if (character.Value.status == CharacterInfo.PoliticStatus.Alive)
                {
                    character.Value.status = CharacterInfo.PoliticStatus.NotPresented;
                }
            }

            foreach (PoliticPosition position in Enum.GetValues(typeof(PoliticPosition)))
            {
                gameState.politicPositions[position] = null;
            }

            gameState.gameCharacter = ruler.Value;
            Assign(gameState, ruler.Value, PoliticPosition.President);
            foreach (KeyValuePair<PoliticPosition, Characters> post in posts)
            {
                if (gameState.characters.ContainsKey(post.Value))
                {
                    Assign(gameState, post.Value, post.Key);
                }
            }

            gameState.politBuro.Clear();
            gameState.politBuro.UnionWith(council);
            gameState.membersOfPolitBuro = council.Count;
        }

        // Словарь постов узнает о персонаже только через чтение: без этого назначение на пост падает
        public static void Assign(GameState gameState, Characters character, PoliticPosition position)
        {
            _ = gameState.politicPositions[character];
            gameState.politicPositions[character] = position;
        }
    }

    // Портреты новых персонажей подгружаются по первому запросу: менеджер картинок игры их не сохраняет
    public static class NewCharacterPortraits
    {
        private static readonly Dictionary<int, string> Paths = new Dictionary<int, string>();
        private static readonly HashSet<int> Loaded = new HashSet<int>();

        public static void Register(Characters id, string file) => Paths[(int)id] = Interceptor.GetPath(Path.Combine("portraits", file));

        public static void EnsureLoaded(int number)
        {
            if (Loaded.Contains(number) || !Paths.TryGetValue(number, out string path) || PlayerCustomImagesManager.inst == null)
            {
                return;
            }

            Loaded.Add(number);
            if (!File.Exists(path) || !PlayerCustomImagesManager.inst.RegisterExternalPortrait(path, number))
            {
                Debug.LogWarning($"[AugustDays] портрет {path} не загружается");
            }
        }
    }

    [HarmonyPatch(typeof(PlayerCustomImagesManager), nameof(PlayerCustomImagesManager.GetSpriteByNumber))]
    public static class PatchGetSpriteByNumber
    {
        static void Prefix(int number) => NewCharacterPortraits.EnsureLoaded(number);
    }
}
