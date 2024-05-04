using System.Collections;
using BepInEx.Configuration;
using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

[InitializeConfig]
public static class SpikeTrapChecker {
    private static ConfigEntry<int> _minimumSpikeTrapCoolDown = null!; // Default: 40000;
    private static ConfigEntry<int> _maximumSpikeTrapCoolDown = null!; // Default: 80000;
    private static ConfigEntry<int> _malfunctionChance = null!; // Default: 20;
    private static long _nextSpikeTrapDisable;
    private static readonly Random _Random = new();
    private static readonly int _Slamming = Animator.StringToHash("Slamming");

    public static void Initialize(ConfigFile configFile) {
        _minimumSpikeTrapCoolDown = configFile.Bind("SpikeTraps", "1. Minimum deactivate cooldown", 40000,
                                                    "Defines the minimum cooldown (in milliseconds) to wait before deactivating a spike trap again");

        _maximumSpikeTrapCoolDown = configFile.Bind("SpikeTraps", "2. Maximum deactivate cooldown", 80000,
                                                    "Defines the maximum cooldown (in milliseconds) to wait before deactivating a spike trap again");

        _malfunctionChance = configFile.Bind("SpikeTraps", "3. Malfunction Chance", 20,
                                             new ConfigDescription("Defines the chance for deactivating a spike trap to fail",
                                                                   new AcceptableValueRange<int>(0, 100)));
    }

    public static bool CheckSpikeTraps(SpikeRoofTrap spikeRoofTrap, PlayerControllerB result) {
        var currentTime = UnixTime.GetCurrentTime();

        if (_nextSpikeTrapDisable > currentTime)
            return false;

        if (result == null)
            return false;

        if (result.playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return false;

        // The reason why we want the parent's parent is the fact that the `TerminalAccessibleObject` isn't attached to the SpikeRoofTrap GameObject, since it is only a Trigger.
        var spikeRoofTrapHazard = spikeRoofTrap.transform.parent.parent;

        var terminalAccessibleObject = spikeRoofTrapHazard.Find("TerminalObjectContainer")
                                                          .GetComponent<TerminalAccessibleObject>();

        if (terminalAccessibleObject is null) {
            AutomaticSignals.Logger.LogFatal("No TerminalAccessibleObject assigned to spikeRoofTrap!");
            return false;
        }

        _nextSpikeTrapDisable = currentTime + _Random.Next(_minimumSpikeTrapCoolDown.Value, _maximumSpikeTrapCoolDown.Value);

        var malfunctionChance = _Random.Next(1, 101);

        if (malfunctionChance <= _malfunctionChance.Value)
            return false;

        terminalAccessibleObject.CallFunctionFromTerminal();

        Transmitter.SendMessage("Evil Spike");

        if (spikeRoofTrap.slamCoroutine is null)
            return true;

        spikeRoofTrap.StopCoroutine(spikeRoofTrap.slamCoroutine);

        spikeRoofTrap.spikeTrapAnimator.SetBool(_Slamming, false);
        return true;
    }
}