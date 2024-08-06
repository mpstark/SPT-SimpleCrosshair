using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SimpleCrosshair.Patches
{
    internal class PlayerLookPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Look));
        }

        [PatchPostfix]
        public static void PatchPostfix(Player __instance)
        {
            if (!__instance.IsYourPlayer)
            {
                return;
            }

            Plugin.Instance.SimpleCrosshairComponent?.SetReasonToHide("looking", __instance.MouseLookControl);
        }
    }
}
