using HarmonyLib;

[Backend.Mod.ModEntryPoint]
public static class ModEntryPoint
{
    public const uint GAME_ID = 1922740;
    public const uint MOD_ID = 3606829008;
    
    [Backend.Mod.ModEntryPoint]
    public static void Init()
    {
        var harmony = new Harmony("AugustDaysMod");
        harmony.PatchAll();
    }
}