using BepInEx.Configuration;
using GameNetcodeStuff;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

[InitializeConfig]
public static class GiantChecker {
    private static ConfigEntry<int> _minimumTeleportCoolDown = null!; // Default: 10000;
    private static ConfigEntry<int> _maximumTeleportCoolDown = null!; // Default: 20000;
    private static ConfigEntry<int> _minimumTimeWaiting = null!; // Default: 1000;
    private static ConfigEntry<int> _maximumTimeWaiting = null!; // Default: 2600;
    private static ConfigEntry<int> _teleportChance = null!; // Default: 70;
    private static long _nextTeleport;
    private static readonly Random _Random = new();
    private static bool _isBeingEaten;
    private static long _teleport;

    public static void Initialize(ConfigFile configFile) {
        _minimumTeleportCoolDown = configFile.Bind("Giants", "1. Minimum teleport cooldown", 1000,
                                                   "Defines the minimum cooldown (in milliseconds) to wait before teleporting a player that is being eaten");

        _maximumTeleportCoolDown = configFile.Bind("Giants", "2. Maximum teleport cooldown", 20000,
                                                   "Defines the maximum cooldown (in milliseconds) to wait before teleporting a player that is being eaten");

        _minimumTimeWaiting = configFile.Bind("Giants", "3. Minimum teleport delay", 1000,
                                              "Defines the minimum delay (in milliseconds) to wait before actually trying to teleport a player that is being eaten");

        _maximumTimeWaiting = configFile.Bind("Giants", "4. Maximum teleport delay", 2600,
                                              "Defines the maximum delay (in milliseconds) to wait before actually trying to teleport a player that is being eaten");

        _teleportChance = configFile.Bind("Giants", "5. Teleport chance", 70,
                                          new ConfigDescription(
                                              "Defines the chance for teleporting a player that is being eaten",
                                              new AcceptableValueRange<int>(0, 100)));
    }

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

        _nextTeleport = currentTime + _Random.Next(_minimumTeleportCoolDown.Value, _maximumTeleportCoolDown.Value);

        var teleportChance = _Random.Next(1, 101);

        if (teleportChance > _teleportChance.Value)
            return;

        var timeWaiting = _Random.Next(_minimumTimeWaiting.Value, _maximumTimeWaiting.Value);

        _teleport = currentTime + timeWaiting;

        _isBeingEaten = true;
    }
}