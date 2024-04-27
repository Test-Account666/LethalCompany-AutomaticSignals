using System.Collections;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals;

public static class Teleporter {
    internal static void TeleportPlayer(PlayerControllerB playerControllerB) {
        var teleporter = GetTeleporter(false);

        // ReSharper disable once UseNullPropagation
        if (teleporter is null)
            return;

        teleporter.StartCoroutine(BeamUpPlayer(playerControllerB, teleporter));
    }

    internal static void TeleportPlayerToLocation(PlayerControllerB playerControllerB, Vector3 position) {
        var teleporter = GetTeleporter(true);

        if (teleporter is null)
            return;

        teleporter.TeleportPlayerOutWithInverseTeleporter((int) playerControllerB.playerClientId, position);
        teleporter.TeleportPlayerOutServerRpc((int) playerControllerB.playerClientId, position);
    }

    internal static Random? GetTeleporterSeed() {
        var teleporter = GetTeleporter(true);

        return teleporter == null? null : teleporter.shipTeleporterSeed;
    }

    private static ShipTeleporter? GetTeleporter(bool inverseTeleporter) {
        var teleporterList = Object.FindObjectsOfType<ShipTeleporter>();

        return teleporterList.FirstOrDefault(teleporter => teleporter.isInverseTeleporter == inverseTeleporter);
    }


    // Blatantly stolen from Zeekerss... Have to keep this until I find a better way
    private static IEnumerator BeamUpPlayer(PlayerControllerB? playerToTeleport, ShipTeleporter? teleporter) {
        if (teleporter is null)
            yield break;

        teleporter.shipTeleporterAudio.PlayOneShot(teleporter.teleporterSpinSFX);

        if (playerToTeleport is null)
            yield break;

        if (playerToTeleport.deadBody is not null)
            yield break;

        teleporter.SetPlayerTeleporterId(playerToTeleport, 1);

        playerToTeleport.beamUpParticle.Play();
        playerToTeleport.movementAudio.PlayOneShot(teleporter.beamUpPlayerBodySFX);

        yield return new WaitForSeconds(3f);

        if (playerToTeleport.deadBody is not null)
            yield break;

        playerToTeleport.DropAllHeldItems();

        var audioReverbPresets = Object.FindObjectOfType<AudioReverbPresets>();

        // ReSharper disable once UseNullPropagation
        if (audioReverbPresets is not null)
            audioReverbPresets.audioPresets[3].ChangeAudioReverbForPlayer(playerToTeleport);

        playerToTeleport.isInElevator = true;
        playerToTeleport.isInHangarShipRoom = true;
        playerToTeleport.isInsideFactory = false;
        playerToTeleport.averageVelocity = 0.0f;
        playerToTeleport.velocityLastFrame = Vector3.zero;

        playerToTeleport.TeleportPlayer(teleporter.teleporterPosition.position, true, 160f);

        teleporter.SetPlayerTeleporterId(playerToTeleport, -1);

        teleporter.shipTeleporterAudio.PlayOneShot(teleporter.teleporterBeamUpSFX);

        if (!GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
            yield break;

        HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
    }
}