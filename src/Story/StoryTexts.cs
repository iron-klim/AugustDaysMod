using System;
using System.Collections.Generic;
using System.IO;
using AugustDaysMod.Assets;
using Newtonsoft.Json;
using UnityEngine;

// Тексты сюжетных событий и концовок мода (assets/story.json, английские — assets/story_en.json)
namespace AugustDaysMod.Story
{
    public class StoryOptionText
    {
        [JsonProperty] public string name = "";
        [JsonProperty] public string result = "";
    }

    public class StoryEventText
    {
        [JsonProperty] public string title = "";
        [JsonProperty] public string description = "";
        [JsonProperty] public List<StoryOptionText> options = new List<StoryOptionText>();

        // Варианты события с другим текстом, например исход голосования
        [JsonProperty] public Dictionary<string, StoryEventText> variants = new Dictionary<string, StoryEventText>();
    }

    public class StoryEndingText
    {
        [JsonProperty] public string title = "";
        [JsonProperty] public string description = "";
    }

    public static class StoryTexts
    {
        public const string FILE_NAME = "story.json";

        private class StoryFile
        {
            [JsonProperty] public Dictionary<int, StoryEventText> events = new Dictionary<int, StoryEventText>();
            [JsonProperty] public Dictionary<int, StoryEndingText> endings = new Dictionary<int, StoryEndingText>();
            [JsonProperty] public Dictionary<int, StoryEventText> decisions = new Dictionary<int, StoryEventText>();
        }

        private static StoryFile file;
        private static string fileLanguage;

        // Язык игры можно сменить в настройках: тогда тексты перечитываются
        private static StoryFile File
        {
            get
            {
                if (file == null || fileLanguage != ModLanguage.Code)
                {
                    fileLanguage = ModLanguage.Code;
                    file = Load();
                }

                return file;
            }
        }

        public static StoryEventText Event(int id, string variant = null)
        {
            if (!File.events.TryGetValue(id, out StoryEventText text))
            {
                Debug.LogWarning($"[AugustDays] нет текста события {id} в {FILE_NAME}");
                return new StoryEventText { title = ModLanguage.Pick($"Событие {id}", $"Event {id}"), description = "" };
            }

            return variant != null && text.variants.TryGetValue(variant, out StoryEventText alternative) ? alternative : text;
        }

        public static StoryEventText Decision(int id) =>
            File.decisions.TryGetValue(id, out StoryEventText text) ? text : new StoryEventText { title = ModLanguage.Pick($"Решение {id}", $"Decision {id}") };

        public static StoryEndingText Ending(int id) =>
            File.endings.TryGetValue(id, out StoryEndingText text) ? text : new StoryEndingText { title = ModLanguage.Pick($"Концовка {id}", $"Ending {id}") };

        // Переведенные записи заменяют русские по номеру, непереведенные остаются русскими
        private static StoryFile Load()
        {
            StoryFile russian = Read(Interceptor.GetPath(FILE_NAME)) ?? new StoryFile();
            string translationPath = ModLanguage.TranslationPath(FILE_NAME);
            StoryFile translation = translationPath != null ? Read(translationPath) : null;
            if (translation != null)
            {
                Overlay(russian.events, translation.events);
                Overlay(russian.endings, translation.endings);
                Overlay(russian.decisions, translation.decisions);
            }

            return russian;
        }

        private static void Overlay<T>(Dictionary<int, T> target, Dictionary<int, T> source)
        {
            foreach (KeyValuePair<int, T> pair in source)
            {
                target[pair.Key] = pair.Value;
            }
        }

        private static StoryFile Read(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<StoryFile>(System.IO.File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[AugustDays] тексты сюжета {path} не читаются: {exception.Message}");
                return null;
            }
        }
    }
}
