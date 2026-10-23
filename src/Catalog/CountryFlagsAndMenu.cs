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

        // Флаг в меню новой игры: на август 1991 года
        public static Sprite ForMenu(CountryProfile profile) =>
            Load(profile?.communistFlagImage) ?? Load(profile?.flagImage);

        // Флаг страны игрока сейчас: при коммунистическом правительстве — советский
        public static Sprite Current(GameState gameState) => Load(CurrentImage(gameState));

        public static string CurrentImage(GameState gameState)
        {
            CountryProfile profile = CountryCatalog.Get(gameState.playerCountry);
            Country.GovernmentType? government = gameState.countries[gameState.playerCountry].governmentType;
            bool communist = government.HasValue && profile?.communistGovernments != null && profile.communistGovernments.Contains(government.Value);
            return communist && Load(profile.communistFlagImage) != null ? profile.communistFlagImage : profile?.flagImage;
        }

        public static Sprite Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (Cache.TryGetValue(fileName, out Sprite sprite))
            {
                return sprite;
            }

            string path = Interceptor.GetPath(Path.Combine("images", fileName));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!File.Exists(path) || !texture.LoadImage(File.ReadAllBytes(path)))
            {
                Debug.LogWarning($"[AugustDays] флаг {path} не загружается");
                Cache[fileName] = null;
                return null;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Cache[fileName] = sprite;
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

            Sprite sprite = CountryFlags.Current(gameState);
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
                Sprite sprite = CountryFlags.ForMenu(pair.Value);
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
        private static Characters? loadedPortraitOf;

        public static void Show(NewGameController controller, CountryProfile profile)
        {
            Characters? ruler = CountryLeadership.Resolve(profile.ruler);
            if (!ruler.HasValue || controller.politics == null || controller.politics.Count == 0)
            {
                return;
            }

            if (card == null)
            {
                // Образец — карточка из набора политиков 1991 года: Янаев есть только в нем
                PoliticEntity source = controller.politics.FirstOrDefault(p => p != null && CharacterRef(p) == Characters.Yanayev) ?? controller.politics[0];
                loadedPortraitOf = null;
                Transform setsParent = controller.politicSets[0].transform.parent;
                GameObject clone = Object.Instantiate(source.gameObject, setsParent);
                clone.name = "CatalogRuler";
                // Ставим на место первой карточки того же набора
                Transform firstSlot = source.transform.parent.childCount > 0 ? source.transform.parent.GetChild(0) : source.transform;
                ((RectTransform)clone.transform).position = firstSlot.position;
                card = clone.GetComponent<PoliticEntity>();
            }

            CharacterRef(card) = ruler.Value;
            StartVariantRef(card) = 0;
            // Карточка скопирована с чужой: портрет правителя лежит отдельным слоем поверх области фото,
            // а пока он не загрузился, область закрашена цветом бумаги
            Image portraitImage = PortraitLayer();
            if (portraitImage != null && loadedPortraitOf != ruler.Value)
            {
                portraitImage.sprite = null;
                portraitImage.color = PaperColor;
            }

            card.gameObject.SetActive(true);

            controller.StartCoroutine(LoadPortrait(controller, ruler.Value));

            cardText = profile.rulerCard;
            ApplyText(card);
        }

        private static string cardText;

        // Карточка сама перерисовывает подпись по ключам локализации, которых у правителей из справочника нет
        public static void ApplyText(PoliticEntity entity)
        {
            if (entity != null && entity == card && CardTextRef(entity) != null && !string.IsNullOrEmpty(cardText))
            {
                CardTextRef(entity).SetText(cardText);
            }
        }

        // Портреты игра подгружает асинхронно: ждем, пока портрет правителя станет доступен
        private static System.Collections.IEnumerator LoadPortrait(NewGameController controller, Characters ruler)
        {
            for (int attempt = 0; attempt < 50 && card != null; attempt++)
            {
                GameState tooltipState = controller.GetScenarioGameStateForTooltip();
                Image layer = PortraitLayer();
                if (layer != null && tooltipState != null && tooltipState.characters.ContainsKey(ruler) && PortraitsManager.inst != null)
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
                        layer.sprite = portrait;
                        layer.color = Color.white;
                        loadedPortraitOf = ruler;
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }
        }

        private static readonly Color PaperColor = new Color(0.93f, 0.9f, 0.82f);

        // Область портрета на карточке 1991 года: сверху, над полосой с подписью
        private static readonly Vector2 PortraitMin = new Vector2(0.0625f, 0.355f);
        private static readonly Vector2 PortraitMax = new Vector2(0.935f, 0.96f);

        private static Image PortraitLayer()
        {
            Transform photo = card != null ? card.transform.Find("Photo") : null;
            if (photo == null)
            {
                return null;
            }

            Transform existing = photo.Find("CatalogPortrait");
            if (existing != null)
            {
                return existing.GetComponent<Image>();
            }

            var layer = new GameObject("CatalogPortrait", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)layer.transform;
            rect.SetParent(photo, false);
            rect.anchorMin = PortraitMin;
            rect.anchorMax = PortraitMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = layer.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        public static void Hide()
        {
            if (card != null)
            {
                card.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(PoliticEntity), "RepaintCardText")]
    public static class PatchPoliticEntityRepaintCardText
    {
        static void Postfix(PoliticEntity __instance) => RulerCard.ApplyText(__instance);
    }
}
