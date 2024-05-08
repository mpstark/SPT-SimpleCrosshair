using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

// THIS IS HEAVILY BASED ON DRAKIAXYZ'S SPT-QuickMoveToContainer
namespace SimpleCrosshair.Config
{
    internal class Settings
    {
        public static ConfigFile Config;
        public static List<ConfigEntryBase> ConfigEntries = new List<ConfigEntryBase>();

        public const string GeneralTitle = "1. General";
        public static ConfigEntry<string> ImageFileName;
        public static ConfigEntry<bool> Show;
        public static ConfigEntry<Color> Color;
        public static ConfigEntry<float> Size;
        public static ConfigEntry<float> FadeInOutTime;
        public static ConfigEntry<float> OffsetX;
        public static ConfigEntry<float> OffsetY;

        public const string DynamicPositionTitle = "2. Dynamic Positioning";
        public static ConfigEntry<bool> UseDynamicPosition;
        public static ConfigEntry<float> DynamicPositionAimDistance;
        public static ConfigEntry<float> DynamicPositionSmoothTime;

        public const string KeybindTitle = "3. Keybinds";
        public static ConfigEntry<KeyboardShortcut> KeyboardShortcut;
        public static ConfigEntry<EKeybindBehavior> KeybindBehavior;

        public static void Init(ConfigFile Config)
        {
            Settings.Config = Config;

            // get all png images in directory
            var customFiles = Directory.EnumerateFiles(Plugin.Path, "*.*", SearchOption.AllDirectories)
                .Where(p => "png" == Path.GetExtension(p).TrimStart('.').ToLowerInvariant())
                .Select(p => Path.GetFileName(p))
                .ToArray();

            ConfigEntries.Add(ImageFileName = Config.Bind(
                GeneralTitle,
                "Crosshair Image",
                "crosshair.png",
                new ConfigDescription(
                    "The file name of a custom crosshair located in \\BepInEx\\plugins\\",
                    new AcceptableValueList<string>(customFiles),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(Show = Config.Bind(
                GeneralTitle,
                "Show Crosshair",
                true,
                new ConfigDescription(
                    "If the crosshair should be displayed on raid load, can still be toggled on by Toggle keybind",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(Color = Config.Bind(
                GeneralTitle,
                "Crosshair Color",
                new Color(0.9f, 0.9f, 0.9f, 0.75f),
                new ConfigDescription(
                    "The color of the crosshair",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(Size = Config.Bind(
                GeneralTitle,
                "Crosshair Size",
                30f,
                new ConfigDescription(
                    "The size of the crosshair",
                    new AcceptableValueRange<float>(15, 125),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(FadeInOutTime = Config.Bind(
                GeneralTitle,
                "Crosshair Fade Time",
                0.10f,
                new ConfigDescription(
                    "How fast should the crosshair fade in and out on show/hide, 0 for instant",
                    new AcceptableValueRange<float>(0f, 0.5f),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(OffsetX = Config.Bind(
                GeneralTitle,
                "Crosshair X Offset",
                0f,
                new ConfigDescription(
                    "The X offset of the crosshair, if you want to move the crosshair outside of the middle. - left right +",
                    new AcceptableValueRange<float>(-100, 100),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(OffsetY = Config.Bind(
                GeneralTitle,
                "Crosshair Y Offset",
                0f,
                new ConfigDescription(
                    "The Y offset of the crosshair, if you want to move the crosshair outside of the middle. - down up +",
                    new AcceptableValueRange<float>(-100, 100),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(UseDynamicPosition = Config.Bind(
                DynamicPositionTitle,
                "Enable Dynamic Crosshair Position",
                false,
                new ConfigDescription(
                    "If the crosshair position should be dynamic to where the gun is pointing/player is looking",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(DynamicPositionAimDistance = Config.Bind(
                DynamicPositionTitle,
                "Dynamic Crosshair Aim Distance",
                15f,
                new ConfigDescription(
                    "How far away from the muzzle that obstacles should be found",
                    new AcceptableValueRange<float>(1f, 50f),
                    new ConfigurationManagerAttributes { IsAdvanced = true })));

            ConfigEntries.Add(DynamicPositionSmoothTime = Config.Bind(
                DynamicPositionTitle,
                "Dynamic Crosshair Smooth Time",
                0.10f,
                new ConfigDescription(
                    "How fast should crosshair react to changes, set to 0 for no smoothing",
                    new AcceptableValueRange<float>(0f, 0.5f),
                    new ConfigurationManagerAttributes { IsAdvanced = true })));

            ConfigEntries.Add(KeyboardShortcut = Config.Bind(
                KeybindTitle,
                "Keyboard Shortcut",
                new KeyboardShortcut(KeyCode.None),
                new ConfigDescription(
                    "The keyboard shortcut to use for the following options",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(KeybindBehavior = Config.Bind(
                KeybindTitle,
                "Shortcut Behavior",
                EKeybindBehavior.PressToggles,
                new ConfigDescription(
                    "What the keybind should do when pressed",
                    null,
                    new ConfigurationManagerAttributes { })));

            RecalcOrder();
        }

        private static void RecalcOrder()
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            int settingOrder = ConfigEntries.Count;
            foreach (var entry in ConfigEntries)
            {
                var attributes = entry.Description.Tags[0] as ConfigurationManagerAttributes;
                if (attributes != null)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
    }
}
