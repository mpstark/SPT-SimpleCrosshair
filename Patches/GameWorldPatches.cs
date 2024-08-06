using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SimpleCrosshair.Patches
{
    internal class GameWorldRegisterPlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.RegisterPlayer));
        }

        [PatchPostfix]
        public static void PatchPostfix(IPlayer iPlayer)
        {
            if (!iPlayer.IsYourPlayer)
            {
                return;
            }

            Plugin.Instance.SimpleCrosshairComponent?.OnRegisterMainPlayer();
        }
    }

    internal class GameWorldUnregisterPlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.UnregisterPlayer));
        }

        [PatchPostfix]
        public static void PatchPostfix(IPlayer iPlayer)
        {
            if (!iPlayer.IsYourPlayer)
            {
                return;
            }

            Plugin.Instance.SimpleCrosshairComponent?.OnUnregisterMainPlayer();
            Plugin.Instance.DestroyCrosshair();
        }
    }
}
