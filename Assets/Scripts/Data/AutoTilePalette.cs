using System.Collections.Generic;
using UnityEngine;

namespace TileMap.Data
{
    [CreateAssetMenu(menuName = "Tools/World/Auto Tile Palette", fileName = "AutoTilePalette")]
    public class AutoTilePalette : ScriptableObject
    {
        [SerializeField] private AutoTileSet[] tileSets = new AutoTileSet[0];

        private readonly Dictionary<byte, AutoTileSet> cache = new Dictionary<byte, AutoTileSet>(256);
        private bool cacheReady;

        public int TileSetCount => tileSets != null ? tileSets.Length : 0;
        public bool IsEmpty => TileSetCount == 0;

        public bool TryGetSet(byte tileId, out AutoTileSet tileSet)
        {
            if (!cacheReady)
            {
                RebuildCache();
            }

            return cache.TryGetValue(tileId, out tileSet);
        }

        public void RebuildCache()
        {
            cache.Clear();
            for (int i = 0; i < tileSets.Length; i++)
            {
                AutoTileSet tileSet = tileSets[i];
                if (tileSet == null)
                {
                    continue;
                }

                cache[(byte)tileSet.TileId] = tileSet;
            }

            cacheReady = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildCache();
        }
#endif
    }
}
