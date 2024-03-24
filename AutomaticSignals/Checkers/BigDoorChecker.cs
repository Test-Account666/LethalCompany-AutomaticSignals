using BepInEx.Configuration;
using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

[InitializeConfig]
public static class BigDoorChecker {
    private static ConfigEntry<int> _minimumDoorCoolDown = null!; // Default:  15000;
    private static ConfigEntry<int> _maximumDoorCoolDown = null!; // Default:  23000;
    private static ConfigEntry<int> _openChance = null!; // Default:  40;
    private static ConfigEntry<int> _malfunctionChance = null!; // Default: 15;
    private static long _nextDoorOpen;

    private static readonly Random _Random = new();

    public static void Initialize(ConfigFile configFile) {
        _minimumDoorCoolDown = configFile.Bind("Blast Doors", "1. Minimum cooldown", 15000,
            "Defines the minimum cooldown (in milliseconds) to wait before opening/closing a blast door");

        _maximumDoorCoolDown = configFile.Bind("Blast Doors", "2. Maximum cooldown", 23000,
            "Defines the maximum cooldown (in milliseconds) to wait before opening/closing a blast door");

        _openChance = configFile.Bind("Blast Doors", "3. Open chance", 40,
            new ConfigDescription("Defines the chance a blast door will open, if looked at",
                new AcceptableValueRange<int>(0, 100)));

        _malfunctionChance = configFile.Bind("Blast Doors", "4. Malfunction chance", 15,
            new ConfigDescription("Defines the chance a blast door will close, if looked at",
                new AcceptableValueRange<int>(0, 100)));
    }

    public static void CheckForBigDoor(PlayerControllerB playerControllerB) {
        var ray = playerControllerB.interactRay;
        var maxDistance = playerControllerB.grabDistance * 2;

        var accessibleObject = GetBigDoor(ray, maxDistance);

        if (accessibleObject is null)
            return;

        var currentTime = UnixTime.GetCurrentTime();

        if (currentTime < _nextDoorOpen)
            return;

        _nextDoorOpen = currentTime + _Random.Next(_minimumDoorCoolDown.Value, _maximumDoorCoolDown.Value);

        OpenOrMalfunctionDoor(accessibleObject,
            accessibleObject.isDoorOpen ? _malfunctionChance.Value : _openChance.Value);
    }

    private static TerminalAccessibleObject? GetBigDoor(Ray ray, float maxDistance) {
        var hit = Physics.Raycast(ray, out var raycastHit, maxDistance);

        if (!hit)
            return null;

        if (!raycastHit.collider.gameObject.name.ToLower().Contains("bigdoor"))
            return null;

        var accessibleObject = raycastHit.collider.gameObject.GetComponent<TerminalAccessibleObject>();

        return accessibleObject;
    }

    private static void OpenOrMalfunctionDoor(TerminalAccessibleObject accessibleObject, int chance) {
        var generatedChance = _Random.Next(0, 100);

        if (generatedChance > chance)
            return;

        accessibleObject.CallFunctionFromTerminal();
    }
}