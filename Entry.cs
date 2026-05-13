using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Renko;

[ModInitializer(nameof(Init))]
public static class Entry {
    public const string ModId = "RenkoRes"; //At the moment, this is used only for the Logger and harmony names.

    public static void Init() {
        var harmony = new Harmony("Renko");
        harmony.PatchAll();

        Log.Debug("Renko initialized!");
    }
}