using UnityEngine;

namespace SimpleCrosshair.Utils
{
    public static class UIUtils
    {
        public static RectTransform GetRectTransform(this GameObject gameObject)
        {
            return gameObject.transform as RectTransform;
        }

        public static RectTransform GetRectTransform(this Component component)
        {
            return component.transform as RectTransform;
        }

        public static void ResetTransform(this GameObject gameObject)
        {
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
		    gameObject.transform.localScale = Vector3.one;
        }
    }
}
