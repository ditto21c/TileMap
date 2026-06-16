using UnityEngine;

namespace TileMap.Utility
{
    public static class NoiseUtility
    {
        public static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 362437);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFFu) / 16777215f;
            }
        }

        public static float FractalNoise01(float x, float y, int seed, int octaves = 4, float frequency = 0.004f, float persistence = 0.5f)
        {
            float value = 0f;
            float amplitude = 1f;
            float amplitudeSum = 0f;
            float currentFrequency = frequency;

            for (int i = 0; i < octaves; i++)
            {
                float sample = Mathf.PerlinNoise(
                    x * currentFrequency + seed * 0.0137f,
                    y * currentFrequency + seed * 0.0219f);

                value += sample * amplitude;
                amplitudeSum += amplitude;
                amplitude *= persistence;
                currentFrequency *= 2f;
            }

            return amplitudeSum > 0f ? value / amplitudeSum : 0f;
        }

        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
