using BepInEx.Configuration;
using GameNetcodeStuff;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

[InitializeConfig]
public static class ShakeChecker {
    private static ConfigEntry<int> _minimumTeleportCoolDown = null!; // Default: 24000;
    private static ConfigEntry<int> _maximumTeleportCoolDown = null!; // Default: 30000;
    private static ConfigEntry<int> _rotationAccumulationThreshold = null!; // Default: 1600F;
    private static ConfigEntry<int> _teleportChance = null!; // Default: 80;
    private static Quaternion _previousRotation;
    private static float _accumulatedRotationDifference;
    private static readonly Random _Random = new();
    private static bool _initialized;
    private static PlayerControllerB? _playerControllerB;
    private static long _nextTeleport;

    public static void Initialize(ConfigFile configFile) {
        _minimumTeleportCoolDown = configFile.Bind("Panicking Player", "1. Minimum teleport cooldown", 24000,
            "Defines the minimum cooldown (in milliseconds) to wait before an emergency teleport can occur");

        _maximumTeleportCoolDown = configFile.Bind("Panicking Player", "2. Maximum teleport cooldown", 30000,
            "Defines the maximum cooldown (in milliseconds) to wait before an emergency teleport can occur");

        _rotationAccumulationThreshold = configFile.Bind("Panicking Player", "3. Rotation Accumulation Threshold", 1600,
            "Defines the accumulated rotation threshold before an emergency teleportation is being registered (Lower Number = More sensitive, Higher Number = Less Sensitive)");

        _teleportChance = configFile.Bind("Panicking Player", "4. Teleport Chance", 80,
            new ConfigDescription(
                "Defines the chance of being teleported back to the ship. If not met, will teleport player to a random position inside the facility",
                new AcceptableValueRange<int>(0, 100)));
    }

    public static void CheckForShaking(PlayerControllerB playerControllerB) {
        if (_playerControllerB is null || _playerControllerB != playerControllerB)
            _playerControllerB = playerControllerB;

        if (!_initialized)
            Start();

        Update();
    }

    private static void Start() {
        Debug.Assert(_playerControllerB != null, nameof(_playerControllerB) + " != null");

        _previousRotation = _playerControllerB.transform.rotation;

        _initialized = true;
    }

    private static void Update() {
        CalculateRotationDifference();
        CheckForTeleportation();
    }

    private static void CalculateRotationDifference() {
        Debug.Assert(_playerControllerB != null, nameof(_playerControllerB) + " != null");
        var rotation = _playerControllerB.transform.rotation;

        var rotationDifference = Quaternion.Angle(_previousRotation, rotation);
        _previousRotation = rotation;

        _accumulatedRotationDifference += rotationDifference;

        if (rotationDifference > .2F)
            return;

        _accumulatedRotationDifference = 0;
    }

    private static void CheckForTeleportation() {
        if (!(_accumulatedRotationDifference >= _rotationAccumulationThreshold.Value))
            return;

        _accumulatedRotationDifference = 0f;

        var currentTime = UnixTime.GetCurrentTime();

        if (_nextTeleport > currentTime)
            return;

        _nextTeleport = currentTime + _Random.Next(_minimumTeleportCoolDown.Value, _maximumTeleportCoolDown.Value);

        if (_Random.Next(0, 100) > _teleportChance.Value) {
            TeleportMalfunction();
            return;
        }

        Debug.Assert(_playerControllerB != null, nameof(_playerControllerB) + " != null");
        Teleporter.TeleportPlayer(_playerControllerB);
    }

    private static void TeleportMalfunction() {
        if (RoundManager.Instance.insideAINodes.Length <= 0)
            return;

        var position = GetRandomTeleportPosition();

        if (position is null)
            return;

        Debug.Assert(_playerControllerB != null, nameof(_playerControllerB) + " != null");
        Teleporter.TeleportPlayerToLocation(_playerControllerB, position.Value);
    }

    private static Vector3? GetRandomTeleportPosition() {
        var teleporterSeed = Teleporter.GetTeleporterSeed();

        if (teleporterSeed == null)
            return null;

        var position2 = RoundManager.Instance
            .insideAINodes[teleporterSeed.Next(0, RoundManager.Instance.insideAINodes.Length)]
            .transform.position;

        var inBoxPredictable =
            RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position2,
                randomSeed: teleporterSeed);

        return inBoxPredictable;
    }
}