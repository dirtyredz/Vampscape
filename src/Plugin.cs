using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Vampscape
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Moonlight Peaks.exe")]
    public sealed class VampscapePlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.dirtyredz.moonlightpeaks.vampscape";
        public const string PluginName = "Vampscape";
        // Keep in step with <Version> in the csproj - pack.ps1 names the archive from that one
        // and BepInEx reports this one. See 12-versioning-and-release.md.
        public const string PluginVersion = ModBuildInfo.Version;

        private Harmony harmony;

        private void Awake()
        {
            Plugin.Bind(Config, Logger);

            harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(ScrollGate));

            gameObject.AddComponent<BuildZoom>();

            Plugin.Log.LogInfo(
                $"{PluginName} {PluginVersion} loaded. Hold {Plugin.Modifier.Value.MainKey} and " +
                "scroll to zoom while building. Nothing is written to your save.");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
    }

    /// <summary>
    /// Config and logging, kept off the plugin type so the patches can reach them without
    /// carrying a reference to the BaseUnityPlugin around.
    /// </summary>
    internal static class Plugin
    {
        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<KeyboardShortcut> Modifier;
        internal static ConfigEntry<float> MinZoom;
        internal static ConfigEntry<float> MaxZoom;
        internal static ConfigEntry<float> ZoomStep;
        internal static ConfigEntry<bool> InvertZoom;

        internal static ConfigEntry<bool> ControllerZoom;
        internal static ConfigEntry<float> ControllerZoomSpeed;
        internal static ConfigEntry<float> ControllerDeadzone;

        // Mod Menu reads these out of ConfigDescription.Tags to title its sections. Display
        // names only - the .cfg section keys are untouched, since renaming one there would
        // orphan every saved value.
        private const string ZoomSection = "ModMenu.Section=Zooming";
        private const string ControllerSection = "ModMenu.Section=Controller";

        internal static void Bind(ConfigFile config, ManualLogSource logger)
        {
            Log = logger;

            // Descriptions are kept to one short line: Mod Menu renders them in a settings row
            // and overflows on anything longer. The reasoning behind each one lives in the mod's
            // README rather than here.

            Enabled = config.Bind(
                "Zoom", "Enabled", true,
                new ConfigDescription(
                    "Zoom the camera while building. Off leaves the wheel entirely to the game.",
                    null,
                    ZoomSection, "ModMenu.Label=Enable build-mode zoom"));

            Modifier = config.Bind(
                "Zoom", "Modifier", new KeyboardShortcut(KeyCode.LeftAlt),
                new ConfigDescription(
                    "Hold this and scroll to zoom. Released, the wheel rotates and resizes as usual.",
                    null,
                    ZoomSection, "ModMenu.Label=Hold to zoom"));

            MinZoom = config.Bind(
                "Zoom", "MinZoom", 0.6f,
                new ConfigDescription(
                    "Closest zoom, as a fraction of the game's own build-mode view.",
                    new AcceptableValueRange<float>(0.3f, 1f),
                    ZoomSection, "ModMenu.Label=Closest zoom"));

            MaxZoom = config.Bind(
                "Zoom", "MaxZoom", 2.2f,
                new ConfigDescription(
                    "Farthest zoom, as a multiple of the game's own build-mode view.",
                    new AcceptableValueRange<float>(1f, 4f),
                    ZoomSection, "ModMenu.Label=Farthest zoom"));

            ZoomStep = config.Bind(
                "Zoom", "ZoomStep", 0.12f,
                new ConfigDescription(
                    "How much one scroll tick changes the zoom.",
                    new AcceptableValueRange<float>(0.02f, 0.5f),
                    ZoomSection, "ModMenu.Label=Step per tick"));

            InvertZoom = config.Bind(
                "Zoom", "InvertZoom", false,
                new ConfigDescription(
                    "Flip the direction, so scrolling up zooms out instead of in.",
                    null,
                    ZoomSection, "ModMenu.Label=Invert direction"));

            ControllerZoom = config.Bind(
                "Controller", "ControllerZoom", true,
                new ConfigDescription(
                    "Zoom with a controller, using whatever Zoom is already bound to. No modifier needed.",
                    null,
                    ControllerSection, "ModMenu.Label=Zoom with a controller"));

            ControllerZoomSpeed = config.Bind(
                "Controller", "ControllerZoomSpeed", 1.6f,
                new ConfigDescription(
                    "How fast the controller zooms. A stick is continuous, unlike a scroll tick.",
                    new AcceptableValueRange<float>(0.2f, 6f),
                    ControllerSection, "ModMenu.Label=Controller speed"));

            ControllerDeadzone = config.Bind(
                "Controller", "ControllerDeadzone", 0.2f,
                new ConfigDescription(
                    "How far the stick or trigger must move before it zooms.",
                    new AcceptableValueRange<float>(0.05f, 0.8f),
                    ControllerSection, "ModMenu.Label=Controller deadzone"));
        }
    }
}
