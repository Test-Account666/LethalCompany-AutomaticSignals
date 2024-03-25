using AutomaticSignals.Checkers;
using GameNetcodeStuff;
using HarmonyLib;

namespace AutomaticSignals.Patches;

[HarmonyPatch(typeof(ForestGiantAI))]
public class ForestGiantPatch {
    [HarmonyPatch("BeginEatPlayer")]
    [HarmonyPostfix]
    public static void AfterBeginEatPlayer(PlayerControllerB playerBeingEaten) {
        if (StartOfRound.Instance is null)
            return;

        if (StartOfRound.Instance.localPlayerController is null)
            return;

        if (playerBeingEaten.actualClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return;
        
        if (!Transmitter.IsSignalTranslatorUnlocked())
            return;

        GiantChecker.EatingProcessStart();
    }
}