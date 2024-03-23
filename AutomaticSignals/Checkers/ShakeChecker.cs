using System;
using GameNetcodeStuff;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;
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
        if (StartOfRound.Instance is null)
            return;

        if (StartOfRound.Instance.localPlayerController is null)
            return;

        if (playerControllerB.actualClientId != StartOfRound.Instance.localPlayerController.playerClientId)
            return;

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

        TeleportPlayer();
    }

    private static void TeleportMalfunction() {
        if (RoundManager.Instance.insideAINodes.Length <= 0)
            return;

        var teleporterList = Object.FindObjectsOfType<ShipTeleporter>();

        foreach (var teleporter in teleporterList) {
            if (!teleporter.isInverseTeleporter)
                continue;

            var position2 = RoundManager.Instance
                .insideAINodes[teleporter.shipTeleporterSeed.Next(0, RoundManager.Instance.insideAINodes.Length)]
                .transform.position;

            var inBoxPredictable =
                RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position2,
                    randomSeed: teleporter.shipTeleporterSeed);

            Debug.Assert(_playerControllerB != null, nameof(_playerControllerB) + " != null");
            teleporter.TeleportPlayerOutWithInverseTeleporter((int)_playerControllerB.playerClientId, inBoxPredictable);
            teleporter.TeleportPlayerOutServerRpc((int)_playerControllerB.playerClientId, inBoxPredictable);
        }
    }

    private static void TeleportPlayer() {
        var teleporterList = Object.FindObjectsOfType<ShipTeleporter>();

        foreach (var teleporter in teleporterList) {
            if (teleporter.isInverseTeleporter)
                continue;

            Debug.Assert(_playerControllerB != null, nameof(_playerControllerB) + " != null");
            StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync((int)_playerControllerB.playerClientId);
            teleporter.PressTeleportButtonServerRpc();
            return;
        }
    }
}