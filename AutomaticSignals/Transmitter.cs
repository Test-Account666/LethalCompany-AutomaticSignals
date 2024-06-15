using System.Linq;
using UnityEngine;
using Math = System.Math;
using Object = UnityEngine.Object;

namespace AutomaticSignals;

public static class Transmitter {
    public static bool IsSignalTranslatorUnlocked() =>
        StartOfRound.Instance.unlockablesList.unlockables
                    .Where(unlockableItem => !unlockableItem.alreadyUnlocked)
                    .Where(unlockableItem => unlockableItem.hasBeenUnlockedByPlayer)
                    .Where(unlockableItem => !unlockableItem.inStorage)
                    .Any(unlockableItem => unlockableItem.unlockableName.ToLower().Contains("translator"));

    public static void SendMessage(string message) {
        var hudManager = HUDManager.Instance;

        if (hudManager is null) {
            AutomaticSignals.Logger.LogFatal("HudManager is null?!");
            return;
        }

        var signalTranslator = Object.FindObjectOfType<SignalTranslator>();

        if (signalTranslator is null) {
            AutomaticSignals.Logger.LogFatal("SignalTranslator is null?!");
            return;
        }

        signalTranslator.timeLastUsingSignalTranslator = Time.realtimeSinceStartup;

        if (signalTranslator.signalTranslatorCoroutine is not null)
            hudManager.StopCoroutine(signalTranslator.signalTranslatorCoroutine);

        message = message[..Mathf.Min(message.Length, 10)];

        var timesSendingMessage = Math.Max(signalTranslator.timesSendingMessage + 1, 1);

        signalTranslator.timesSendingMessage = timesSendingMessage;

        var routine =
            hudManager.StartCoroutine(hudManager.DisplaySignalTranslatorMessage(message, timesSendingMessage, signalTranslator));

        signalTranslator.signalTranslatorCoroutine = routine;
    }
}