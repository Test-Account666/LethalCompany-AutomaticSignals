using AutomaticSignals.Checkers;
using GameNetcodeStuff;
using HarmonyLib;

namespace AutomaticSignals.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatch {
    [HarmonyPatch(nameof(PlayerControllerB.Update))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterUpdate(PlayerControllerB __instance) {
        if (StartOfRound.Instance is null)
            return;

        if (StartOfRound.Instance.localPlayerController is null)
            return;

        if (__instance.playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return;

        if (!Transmitter.IsSignalTranslatorUnlocked())
            return;

        EnemyChecker.CheckForEnemies(__instance);
        ShakeChecker.CheckForShaking(__instance);
        BigDoorChecker.CheckForBigDoor(__instance);
        GiantChecker.Update(__instance);
        RadMechChecker.Update(__instance);
    }
}