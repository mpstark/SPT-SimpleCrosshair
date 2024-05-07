using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

// THIS IS HEAVILY BASED ON DRAKIAXYZ'S SPT-QuickMoveToContainer
namespace SimpleCrosshair.Config
{
    internal class Settings
    {
        public static ConfigFile Config;
        public static List<ConfigEntryBase> ConfigEntries = new List<ConfigEntryBase>();

        public const string GeneralSectionTitle = "General";
        public static ConfigEntry<Color> CrosshairColor;
        public static ConfigEntry<float> CrosshairSize;
        public static ConfigEntry<Vector2> CrosshairOffset;
        public static ConfigEntry<bool> CrosshairUseDynamicPosition;
        public static ConfigEntry<float> CrosshairRaycastLength;

        public static void Init(ConfigFile Config)
        {
            Settings.Config = Config;

            ConfigEntries.Add(CrosshairColor = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Color",
                new Color(0.9f, 0.9f, 0.9f, 0.75f),
                new ConfigDescription(
                    "The color of the crosshair",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(CrosshairSize = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Size",
                30f,
                new ConfigDescription(
                    "The size of the crosshair",
                    new AcceptableValueRange<float>(15, 125),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(CrosshairOffset = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Offset",
                new Vector2(0, 0),
                new ConfigDescription(
                    "The X and Y offset of the crosshair, if you want to move the crosshair outside of the middle",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(CrosshairUseDynamicPosition = Config.Bind(
                GeneralSectionTitle,
                "Enable Dynamic Crosshair Position",
                false,
                new ConfigDescription(
                    "If the crosshair position should be dynamic to where the gun is pointing/player is looking",
                    null,
                    new ConfigurationManagerAttributes { })));
            RecalcOrder();

            ConfigEntries.Add(CrosshairRaycastLength = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Raycast Length",
                10f,
                new ConfigDescription(
                    "The length of the raycast to move position",
                    new AcceptableValueRange<float>(1, 100),
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
