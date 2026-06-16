using UnityEngine;

namespace TileMap.Data
{
    [CreateAssetMenu(menuName = "Tools/World/Tile Definition", fileName = "TileDefinition")]
    public class TileDefinition : ScriptableObject
    {
        [Header("Identity")]
        public byte id;
        public string tileName;

        [Header("Navigation")]
        public bool walkable = true;
        public bool blocksVision;
        public bool blocksProjectiles;
        [Tooltip("100 = normal speed, 130 = slower, 255 = blocked")]
        public byte moveCost = 100;

        [Header("World Semantics")]
        public BiomeType biomeType;
        [Range(0, 255)] public byte danger;
        [Range(0, 255)] public byte lootTier;
        [Range(0, 255)] public byte encounterTier;

        [Header("Gameplay Effects")]
        public byte gatherType;
        public byte damageType;
        public byte damageValue;

        public bool IsBlocked => !walkable || moveCost >= 255;
    }
}
