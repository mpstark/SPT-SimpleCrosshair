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
        public static ConfigEntry<Color> Color;
        public static ConfigEntry<float> Size;
        public static ConfigEntry<Vector2> Offset;
        public static ConfigEntry<float> FadeInOutTime;
        public static ConfigEntry<bool> UseDynamicPosition;
        public static ConfigEntry<float> DynamicPositionAimDistance;
        public static ConfigEntry<float> DynamicPositionSmoothTime;

        public static void Init(ConfigFile Config)
        {
            Settings.Config = Config;

            ConfigEntries.Add(Color = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Color",
                new Color(0.9f, 0.9f, 0.9f, 0.75f),
                new ConfigDescription(
                    "The color of the crosshair",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(Size = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Size",
                30f,
                new ConfigDescription(
                    "The size of the crosshair",
                    new AcceptableValueRange<float>(15, 125),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(Offset = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Offset",
                new Vector2(0, 0),
                new ConfigDescription(
                    "The X and Y offset of the crosshair, if you want to move the crosshair outside of the middle",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(FadeInOutTime = Config.Bind(
                GeneralSectionTitle,
                "Crosshair Fade Time",
                0.10f,
                new ConfigDescription(
                    "How fast should the crosshair fade in and out on show/hide, 0 for instant",
                    new AcceptableValueRange<float>(0f, 0.5f),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(UseDynamicPosition = Config.Bind(
                GeneralSectionTitle,
                "Enable Dynamic Crosshair Position",
                false,
                new ConfigDescription(
                    "If the crosshair position should be dynamic to where the gun is pointing/player is looking",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(DynamicPositionAimDistance = Config.Bind(
                GeneralSectionTitle,
                "Dynamic Crosshair Aim Distance",
                15f,
                new ConfigDescription(
                    "How far away from the muzzle that obstacles should be found",
                    new AcceptableValueRange<float>(1f, 50f),
                    new ConfigurationManagerAttributes { IsAdvanced = true })));

            ConfigEntries.Add(DynamicPositionSmoothTime = Config.Bind(
                GeneralSectionTitle,
                "Dynamic Crosshair Smooth Time",
                0.10f,
                new ConfigDescription(
                    "How fast should crosshair react to changes, set to 0 for no smoothing",
                    new AcceptableValueRange<float>(0f, 0.5f),
                    new ConfigurationManagerAttributes { IsAdvanced = true })));

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
