using HarmonyLib;

[Backend.Mod.ModEntryPoint]
public static class ModEntryPoint
{
    // Должен совпадать с id в assets/info.json
    public const string MOD_INFO_ID = "AugustDays1991Mod";

    [Backend.Mod.ModEntryPoint]
    public static void Init()
    {
        var harmony = new Harmony("AugustDaysMod");
        harmony.PatchAll();
    }
}
