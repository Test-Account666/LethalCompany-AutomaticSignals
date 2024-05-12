using AutomaticSignals.Checkers;
using GameNetcodeStuff;
using HarmonyLib;

namespace AutomaticSignals.Patches;

[HarmonyPatch(typeof(RadMechAI))]
public static class RadMechPatch {
    [HarmonyPatch(nameof(RadMechAI.BeginTorchPlayer))]
    [HarmonyPostfix]
    private static void BeginTorchPlayerPostfix(PlayerControllerB playerBeingTorched) {
        if (StartOfRound.Instance is null)
            return;

        if (StartOfRound.Instance.localPlayerController is null)
            return;

        if (playerBeingTorched.playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return;

        if (!Transmitter.IsSignalTranslatorUnlocked())
            return;

        GiantChecker.EatingProcessStart();
    }
}