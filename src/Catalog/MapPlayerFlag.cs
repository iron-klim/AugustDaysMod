using System.Collections.Generic;
using Backend.Gamedesign;
using Backend.Managers;
using Frontend.Map;
using HarmonyLib;
using UnityEngine;

namespace AugustDaysMod.Catalog
{
    // На политической карте территория страны игрока закрашена ее флагом: один флаг растянут на всю страну,
    // а маски по форме ее регионов обрезают его по границам
    [HarmonyPatch(typeof(MapController), "RepaintRegions")]
    public static class PatchMapPlayerFlag
    {
        private const string MASK_NAME = "AugustPlayerFlagMask";
        private const float ALPHA = 0.85f;

        private static readonly AccessTools.FieldRef<MapController, List<RegionController>> RegionsRef =
            AccessTools.FieldRefAccess<MapController, List<RegionController>>("regions");

        private static readonly AccessTools.FieldRef<int> MapModeRef = AccessTools.StaticFieldRefAccess<int>(AccessTools.Field(typeof(MapController), "currentMapMode"));

        private static SpriteRenderer flag;

        static void Postfix(MapController __instance)
        {
            GameState gameState = GameStateManager.inst?.GameState;
            List<RegionController> regions = RegionsRef(__instance);
            if (gameState == null || regions == null || regions.Count == 0)
            {
                return;
            }

            Sprite sprite = gameState.playerCountry != Countries.USSR && MapModeRef() == 0 ? CountryFlags.Current(gameState) : null;
            bool hasBounds = false;
            Bounds bounds = default;
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
                if (flag != null)
                {
                    flag.gameObject.SetActive(false);
                }

                return;
            }

            if (flag == null)
            {
                var flagObject = new GameObject("AugustPlayerFlag");
                flagObject.transform.SetParent(sample.transform.parent, false);
                flag = flagObject.AddComponent<SpriteRenderer>();
                flag.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }

            flag.gameObject.layer = sample.gameObject.layer;
            flag.sortingLayerID = sample.sortingLayerID;
            flag.sortingOrder = sample.sortingOrder + 1;
            flag.sprite = sprite;
            flag.color = new Color(1f, 1f, 1f, ALPHA);
            flag.gameObject.SetActive(true);

            // Флаг растягиваем на прямоугольник, описанный вокруг страны
            Vector3 size = sprite.bounds.size;
            Transform transform = flag.transform;
            transform.position = new Vector3(bounds.center.x, bounds.center.y, sample.transform.position.z);
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(bounds.size.x / size.x / parentScale.x, bounds.size.y / size.y / parentScale.y, 1f);
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
