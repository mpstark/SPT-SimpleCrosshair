using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT.UI;
using SimpleCrosshair.Config;
using SimpleCrosshair.Patches;

namespace SimpleCrosshair
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.SimpleCrosshair", "SimpleCrosshair", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;
        public static string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public SimpleCrosshairComponent SimpleCrosshair { get; private set; }

        internal void Awake()
        {
            Settings.Init(Config);
            Config.SettingChanged += (x, y) => SimpleCrosshair?.ReadConfig();

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
        public void TryAttachToBattleUIScreen(BattleUIScreen screen)
        {
            // if the screen already has the component, don't add it
            if (screen.GetComponentInChildren<SimpleCrosshairComponent>() != null)
            {
                return;
            }

            if (SimpleCrosshair != null)
            {
                Log.LogWarning($"Attaching new SimpleCrosshairComponent, but one already existed? Maybe old reference.");
                SimpleCrosshair.OnUnregisterMainPlayer();
                Destroy(SimpleCrosshair);
                SimpleCrosshair = null;
            }

            SimpleCrosshair = SimpleCrosshairComponent.AttachToBattleUIScreen(screen);
        }
    }
}
