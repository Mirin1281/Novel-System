using UnityEngine;
using UnityEngine.UI;

namespace Novel
{
    public static class ImageExtension
    {
        public static void SetAlpha(this Image image, float alpha)
        {
            var c = image.color;
            image.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}