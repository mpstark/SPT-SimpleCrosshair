using Comfort.Common;
using EFT;

namespace SimpleCrosshair
{
    public static class GameUtils
    {
        public static Player Player => Singleton<GameWorld>.Instance.MainPlayer;
    }
}