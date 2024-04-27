using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AutomaticSignals;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class AutomaticSignals : BaseUnityPlugin {
    public static AutomaticSignals Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        InitializeConfig();

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

        Logger.LogInfo("Please don't forget, that this mod still needs some polishing!");
    }

    internal static void Patch() {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }

    public void InitializeConfig() {
        var types = Assembly.GetExecutingAssembly().GetTypes();

        var markedTypes = types.Where(Predicate);

        foreach (var type in markedTypes) {
            var initializeMethod = type.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);

            initializeMethod?.Invoke(null, [
                Config,
            ]);
        }
    }

    private static bool Predicate(Type type) =>
        type.GetCustomAttributes(typeof(InitializeConfigAttribute), false).Length > 0;
}