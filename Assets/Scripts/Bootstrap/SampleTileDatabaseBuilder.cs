using System.Collections.Generic;
using UnityEngine;
using TileMap.Data;

namespace TileMap.Bootstrap
{
    public static class SampleTileDatabaseBuilder
    {
        public static TileDatabase CreateRuntimeDatabase()
        {
            var database = ScriptableObject.CreateInstance<TileDatabase>();
            database.name = "RuntimeTileDatabase";

            var definitions = new List<TileDefinition>
            {
                Create((byte)TileId.Void, "Void", false, 255, BiomeType.None),
                Create((byte)TileId.SpawnPlayer, "Spawn Player", true, 100, BiomeType.Grassland),
                Create((byte)TileId.EventMinor, "Event Minor", true, 100, BiomeType.Ruins, lootTier: 40),
                Create((byte)TileId.TreasureMinor, "Treasure Minor", true, 100, BiomeType.Ruins, lootTier: 70),

                Create((byte)TileId.GrassA, "Grass A", true, 100, BiomeType.Grassland),
                Create((byte)TileId.GrassB, "Grass B", true, 100, BiomeType.Grassland),
                Create((byte)TileId.DirtA, "Dirt A", true, 100, BiomeType.Grassland),
                Create((byte)TileId.RoadA, "Road", true, 85, BiomeType.Grassland),
                Create((byte)TileId.TallGrass, "Tall Grass", true, 115, BiomeType.Grassland),
                Create((byte)TileId.RockLarge, "Large Rock", false, 255, BiomeType.Grassland),
                Create((byte)TileId.RuinFloor, "Ruin Floor", true, 100, BiomeType.Ruins),
                Create((byte)TileId.OldWall, "Old Wall", false, 255, BiomeType.Ruins, blocksVision: true),
                Create((byte)TileId.Campfire, "Campfire", true, 100, BiomeType.Grassland, lootTier: 25),
                Create((byte)TileId.MerchantSpot, "Merchant Spot", true, 100, BiomeType.Ruins, lootTier: 40),
                Create((byte)TileId.EnemyTier1, "Enemy Tier 1", true, 100, BiomeType.Grassland, danger: 35, encounterTier: 45),
                Create((byte)TileId.OreCopper, "Copper Ore", true, 115, BiomeType.Grassland, lootTier: 40, gatherType: 1),
                Create((byte)TileId.WaterShallow, "Shallow Water", true, 135, BiomeType.Swamp),
                Create((byte)TileId.WaterDeep, "Deep Water", false, 255, BiomeType.Swamp),

                Create((byte)TileId.ForestFloorA, "Forest Floor A", true, 100, BiomeType.Forest),
                Create((byte)TileId.ForestFloorB, "Forest Floor B", true, 110, BiomeType.Forest),
                Create((byte)TileId.BushDense, "Dense Bush", false, 255, BiomeType.Forest, blocksVision: true),
                Create((byte)TileId.TreePine, "Pine Tree", false, 255, BiomeType.Forest, blocksVision: true),
                Create((byte)TileId.SwampShallow, "Swamp Shallow", true, 140, BiomeType.Swamp, danger: 40),
                Create((byte)TileId.SwampDeep, "Swamp Deep", false, 255, BiomeType.Swamp, damageType: 1, damageValue: 1),
                Create((byte)TileId.EnemyTier3, "Enemy Tier 3", true, 100, BiomeType.Forest, danger: 90, encounterTier: 120),
                Create((byte)TileId.ResourceWood, "Wood Resource", true, 115, BiomeType.Forest, lootTier: 55, gatherType: 3),
                Create((byte)TileId.OreIron, "Iron Ore", true, 120, BiomeType.Swamp, lootTier: 70, gatherType: 1),
                Create((byte)TileId.ShrineNature, "Nature Shrine", true, 100, BiomeType.Forest, lootTier: 80),
                Create((byte)TileId.TreasureForest, "Treasure Forest", true, 100, BiomeType.Forest, lootTier: 95),

                Create((byte)TileId.CaveFloorA, "Cave Floor", true, 100, BiomeType.Cave),
                Create((byte)TileId.Pit, "Pit", false, 255, BiomeType.Cave),
                Create((byte)TileId.MineTrack, "Mine Track", true, 95, BiomeType.Cave),
                Create((byte)TileId.OreSilver, "Silver Ore", true, 120, BiomeType.Cave, lootTier: 100, gatherType: 1),
                Create((byte)TileId.EnemyCave1, "Cave Enemy", true, 100, BiomeType.Cave, danger: 110, encounterTier: 130),
                Create((byte)TileId.CaveBossGate, "Cave Boss Gate", true, 100, BiomeType.Cave, danger: 140),
                Create((byte)TileId.ForgeUnderground, "Underground Forge", true, 100, BiomeType.Cave, lootTier: 110),
                Create((byte)TileId.TreasureCave, "Treasure Cave", true, 100, BiomeType.Cave, lootTier: 120),

                Create((byte)TileId.AshGround, "Ash Ground", true, 100, BiomeType.Volcano, danger: 140),
                Create((byte)TileId.LavaShallow, "Lava Shallow", true, 150, BiomeType.Volcano, danger: 170, damageType: 2, damageValue: 1),
                Create((byte)TileId.LavaDeep, "Lava Deep", false, 255, BiomeType.Volcano, danger: 220, damageType: 2, damageValue: 3),
                Create((byte)TileId.EnemyFire1, "Fire Enemy", true, 100, BiomeType.Volcano, danger: 180, encounterTier: 180),
                Create((byte)TileId.BossArena, "Boss Arena", true, 100, BiomeType.BossCore, danger: 250, encounterTier: 255),
                Create((byte)TileId.ShrineFire, "Fire Shrine", true, 100, BiomeType.Volcano, lootTier: 130),
                Create((byte)TileId.OreMythril, "Mythril Ore", true, 120, BiomeType.Volcano, lootTier: 150, gatherType: 1),
                Create((byte)TileId.EndPortal, "End Portal", true, 100, BiomeType.BossCore, lootTier: 255),
            };

            database.SetDefinitions(definitions);
            return database;
        }

        private static TileDefinition Create(
            byte id,
            string tileName,
            bool walkable,
            byte moveCost,
            BiomeType biomeType,
            bool blocksVision = false,
            bool blocksProjectiles = false,
            byte danger = 0,
            byte lootTier = 0,
            byte encounterTier = 0,
            byte gatherType = 0,
            byte damageType = 0,
            byte damageValue = 0)
        {
            var def = ScriptableObject.CreateInstance<TileDefinition>();
            def.name = tileName;
            def.id = id;
            def.tileName = tileName;
            def.walkable = walkable;
            def.moveCost = moveCost;
            def.biomeType = biomeType;
            def.blocksVision = blocksVision;
            def.blocksProjectiles = blocksProjectiles;
            def.danger = danger;
            def.lootTier = lootTier;
            def.encounterTier = encounterTier;
            def.gatherType = gatherType;
            def.damageType = damageType;
            def.damageValue = damageValue;
            return def;
        }
    }
}
