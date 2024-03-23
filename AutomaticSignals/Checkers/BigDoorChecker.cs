using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

public static class BigDoorChecker {
    private const int MINIMUM_DOOR_COOL_DOWN = 15000;
    private const int MAXIMUM_DOOR_COOL_DOWN = 23000;
    private const int OPEN_CHANCE = 30;
    private const int MALFUNCTION_CHANCE = 15;
    private static long _nextDoorOpen;
    private static readonly Random _Random = new();

    public static void CheckForBigDoor(PlayerControllerB playerControllerB) {
        var ray = playerControllerB.interactRay;
        var maxDistance = playerControllerB.grabDistance * 2;

        var accessibleObject = GetBigDoor(ray, maxDistance);

        if (accessibleObject is null)
            return;

        var currentTime = UnixTime.GetCurrentTime();

        if (currentTime < _nextDoorOpen)
            return;

        _nextDoorOpen = currentTime + _Random.Next(MINIMUM_DOOR_COOL_DOWN, MAXIMUM_DOOR_COOL_DOWN);

        OpenOrMalfunctionDoor(accessibleObject, accessibleObject.isDoorOpen ? MALFUNCTION_CHANCE : OPEN_CHANCE);
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