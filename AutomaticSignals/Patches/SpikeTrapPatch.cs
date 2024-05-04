using System.Diagnostics.CodeAnalysis;
using AutomaticSignals.Checkers;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace AutomaticSignals.Patches;

[HarmonyPatch(typeof(SpikeRoofTrap))]
public static class SpikeTrapPatch {
    [HarmonyPatch(nameof(SpikeRoofTrap.OnTriggerStay))]
    [HarmonyPrefix]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    // ReSharper disable once SuggestBaseTypeForParameter
    private static bool OnTriggerStayPrefix(SpikeRoofTrap __instance, Collider other) {
        if (!Transmitter.IsSignalTranslatorUnlocked())
            return true;

        if (__instance && (__instance is not ({
                trapActive: true,
            } or {
                slammingDown: true,
            }) || Time.realtimeSinceStartup - __instance.timeSinceMovingUp < 0.75))
            return true;

        var player = other.gameObject.GetComponent<PlayerControllerB>();

        if (player is null)
            return true;

        var localPlayer = StartOfRound.Instance?.localPlayerController;

        if (localPlayer is null || player != localPlayer)
            return true;

        var cancel = SpikeTrapChecker.CheckSpikeTraps(__instance, player);

        if (!cancel)
            return true;

        __instance.slammingDown = false;
        return false;
    }
}