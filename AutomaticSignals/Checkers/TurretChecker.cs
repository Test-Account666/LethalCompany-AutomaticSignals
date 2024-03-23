using System;
using GameNetcodeStuff;

namespace AutomaticSignals.Checkers;

public static class TurretChecker {
    private const int MINIMUM_TURRET_COOL_DOWN = 10000;
    private const int MAXIMUM_TURRET_COOL_DOWN = 20000;
    private const int MALFUNCTION_CHANCE = 5;
    private static long _nextTurretDisable;
    private static readonly Random _Random = new();

    public static void CheckTurrets(Turret turret, ref PlayerControllerB result) {
        var currentTime = UnixTime.GetCurrentTime();

        if (_nextTurretDisable > currentTime)
            return;

        if (result == null)
            return;

        if (result.playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return;

        var terminalAccessibleObject = turret.GetComponent<TerminalAccessibleObject>();

        if (terminalAccessibleObject is null) {
            AutomaticSignals.Logger.LogFatal("No TerminalAccessibleObject assigned to turret!");
            return;
        }

        _nextTurretDisable = _Random.Next(MINIMUM_TURRET_COOL_DOWN, MAXIMUM_TURRET_COOL_DOWN);

        var malfunctionChance = _Random.Next(0, 100);

        if (malfunctionChance <= MALFUNCTION_CHANCE)
            return;

        terminalAccessibleObject.CallFunctionFromTerminal();
        result = null!;
    }
}