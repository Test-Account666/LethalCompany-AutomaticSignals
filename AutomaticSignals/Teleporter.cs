using System.Linq;
using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals;

public static class Teleporter {
    internal static void TeleportPlayer(PlayerControllerB playerControllerB) {
        var teleporter = GetTeleporter(false);

        if (teleporter is null)
            return;

        StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync((int)playerControllerB.playerClientId);
        teleporter.PressTeleportButtonServerRpc();
    }

    internal static void TeleportPlayerToLocation(PlayerControllerB playerControllerB, Vector3 position) {
        var teleporter = GetTeleporter(true);

        if (teleporter is null)
            return;

        teleporter.TeleportPlayerOutWithInverseTeleporter((int)playerControllerB.playerClientId, position);
        teleporter.TeleportPlayerOutServerRpc((int)playerControllerB.playerClientId, position);
    }

    internal static Random? GetTeleporterSeed() {
        var teleporter = GetTeleporter(true);

        return teleporter == null ? null : teleporter.shipTeleporterSeed;
    }

    private static ShipTeleporter? GetTeleporter(bool inverseTeleporter) {
        var teleporterList = Object.FindObjectsOfType<ShipTeleporter>();

        return teleporterList.FirstOrDefault(teleporter => teleporter.isInverseTeleporter == inverseTeleporter);
    }
}