using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using SimpleCrosshair.Config;

namespace SimpleCrosshair
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.SimpleCrosshair", "SimpleCrosshair", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;
        public static string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal void Awake()
        {
            Settings.Init(Config);
            // Config.SettingChanged += (x, y) => PlayerEncumbranceBar.OnSettingChanged();

            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
