using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;

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

            Plugin.Instance.SimpleCrosshair?.OnRegisterMainPlayer();
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

            Plugin.Instance.SimpleCrosshair?.OnUnregisterMainPlayer();
            Plugin.Instance.DestroyCrosshair();
        }
    }
}
