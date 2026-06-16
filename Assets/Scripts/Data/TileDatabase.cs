using System.Collections.Generic;
using UnityEngine;

namespace TileMap.Data
{
    [CreateAssetMenu(menuName = "Tools/World/Tile Database", fileName = "TileDatabase")]
    public class TileDatabase : ScriptableObject
    {
        [SerializeField] private List<TileDefinition> definitions = new List<TileDefinition>();
        private readonly Dictionary<byte, TileDefinition> cache = new Dictionary<byte, TileDefinition>(256);

        public IReadOnlyList<TileDefinition> Definitions => definitions;

        public TileDefinition Get(byte id)
        {
            if (cache.Count == 0)
            {
                RebuildCache();
            }

            cache.TryGetValue(id, out var definition);
            return definition;
        }

        public bool IsWalkable(byte id)
        {
            var def = Get(id);
            return def != null && def.walkable && def.moveCost < 255;
        }

        public void SetDefinitions(IEnumerable<TileDefinition> newDefinitions)
        {
            definitions.Clear();
            definitions.AddRange(newDefinitions);
            RebuildCache();
        }

        public void RebuildCache()
        {
            cache.Clear();
            for (int i = 0; i < definitions.Count; i++)
            {
                var def = definitions[i];
                if (def == null)
                {
                    continue;
                }

                cache[def.id] = def;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildCache();
        }
#endif
    }
}
