using UnityEngine;
using TileMap.Data;
using TileMap.Utility;
using TileMap.World;

namespace TileMap.Generation
{
    public class BiomeGenerator : MonoBehaviour
    {
        [SerializeField] private int seed = 12345;
        [SerializeField] private float sectorNoiseStrength = 0.22f;
        [SerializeField] private int roadHalfWidth = 2;
        [SerializeField] private int roadSpacing = 720;
        [SerializeField] private float featureNoiseScale = 0.014f;

        public int Seed => seed;

        public void GenerateChunk(WorldTileMap world, int chunkX, int chunkY)
        {
            if (world == null || world.IsChunkGenerated(chunkX, chunkY))
            {
                return;
            }

            RectInt bounds = world.GetChunkBounds(chunkX, chunkY);
            int xMax = Mathf.Min(world.WorldWidth, bounds.xMax);
            int yMax = Mathf.Min(world.WorldHeight, bounds.yMax);

            for (int y = bounds.yMin; y < yMax; y++)
            {
                for (int x = bounds.xMin; x < xMax; x++)
                {
                    world.SetTile(x, y, GenerateTileAt(world, x, y));
                }
            }

            world.MarkChunkGenerated(chunkX, chunkY);
        }

        public BiomeType GetBiomeAt(WorldTileMap world, int x, int y)
        {
            float centerX = world.WorldWidth * 0.5f;
            float centerY = world.WorldHeight * 0.5f;
            float nx = (x - centerX) / (world.WorldWidth * 0.5f);
            float ny = (y - centerY) / (world.WorldHeight * 0.5f);
            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            float angle = Mathf.Atan2(ny, nx);
            float noise = NoiseUtility.FractalNoise01(x, y, seed, 3, 0.0027f, 0.55f);
            float warpedAngle = angle + (noise - 0.5f) * sectorNoiseStrength;

            if (distance < 0.065f)
            {
                return BiomeType.BossCore;
            }

            if (distance < 0.19f)
            {
                return BiomeType.Volcano;
            }

            if (distance < 0.28f && Mathf.Abs(ny) < 0.10f)
            {
                return BiomeType.Grassland;
            }

            if (warpedAngle >= 2.35f || warpedAngle < -2.45f)
            {
                return BiomeType.Forest;
            }

            if (warpedAngle >= -0.75f && warpedAngle < 0.55f)
            {
                return BiomeType.Swamp;
            }

            if (warpedAngle >= 0.55f && warpedAngle < 1.95f)
            {
                return BiomeType.Cave;
            }

            if (warpedAngle >= -2.45f && warpedAngle < -0.75f)
            {
                return BiomeType.Ruins;
            }

            return BiomeType.Grassland;
        }

        private byte GenerateTileAt(WorldTileMap world, int x, int y)
        {
            BiomeType biome = GetBiomeAt(world, x, y);

            if (IsSpawnArea(world, x, y))
            {
                return (byte)TileId.SpawnPlayer;
            }

            if (IsBossCore(world, x, y))
            {
                return GenerateBossCoreTile(world, x, y);
            }

            if (IsMainRoad(world, x, y))
            {
                return (byte)TileId.RoadA;
            }

            float feature = NoiseUtility.Hash01(x, y, seed + 991);
            float macroNoise = NoiseUtility.FractalNoise01(x, y, seed + 211, 4, featureNoiseScale, 0.55f);

            switch (biome)
            {
                case BiomeType.Grassland:
                    return GenerateGrasslandTile(x, y, feature, macroNoise);
                case BiomeType.Ruins:
                    return GenerateRuinsTile(x, y, feature, macroNoise);
                case BiomeType.Forest:
                    return GenerateForestTile(x, y, feature, macroNoise);
                case BiomeType.Swamp:
                    return GenerateSwampTile(x, y, feature, macroNoise);
                case BiomeType.Cave:
                    return GenerateCaveTile(x, y, feature, macroNoise);
                case BiomeType.Volcano:
                    return GenerateVolcanoTile(x, y, feature, macroNoise);
                case BiomeType.BossCore:
                    return (byte)TileId.BossArena;
                default:
                    return (byte)TileId.GrassA;
            }
        }

        private bool IsSpawnArea(WorldTileMap world, int x, int y)
        {
            Vector2Int spawn = GetRecommendedSpawnCell(world);
            return Mathf.Abs(x - spawn.x) <= 1 && Mathf.Abs(y - spawn.y) <= 1;
        }

        public Vector2Int GetRecommendedSpawnCell(WorldTileMap world)
        {
            return new Vector2Int(world.WorldWidth / 2, world.WorldHeight / 2 - 2200);
        }

        private bool IsBossCore(WorldTileMap world, int x, int y)
        {
            int centerX = world.WorldWidth / 2;
            int centerY = world.WorldHeight / 2;
            int dx = x - centerX;
            int dy = y - centerY;
            return dx * dx + dy * dy <= 100 * 100;
        }

        private bool IsMainRoad(WorldTileMap world, int x, int y)
        {
            int centerX = world.WorldWidth / 2;
            int centerY = world.WorldHeight / 2;

            bool ringRoad = Mathf.Abs(Mathf.Abs(x - centerX) - roadSpacing) <= roadHalfWidth && Mathf.Abs(y - centerY) < roadSpacing;
            bool verticalRoad = Mathf.Abs(x - centerX) <= roadHalfWidth;
            bool horizontalRoad = Mathf.Abs(y - (centerY - 2200)) <= roadHalfWidth && Mathf.Abs(x - centerX) <= roadSpacing * 2;

            return ringRoad || verticalRoad || horizontalRoad;
        }

        private byte GenerateBossCoreTile(WorldTileMap world, int x, int y)
        {
            int centerX = world.WorldWidth / 2;
            int centerY = world.WorldHeight / 2;
            int dx = x - centerX;
            int dy = y - centerY;
            int d2 = dx * dx + dy * dy;

            if (d2 <= 24 * 24)
            {
                return (byte)TileId.EndPortal;
            }

            if (d2 <= 64 * 64)
            {
                return (byte)TileId.BossArena;
            }

            return (byte)TileId.LavaShallow;
        }

        private byte GenerateGrasslandTile(int x, int y, float feature, float macroNoise)
        {
            if (feature < 0.012f) return (byte)TileId.OreCopper;
            if (feature < 0.020f) return (byte)TileId.Campfire;
            if (feature < 0.040f) return (byte)TileId.EnemyTier1;
            if (feature < 0.065f) return (byte)TileId.TallGrass;
            if (feature < 0.078f) return (byte)TileId.RockLarge;
            if (feature < 0.096f) return (byte)TileId.WaterShallow;
            if (macroNoise > 0.6f) return (byte)TileId.GrassB;
            return (byte)TileId.GrassA;
        }

        private byte GenerateRuinsTile(int x, int y, float feature, float macroNoise)
        {
            if (feature < 0.020f) return (byte)TileId.OldWall;
            if (feature < 0.038f) return (byte)TileId.EnemyTier1;
            if (feature < 0.048f) return (byte)TileId.OreCopper;
            if (feature < 0.055f) return (byte)TileId.MerchantSpot;
            if (feature < 0.065f) return (byte)TileId.TreasureMinor;
            if (macroNoise > 0.62f) return (byte)TileId.DirtA;
            return (byte)TileId.RuinFloor;
        }

        private byte GenerateForestTile(int x, int y, float feature, float macroNoise)
        {
            if (feature < 0.11f) return (byte)TileId.TreePine;
            if (feature < 0.125f) return (byte)TileId.BushDense;
            if (feature < 0.135f) return (byte)TileId.ResourceWood;
            if (feature < 0.150f) return (byte)TileId.EnemyTier3;
            if (feature < 0.156f) return (byte)TileId.ShrineNature;
            if (feature < 0.162f) return (byte)TileId.TreasureForest;
            return macroNoise > 0.58f ? (byte)TileId.ForestFloorB : (byte)TileId.ForestFloorA;
        }

        private byte GenerateSwampTile(int x, int y, float feature, float macroNoise)
        {
            if (feature < 0.060f) return (byte)TileId.SwampDeep;
            if (feature < 0.090f) return (byte)TileId.WaterShallow;
            if (feature < 0.120f) return (byte)TileId.EnemyTier3;
            if (feature < 0.132f) return (byte)TileId.OreIron;
            if (feature < 0.138f) return (byte)TileId.EventMinor;
            return macroNoise > 0.55f ? (byte)TileId.WaterDeep : (byte)TileId.SwampShallow;
        }

        private byte GenerateCaveTile(int x, int y, float feature, float macroNoise)
        {
            if (feature < 0.040f) return (byte)TileId.Pit;
            if (feature < 0.070f) return (byte)TileId.MineTrack;
            if (feature < 0.090f) return (byte)TileId.OreSilver;
            if (feature < 0.112f) return (byte)TileId.EnemyCave1;
            if (feature < 0.120f) return (byte)TileId.ForgeUnderground;
            if (feature < 0.126f) return (byte)TileId.TreasureCave;
            if (macroNoise > 0.63f) return (byte)TileId.CaveBossGate;
            return (byte)TileId.CaveFloorA;
        }

        private byte GenerateVolcanoTile(int x, int y, float feature, float macroNoise)
        {
            if (feature < 0.030f) return (byte)TileId.LavaDeep;
            if (feature < 0.070f) return (byte)TileId.LavaShallow;
            if (feature < 0.094f) return (byte)TileId.EnemyFire1;
            if (feature < 0.102f) return (byte)TileId.ShrineFire;
            if (feature < 0.110f) return (byte)TileId.OreMythril;
            return macroNoise > 0.58f ? (byte)TileId.LavaShallow : (byte)TileId.AshGround;
        }
    }
}
