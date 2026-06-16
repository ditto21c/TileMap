using System.Collections.Generic;
using UnityEngine;

namespace TileMap.Utility
{
    public static class RuntimeSpriteFactory
    {
        private static Sprite whiteSprite;
        private static readonly Dictionary<Color32, Sprite> spriteCache = new Dictionary<Color32, Sprite>();

        public static Sprite GetSolidSprite(Color32 color)
        {
            if (spriteCache.TryGetValue(color, out var cached))
            {
                return cached;
            }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"RuntimeSprite_{color.r}_{color.g}_{color.b}_{color.a}"
            };

            var pixels = new Color32[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = texture.name;
            spriteCache[color] = sprite;
            return sprite;
        }

        public static Sprite GetWhiteSprite()
        {
            if (whiteSprite == null)
            {
                whiteSprite = GetSolidSprite(new Color32(255, 255, 255, 255));
            }

            return whiteSprite;
        }
    }
}
