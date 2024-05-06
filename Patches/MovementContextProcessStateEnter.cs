using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;

namespace SimpleCrosshair.Patches
{
    internal class MovementContextProcessStateEnterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.ProcessStateEnter));
        }

        [PatchPostfix]
        public static void PatchPostfix(MovementContext __instance)
        {
            if (__instance != GameUtils.Player.MovementContext)
            {
                return;
            }

            Plugin.Instance.SimpleCrosshair?.OnMovementStateChanged(__instance.CurrentState.Name);
        }
    }
}
