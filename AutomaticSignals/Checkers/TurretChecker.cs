using System;
using BepInEx.Configuration;
using GameNetcodeStuff;

namespace AutomaticSignals.Checkers;

[InitializeConfig]
public static class TurretChecker {
    private static ConfigEntry<int> _minimumTurretCoolDown = null!; // Default: 10000;
    private static ConfigEntry<int> _maximumTurretCoolDown = null!; // Default: 20000;
    private static ConfigEntry<int> _malfunctionChance = null!; // Default: 15;
    private static long _nextTurretDisable;
    private static readonly Random _Random = new();

    public static void Initialize(ConfigFile configFile) {
        _minimumTurretCoolDown = configFile.Bind("Turrets", "1. Minimum deactivate cooldown", 20000,
            "Defines the minimum cooldown (in milliseconds) to wait before deactivating a turret again");

        _maximumTurretCoolDown = configFile.Bind("Turrets", "2. Maximum deactivate cooldown", 40000,
            "Defines the maximum cooldown (in milliseconds) to wait before deactivating a turret again");

        _malfunctionChance = configFile.Bind("Turrets", "3. Malfunction Chance", 15,
            new ConfigDescription("Defines the chance for deactivating a turret to fail",
                new AcceptableValueRange<int>(0, 100)));
    }

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

        _nextTurretDisable = currentTime + _Random.Next(_minimumTurretCoolDown.Value, _maximumTurretCoolDown.Value);

        var malfunctionChance = _Random.Next(0, 100);

        if (malfunctionChance <= _malfunctionChance.Value)
            return;

        terminalAccessibleObject.CallFunctionFromTerminal();
        result = null!;
    }
}