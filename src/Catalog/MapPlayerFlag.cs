using System.Collections.Generic;
using Backend.Gamedesign;
using Backend.Managers;
using Frontend.Map;
using HarmonyLib;
using UnityEngine;

namespace AugustDaysMod.Catalog
{
    // На политической карте территория страны игрока закрашена ее флагом, а маски по форме ее регионов
    // обрезают флаг по границам
    [HarmonyPatch(typeof(MapController), "RepaintRegions")]
    public static class PatchMapPlayerFlag
    {
        private const string MASK_NAME = "AugustPlayerFlagMask";
        private const float ALPHA = 0.85f;

        private static readonly AccessTools.FieldRef<MapController, List<RegionController>> RegionsRef =
            AccessTools.FieldRefAccess<MapController, List<RegionController>>("regions");

        private static readonly AccessTools.FieldRef<int> MapModeRef = AccessTools.StaticFieldRefAccess<int>(AccessTools.Field(typeof(MapController), "currentMapMode"));

        private static SpriteRenderer background;
        private static SpriteRenderer hoist;
        private static SpriteRenderer edge;

        static void Postfix(MapController __instance)
        {
            GameState gameState = GameStateManager.inst?.GameState;
            List<RegionController> regions = RegionsRef(__instance);
            if (gameState == null || regions == null || regions.Count == 0)
            {
                return;
            }

            string image = gameState.playerCountry != Countries.USSR && MapModeRef() == 0 ? CountryFlags.CurrentImage(gameState) : null;
            Sprite sprite = CountryFlags.Load(image);
            MapFlagHoist hoistSettings = CountryCatalog.Get(gameState.playerCountry)?.mapFlagHoist;
            if (hoistSettings != null && hoistSettings.image != image)
            {
                hoistSettings = null;
            }

            bool hasBounds = false;
            Bounds bounds = default;
            Bounds? hoistRegionBounds = null;
            SpriteRenderer sample = null;
            foreach (RegionController region in regions)
            {
                if (region == null)
                {
                    continue;
                }

                SpriteRenderer renderer = region.GetComponent<SpriteRenderer>();
                bool own = sprite != null && renderer != null && gameState.regionsStorage[region.CurrentRegion] == gameState.playerCountry;
                Transform mask = region.transform.Find(MASK_NAME);
                if (!own)
                {
                    if (mask != null)
                    {
                        mask.gameObject.SetActive(false);
                    }

                    continue;
                }

                if (mask == null)
                {
                    mask = CreateMask(region.transform, renderer);
                }

                mask.GetComponent<SpriteMask>().sprite = renderer.sprite;
                mask.gameObject.SetActive(true);
                if (hoistSettings != null && region.CurrentRegion == hoistSettings.region)
                {
                    hoistRegionBounds = renderer.bounds;
                }

                if (hasBounds)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                    sample = renderer;
                }
            }

            if (!hasBounds)
            {
                Hide(background);
                Hide(hoist);
                Hide(edge);
                return;
            }

            background = Ensure(background, "AugustPlayerFlag", sample, 1);
            if (hoistSettings == null || !hoistRegionBounds.HasValue)
            {
                // Флаг растянут на прямоугольник, описанный вокруг страны
                background.sprite = sprite;
                Place(background, bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
                Hide(hoist);
                Hide(edge);
                return;
            }

            // Флаг с гербом у древка: полоса и герб в своих пропорциях закрывают часть страны от западной границы
            // опорного региона и по высоте до его верхнего края, остальная территория — цветом свободного края полотна
            Texture2D texture = sprite.texture;
            int hoistWidth = Mathf.Clamp(Mathf.RoundToInt(texture.width * hoistSettings.width), 1, texture.width);
            background.sprite = Part(texture, texture.width - 1, 1);
            Place(background, bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);

            float top = hoistRegionBounds.Value.max.y;
            float left = hoistRegionBounds.Value.min.x;
            float height = top - bounds.min.y;
            hoist = Ensure(hoist, "AugustPlayerFlagHoist", sample, 2);
            hoist.sprite = HoistPart(texture, hoistWidth);
            Place(hoist, left, left + height * hoistWidth / texture.height, bounds.min.y, top);

            // Западнее опорного региона полоса у древка продолжается до границы
            if (left > bounds.min.x)
            {
                edge = Ensure(edge, "AugustPlayerFlagEdge", sample, 2);
                edge.sprite = Part(texture, 0, 1);
                Place(edge, bounds.min.x, left, bounds.min.y, top);
            }
            else
            {
                Hide(edge);
            }
        }

        private static void Hide(SpriteRenderer renderer)
        {
            if (renderer != null)
            {
                renderer.gameObject.SetActive(false);
            }
        }

        private static SpriteRenderer Ensure(SpriteRenderer renderer, string name, SpriteRenderer sample, int order)
        {
            if (renderer == null)
            {
                var flagObject = new GameObject(name);
                flagObject.transform.SetParent(sample.transform.parent, false);
                renderer = flagObject.AddComponent<SpriteRenderer>();
                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }

            renderer.gameObject.layer = sample.gameObject.layer;
            renderer.sortingLayerID = sample.sortingLayerID;
            renderer.sortingOrder = sample.sortingOrder + order;
            renderer.color = new Color(1f, 1f, 1f, ALPHA);
            Vector3 position = renderer.transform.position;
            renderer.transform.position = new Vector3(position.x, position.y, sample.transform.position.z);
            renderer.gameObject.SetActive(true);
            return renderer;
        }

        // Растягивает спрайт на прямоугольник карты
        private static void Place(SpriteRenderer renderer, float xMin, float xMax, float yMin, float yMax)
        {
            Vector3 size = renderer.sprite.bounds.size;
            Transform transform = renderer.transform;
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.position = new Vector3((xMin + xMax) / 2f, (yMin + yMax) / 2f, transform.position.z);
            transform.localScale = new Vector3((xMax - xMin) / size.x / parentScale.x, (yMax - yMin) / size.y / parentScale.y, 1f);
        }

        private static readonly Dictionary<(Texture2D, int, int), Sprite> Parts = new Dictionary<(Texture2D, int, int), Sprite>();

        // Вертикальная полоса картинки флага во всю ее высоту
        private static Sprite Part(Texture2D texture, int x, int width)
        {
            if (!Parts.TryGetValue((texture, x, width), out Sprite part) || part == null)
            {
                part = Sprite.Create(texture, new Rect(x, 0f, width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                Parts[(texture, x, width)] = part;
            }

            return part;
        }

        private static readonly Dictionary<(Texture2D, int), Sprite> Hoists = new Dictionary<(Texture2D, int), Sprite>();

        // Часть у древка без цвета полотна: она лежит поверх фона того же цвета, и двойной полупрозрачный слой
        // был бы заметен прямоугольником
        private static Sprite HoistPart(Texture2D texture, int width)
        {
            if (Hoists.TryGetValue((texture, width), out Sprite part) && part != null)
            {
                return part;
            }

            Color field = texture.GetPixel(texture.width - 1, texture.height / 2);
            Color[] pixels = texture.GetPixels(0, 0, width, texture.height);
            for (int index = 0; index < pixels.Length; index++)
            {
                Color pixel = pixels[index];
                float distance = Mathf.Abs(pixel.r - field.r) + Mathf.Abs(pixel.g - field.g) + Mathf.Abs(pixel.b - field.b);
                if (distance < 0.12f)
                {
                    pixels[index].a = 0f;
                }
            }

            var hoistTexture = new Texture2D(width, texture.height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            hoistTexture.SetPixels(pixels);
            hoistTexture.Apply();
            part = Sprite.Create(hoistTexture, new Rect(0f, 0f, width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            Hoists[(texture, width)] = part;
            return part;
        }

        private static Transform CreateMask(Transform region, SpriteRenderer renderer)
        {
            var maskObject = new GameObject(MASK_NAME);
            maskObject.layer = region.gameObject.layer;
            maskObject.transform.SetParent(region, false);
            SpriteMask mask = maskObject.AddComponent<SpriteMask>();
            mask.sprite = renderer.sprite;
            // У спрайтов регионов полупрозрачный фон вокруг контура: он не должен попадать в маску
            mask.alphaCutoff = 0.9f;
            return maskObject.transform;
        }
    }
}

namespace AugustDaysMod.Catalog
{
    // Карту дополнительно рисует камера эффектов в текстуру без буфера трафарета, а маски спрайтов работают через него.
    // Без буфера флаг страны игрока в эффектах проступает прямоугольником за ее границами
    [HarmonyPatch(typeof(MapRenderTextureSetter), "Awake")]
    public static class PatchMapRenderTextureStencil
    {
        private static readonly AccessTools.FieldRef<MapRenderTextureSetter, RenderTexture> RtRef =
            AccessTools.FieldRefAccess<MapRenderTextureSetter, RenderTexture>("rt");

        private static readonly AccessTools.FieldRef<MapRenderTextureSetter, string> TexNameRef =
            AccessTools.FieldRefAccess<MapRenderTextureSetter, string>("texName");

        static void Postfix(MapRenderTextureSetter __instance)
        {
            RenderTexture old = RtRef(__instance);
            RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            RtRef(__instance) = rt;
            __instance.mapCamera.targetTexture = rt;
            foreach (Material material in __instance.materials)
            {
                material.SetTexture(TexNameRef(__instance), rt);
            }

            if (old != null)
            {
                RenderTexture.ReleaseTemporary(old);
            }
        }
    }
}
