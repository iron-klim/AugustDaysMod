using System.IO;
using Backend.Managers;
using UnityEngine;

// Язык текстов мода. Мод написан на русском и переведен на английский: при русском языке игры тексты русские,
// при любом другом — английские
namespace AugustDaysMod.Assets
{
    public static class ModLanguage
    {
        public static bool Russian => LanguageManager.inst == null || LanguageManager.inst.language == SystemLanguage.Russian;

        public static string Code => Russian ? "ru" : "en";

        public static string Pick(string ru, string en) => Russian ? ru : en;

        // Английский файл лежит рядом с русским: story.json → story_en.json. Его нет — null
        public static string TranslationPath(string fileName)
        {
            if (Russian)
            {
                return null;
            }

            string path = Interceptor.GetPath($"{Path.GetFileNameWithoutExtension(fileName)}_en{Path.GetExtension(fileName)}");
            return File.Exists(path) ? path : null;
        }
    }
}
