using System;
using System.Collections.Generic;
using System.IO;
using AugustDaysMod.Assets;
using Newtonsoft.Json;
using UnityEngine;

// Тексты сюжетных событий и концовок мода (assets/story.json)
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

        private static StoryFile File => file ?? (file = Load());

        public static StoryEventText Event(int id, string variant = null)
        {
            if (!File.events.TryGetValue(id, out StoryEventText text))
            {
                Debug.LogWarning($"[AugustDays] нет текста события {id} в {FILE_NAME}");
                return new StoryEventText { title = $"Событие {id}", description = "" };
            }

            return variant != null && text.variants.TryGetValue(variant, out StoryEventText alternative) ? alternative : text;
        }

        public static StoryEventText Decision(int id) =>
            File.decisions.TryGetValue(id, out StoryEventText text) ? text : new StoryEventText { title = $"Решение {id}" };

        public static StoryEndingText Ending(int id) =>
            File.endings.TryGetValue(id, out StoryEndingText text) ? text : new StoryEndingText { title = $"Концовка {id}" };

        private static StoryFile Load()
        {
            string path = Interceptor.GetPath(FILE_NAME);
            try
            {
                return JsonConvert.DeserializeObject<StoryFile>(System.IO.File.ReadAllText(path)) ?? new StoryFile();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[AugustDays] тексты сюжета {path} не читаются: {exception.Message}");
                return new StoryFile();
            }
        }
    }
}
