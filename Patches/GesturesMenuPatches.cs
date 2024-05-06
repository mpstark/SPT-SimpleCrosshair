using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI.Gestures;
using HarmonyLib;

namespace SimpleCrosshair.Patches
{
    internal class GesturesMenuShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GesturesMenu), nameof(GesturesMenu.Show));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            Plugin.Instance.SimpleCrosshair?.SetReasonToHide("gestureMenuOpen", true);
        }
    }

    internal class GesturesMenuClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GesturesMenu), nameof(GesturesMenu.Close));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            Plugin.Instance.SimpleCrosshair?.SetReasonToHide("gestureMenuOpen", false);
        }
    }
}
