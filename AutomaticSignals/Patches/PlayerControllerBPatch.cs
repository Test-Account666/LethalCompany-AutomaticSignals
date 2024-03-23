using AutomaticSignals.Checkers;
using GameNetcodeStuff;
using HarmonyLib;

namespace AutomaticSignals.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatch {
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    public static void AfterUpdate(PlayerControllerB __instance) {
        EnemyChecker.CheckForEnemies(__instance);
        ShakeChecker.CheckForShaking(__instance);
        BigDoorChecker.CheckForBigDoor(__instance);
    }
}