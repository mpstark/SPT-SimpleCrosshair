using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT.UI;
using SimpleCrosshair.Config;
using SimpleCrosshair.Patches;
using UnityEngine;

namespace SimpleCrosshair
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.SimpleCrosshair", "SimpleCrosshair", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;
        public static string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public GameObject SimpleCrosshairGO { get; private set; }
        public SimpleCrosshairComponent SimpleCrosshairComponent { get; private set; }

        internal void Awake()
        {
            Settings.Init(Config);
            Config.SettingChanged += (x, y) => SimpleCrosshairComponent?.ReadConfig();

            Instance = this;
            DontDestroyOnLoad(this);

            // patches
            new BattleUIScreenShowPatch().Enable();
            new PlayerLookPatch().Enable();
            new GameWorldRegisterPlayerPatch().Enable();
            new GameWorldUnregisterPlayerPatch().Enable();
            new GesturesMenuShowPatch().Enable();
            new GesturesMenuClosePatch().Enable();
            new MovementContextProcessStateEnterPatch().Enable();
        }

        /// <summary>
        /// Attach the crosshair to the battle ui screen
        /// </summary>
        internal void TryAttachToBattleUIScreen(EftBattleUIScreen screen)
        {
            // if the screen already has the component, don't add it
            if (screen.GetComponentInChildren<SimpleCrosshairComponent>() != null)
            {
                return;
            }

            if (SimpleCrosshairGO != null)
            {
                Log.LogWarning($"Attaching new SimpleCrosshairComponent, but one already existed? Maybe old reference.");
                DestroyCrosshair();
            }

            SimpleCrosshairGO = SimpleCrosshairComponent.AttachToBattleUIScreen(screen);
            SimpleCrosshairComponent = SimpleCrosshairGO.GetComponent<SimpleCrosshairComponent>();
        }

        /// <summary>
        /// This destroys the current crosshair
        /// </summary>
        internal void DestroyCrosshair()
        {
            Destroy(SimpleCrosshairGO);

            SimpleCrosshairGO = null;
            SimpleCrosshairComponent = null;
        }
    }
}
