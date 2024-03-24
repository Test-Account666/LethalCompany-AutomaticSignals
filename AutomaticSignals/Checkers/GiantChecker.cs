using GameNetcodeStuff;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

public static class GiantChecker {
    private const int MINIMUM_TELEPORT_COOL_DOWN = 10000;
    private const int MAXIMUM_TELEPORT_COOL_DOWN = 20000;
    private const int MINIMUM_TIME_WAITING = 1000;
    private const int MAXIMUM_TIME_WAITING = 2600;
    private const int TELEPORT_CHANCE = 70;
    private static long _nextTeleport;
    private static readonly Random _Random = new();
    private static bool _isBeingEaten;
    private static long _teleport;

    public static void Update(PlayerControllerB playerControllerB) {
        if (!_isBeingEaten)
            return;

        var currentTime = UnixTime.GetCurrentTime();

        if (_teleport > currentTime)
            return;

        Teleporter.TeleportPlayer(playerControllerB);
        _isBeingEaten = false;
    }

    public static void EatingProcessStart() {
        var currentTime = UnixTime.GetCurrentTime();

        if (_nextTeleport > currentTime)
            return;

        _nextTeleport = currentTime + _Random.Next(MINIMUM_TELEPORT_COOL_DOWN, MAXIMUM_TELEPORT_COOL_DOWN);

        var teleportChance = _Random.Next(0, 100);

        if (teleportChance > TELEPORT_CHANCE)
            return;

        var timeWaiting = _Random.Next(MINIMUM_TIME_WAITING, MAXIMUM_TIME_WAITING);

        _teleport = currentTime + timeWaiting;

        _isBeingEaten = true;
    }
}