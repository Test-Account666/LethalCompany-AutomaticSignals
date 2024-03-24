using GameNetcodeStuff;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

public static class ShakeChecker {
    private const int TELEPORT_COOL_DOWN = 30000;
    private const float ROTATION_ACCUMULATION_THRESHOLD = 1600F;
    private const int TELEPORT_CHANCE = 80;
    private static Quaternion _previousRotation;
    private static float _accumulatedRotationDifference;
    private static readonly Random _Random = new();
    private static bool _initialized;
    private static PlayerControllerB? _playerControllerB;
    private static long _nextTeleport;

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
        if (!(_accumulatedRotationDifference >= ROTATION_ACCUMULATION_THRESHOLD))
            return;

        _accumulatedRotationDifference = 0f;

        var currentTime = UnixTime.GetCurrentTime();

        if (_nextTeleport > currentTime)
            return;

        _nextTeleport = currentTime + TELEPORT_COOL_DOWN;

        if (_Random.Next(0, 100) > TELEPORT_CHANCE) {
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