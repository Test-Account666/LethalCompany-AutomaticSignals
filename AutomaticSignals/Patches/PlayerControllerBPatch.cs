using AutomaticSignals.Checkers;
using GameNetcodeStuff;
using HarmonyLib;

namespace AutomaticSignals.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatch {
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    public static void AfterUpdate(PlayerControllerB __instance) {
        if (StartOfRound.Instance is null)
            return;

        if (StartOfRound.Instance.localPlayerController is null)
            return;

        if (__instance.playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return;

        EnemyChecker.CheckForEnemies(__instance);
        ShakeChecker.CheckForShaking(__instance);
        BigDoorChecker.CheckForBigDoor(__instance);
        GiantChecker.Update(__instance);
    }
}