using System.Collections.Generic;
using System.Linq;
using System.IO;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Managers;
using Frontend.NewGame;
using Frontend.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AugustDaysMod.Catalog
{
    public static class CountryFlags
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(CountryProfile profile)
        {
            if (string.IsNullOrEmpty(profile?.flagImage))
            {
                return null;
            }

            if (Cache.TryGetValue(profile.flagImage, out Sprite sprite))
            {
                return sprite;
            }

            string path = Interceptor.GetPath(Path.Combine("images", profile.flagImage));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!File.Exists(path) || !texture.LoadImage(File.ReadAllBytes(path)))
            {
                Debug.LogWarning($"[AugustDays] флаг {path} не загружается");
                Cache[profile.flagImage] = null;
                return null;
            }

            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Cache[profile.flagImage] = sprite;
            return sprite;
        }
    }

    // Флаг страны игрока в окне статистики
    [HarmonyPatch(typeof(FlagDisplayer), nameof(FlagDisplayer.DisplayFlag))]
    public static class PatchFlagDisplayer
    {
        private static readonly AccessTools.FieldRef<FlagDisplayer, Image> ImageRef = AccessTools.FieldRefAccess<FlagDisplayer, Image>("image");

        static void Postfix(FlagDisplayer __instance)
        {
            GameState gameState = GameStateManager.inst?.GameState;
            if (gameState == null || gameState.playerCountry == Countries.USSR)
            {
                return;
            }

            Sprite sprite = CountryFlags.Get(CountryCatalog.Get(gameState.playerCountry));
            if (sprite != null)
            {
                ImageRef(__instance).sprite = sprite;
            }
        }
    }

    // Меню новой игры: флаги стран из справочника рядом с советским. Правителя выбирать не нужно — его задает справочник
    public static class NewGameCountries
    {
        public static bool IsCatalogCountry(Countries? country) =>
            country.HasValue && country.Value != Countries.USSR && CountryCatalog.IsSupported(country.Value);
    }

    [HarmonyPatch(typeof(NewGameController), "Awake")]
    public static class PatchNewGameControllerAwake
    {
        private static readonly AccessTools.FieldRef<FlagEntity, Countries> CountryRef = AccessTools.FieldRefAccess<FlagEntity, Countries>("country");
        private static readonly AccessTools.FieldRef<FlagEntity, GameObject> ComingSoonRef = AccessTools.FieldRefAccess<FlagEntity, GameObject>("comingSoon");

        static void Postfix(NewGameController __instance)
        {
            if (__instance.flags == null || __instance.flags.Count == 0)
            {
                return;
            }

            FlagEntity source = __instance.flags[0];
            var sourceRect = (RectTransform)source.transform;
            int added = 0;
            foreach (KeyValuePair<Countries, CountryProfile> pair in CountryCatalog.Profiles)
            {
                Sprite sprite = CountryFlags.Get(pair.Value);
                if (pair.Key == Countries.USSR || sprite == null || __instance.flags.Exists(existing => CountryRef(existing) == pair.Key))
                {
                    continue;
                }

                GameObject clone = Object.Instantiate(source.gameObject, source.transform.parent);
                clone.name = $"Flag{pair.Key}";
                FlagEntity flag = clone.GetComponent<FlagEntity>();
                CountryRef(flag) = pair.Key;
                if (ComingSoonRef(flag) != null)
                {
                    ComingSoonRef(flag).SetActive(false);
                }

                clone.GetComponent<Image>().sprite = sprite;
                added++;
                if (source.transform.parent.GetComponent<LayoutGroup>() == null)
                {
                    ((RectTransform)clone.transform).anchoredPosition = sourceRect.anchoredPosition + new Vector2((sourceRect.rect.width + 12f) * added, 0f);
                }

                __instance.flags.Add(flag);
            }
        }
    }

    [HarmonyPatch(typeof(NewGameController), nameof(NewGameController.SetCountry))]
    public static class PatchNewGameControllerSetCountry
    {
        static void Postfix(NewGameController __instance, Countries country)
        {
            if (!NewGameCountries.IsCatalogCountry(country))
            {
                return;
            }

            Characters? ruler = CountryLeadership.Resolve(CountryCatalog.Get(country).ruler);
            if (ruler.HasValue)
            {
                __instance.currentPolitic = ruler;
                __instance.Repaint();
            }
        }
    }

    // Для страны из справочника наборы советских политиков не показываем, а описание берем из справочника
    [HarmonyPatch(typeof(NewGameController), nameof(NewGameController.Repaint))]
    [HarmonyPriority(Priority.Last)]
    public static class PatchNewGameControllerRepaintCountry
    {
        static void Postfix(NewGameController __instance)
        {
            if (!NewGameCountries.IsCatalogCountry(__instance.currentCountry))
            {
                RulerCard.Hide();
                return;
            }

            foreach (GameObject set in __instance.politicSets)
            {
                set.SetActive(false);
            }

            __instance.addPoliticianButton.SetActive(false);
            CountryProfile profile = CountryCatalog.Get(__instance.currentCountry.Value);
            RulerCard.Show(__instance, profile);
            if (!string.IsNullOrEmpty(profile.startDescription))
            {
                __instance.descText.SetText(profile.startDescription);
            }
        }
    }

    // Карточка правителя страны из справочника: копия карточки советского политика с другим персонажем и портретом
    public static class RulerCard
    {
        private static readonly AccessTools.FieldRef<PoliticEntity, Characters> CharacterRef = AccessTools.FieldRefAccess<PoliticEntity, Characters>("character");
        private static readonly AccessTools.FieldRef<PoliticEntity, int> StartVariantRef = AccessTools.FieldRefAccess<PoliticEntity, int>("startVariant");
        private static readonly AccessTools.FieldRef<PoliticEntity, TMPro.TextMeshProUGUI> CardTextRef = AccessTools.FieldRefAccess<PoliticEntity, TMPro.TextMeshProUGUI>("cardText");

        private static PoliticEntity card;

        public static void Show(NewGameController controller, CountryProfile profile)
        {
            Characters? ruler = CountryLeadership.Resolve(profile.ruler);
            if (!ruler.HasValue || controller.politics == null || controller.politics.Count == 0)
            {
                return;
            }

            if (card == null)
            {
                PoliticEntity source = controller.politics[0];
                Transform setsParent = controller.politicSets[0].transform.parent;
                GameObject clone = Object.Instantiate(source.gameObject, setsParent);
                clone.name = "CatalogRuler";
                ((RectTransform)clone.transform).position = source.transform.position;
                card = clone.GetComponent<PoliticEntity>();
            }

            CharacterRef(card) = ruler.Value;
            StartVariantRef(card) = 0;
            card.gameObject.SetActive(true);

            controller.StartCoroutine(LoadPortrait(controller, ruler.Value));

            if (CardTextRef(card) != null && !string.IsNullOrEmpty(profile.rulerCard))
            {
                CardTextRef(card).SetText(profile.rulerCard);
            }
        }

        // Портреты игра подгружает асинхронно: ждем, пока портрет правителя станет доступен
        private static System.Collections.IEnumerator LoadPortrait(NewGameController controller, Characters ruler)
        {
            for (int attempt = 0; attempt < 50 && card != null; attempt++)
            {
                GameState tooltipState = controller.GetScenarioGameStateForTooltip();
                Transform photo = card.transform.Find("Photo");
                if (photo != null && tooltipState != null && tooltipState.characters.ContainsKey(ruler) && PortraitsManager.inst != null)
                {
                    Sprite portrait = null;
                    try
                    {
                        portrait = PortraitsManager.inst.GetPortrait(tooltipState, ruler);
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                    }

                    if (portrait != null)
                    {
                        photo.GetComponent<Image>().sprite = portrait;
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }
        }

        public static void Hide()
        {
            if (card != null)
            {
                card.gameObject.SetActive(false);
            }
        }
    }
}
