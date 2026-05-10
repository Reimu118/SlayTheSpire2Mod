using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace MyMod;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public static void Init()
    {
        var harmony = new Harmony("MyMod");
        harmony.PatchAll();

        Log.Debug("MyMod initialized!");
    }
}