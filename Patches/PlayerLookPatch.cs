using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;

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

            Plugin.Instance.SimpleCrosshair?.SetReasonToHide("looking", __instance.MouseLookControl);
        }
    }
}
