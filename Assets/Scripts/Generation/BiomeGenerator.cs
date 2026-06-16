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
        [SerializeField] private bool placeGrassEdgeOverlays = true;
        [SerializeField] private TileId grassEdgeOverlayTile = TileId.GrassEdgeOverlay;
        [SerializeField] private TileId[] grassEdgeSourceGrounds = { TileId.GrassA, TileId.GrassB };
        [SerializeField] private TileId[] grassEdgeOverlayHosts = { TileId.RoadA, TileId.DirtA, TileId.RuinFloor };

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
                    GenerateTileAt(world, x, y, out byte groundTileId, out byte overlayTileId);
                    world.SetLayeredTile(x, y, groundTileId, overlayTileId);
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

        private void GenerateTileAt(WorldTileMap world, int x, int y, out byte groundTileId, out byte overlayTileId)
        {
            GenerateBaseTileAt(world, x, y, out groundTileId, out overlayTileId);
            if (overlayTileId == (byte)TileId.Void && ShouldPlaceGrassEdgeOverlay(world, x, y, groundTileId))
            {
                overlayTileId = (byte)grassEdgeOverlayTile;
            }
        }

        private void GenerateBaseTileAt(WorldTileMap world, int x, int y, out byte groundTileId, out byte overlayTileId)
        {
            BiomeType biome = GetBiomeAt(world, x, y);
            overlayTileId = (byte)TileId.Void;

            if (IsSpawnArea(world, x, y))
            {
                groundTileId = IsMainRoad(world, x, y) ? (byte)TileId.RoadA : (byte)TileId.GrassA;
                overlayTileId = (byte)TileId.SpawnPlayer;
                return;
            }

            if (IsBossCore(world, x, y))
            {
                GenerateBossCoreTile(world, x, y, out groundTileId, out overlayTileId);
                return;
            }

            if (IsMainRoad(world, x, y))
            {
                groundTileId = (byte)TileId.RoadA;
                return;
            }

            float feature = NoiseUtility.Hash01(x, y, seed + 991);
            float macroNoise = NoiseUtility.FractalNoise01(x, y, seed + 211, 4, featureNoiseScale, 0.55f);

            switch (biome)
            {
                case BiomeType.Grassland:
                    GenerateGrasslandTile(feature, macroNoise, out groundTileId, out overlayTileId);
                    return;
                case BiomeType.Ruins:
                    GenerateRuinsTile(feature, macroNoise, out groundTileId, out overlayTileId);
                    return;
                case BiomeType.Forest:
                    GenerateForestTile(feature, macroNoise, out groundTileId, out overlayTileId);
                    return;
                case BiomeType.Swamp:
                    GenerateSwampTile(feature, macroNoise, out groundTileId, out overlayTileId);
                    return;
                case BiomeType.Cave:
                    GenerateCaveTile(feature, macroNoise, out groundTileId, out overlayTileId);
                    return;
                case BiomeType.Volcano:
                    GenerateVolcanoTile(feature, macroNoise, out groundTileId, out overlayTileId);
                    return;
                case BiomeType.BossCore:
                    groundTileId = (byte)TileId.BossArena;
                    return;
                default:
                    groundTileId = (byte)TileId.GrassA;
                    return;
            }
        }

        private bool ShouldPlaceGrassEdgeOverlay(WorldTileMap world, int x, int y, byte groundTileId)
        {
            if (!placeGrassEdgeOverlays
                || grassEdgeOverlayTile == TileId.Void
                || !ContainsTileId(grassEdgeOverlayHosts, groundTileId))
            {
                return false;
            }

            return HasCardinalGroundNeighbor(world, x, y, grassEdgeSourceGrounds);
        }

        private bool HasCardinalGroundNeighbor(WorldTileMap world, int x, int y, TileId[] groundTileIds)
        {
            return IsGeneratedGroundInSet(world, x, y + 1, groundTileIds)
                || IsGeneratedGroundInSet(world, x + 1, y, groundTileIds)
                || IsGeneratedGroundInSet(world, x, y - 1, groundTileIds)
                || IsGeneratedGroundInSet(world, x - 1, y, groundTileIds);
        }

        private bool IsGeneratedGroundInSet(WorldTileMap world, int x, int y, TileId[] groundTileIds)
        {
            if (!world.IsInBounds(x, y))
            {
                return false;
            }

            GenerateBaseTileAt(world, x, y, out byte neighborGroundTileId, out _);
            return ContainsTileId(groundTileIds, neighborGroundTileId);
        }

        private static bool ContainsTileId(TileId[] tileIds, byte tileId)
        {
            if (tileIds == null)
            {
                return false;
            }

            for (int i = 0; i < tileIds.Length; i++)
            {
                if (tileId == (byte)tileIds[i])
                {
                    return true;
                }
            }

            return false;
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

        private void GenerateBossCoreTile(WorldTileMap world, int x, int y, out byte groundTileId, out byte overlayTileId)
        {
            overlayTileId = (byte)TileId.Void;
            int centerX = world.WorldWidth / 2;
            int centerY = world.WorldHeight / 2;
            int dx = x - centerX;
            int dy = y - centerY;
            int d2 = dx * dx + dy * dy;

            if (d2 <= 24 * 24)
            {
                groundTileId = (byte)TileId.BossArena;
                overlayTileId = (byte)TileId.EndPortal;
                return;
            }

            if (d2 <= 64 * 64)
            {
                groundTileId = (byte)TileId.BossArena;
                return;
            }

            groundTileId = (byte)TileId.LavaShallow;
        }

        private void GenerateGrasslandTile(float feature, float macroNoise, out byte groundTileId, out byte overlayTileId)
        {
            groundTileId = macroNoise > 0.6f ? (byte)TileId.GrassB : (byte)TileId.GrassA;
            overlayTileId = (byte)TileId.Void;

            if (feature < 0.012f) overlayTileId = (byte)TileId.OreCopper;
            else if (feature < 0.020f) overlayTileId = (byte)TileId.Campfire;
            else if (feature < 0.040f) overlayTileId = (byte)TileId.EnemyTier1;
            else if (feature < 0.065f) overlayTileId = (byte)TileId.TallGrass;
            else if (feature < 0.078f) overlayTileId = (byte)TileId.RockLarge;
            else if (feature < 0.096f) groundTileId = (byte)TileId.WaterShallow;
        }

        private void GenerateRuinsTile(float feature, float macroNoise, out byte groundTileId, out byte overlayTileId)
        {
            groundTileId = macroNoise > 0.62f ? (byte)TileId.DirtA : (byte)TileId.RuinFloor;
            overlayTileId = (byte)TileId.Void;

            if (feature < 0.020f) overlayTileId = (byte)TileId.OldWall;
            else if (feature < 0.038f) overlayTileId = (byte)TileId.EnemyTier1;
            else if (feature < 0.048f) overlayTileId = (byte)TileId.OreCopper;
            else if (feature < 0.055f) overlayTileId = (byte)TileId.MerchantSpot;
            else if (feature < 0.065f) overlayTileId = (byte)TileId.TreasureMinor;
        }

        private void GenerateForestTile(float feature, float macroNoise, out byte groundTileId, out byte overlayTileId)
        {
            groundTileId = macroNoise > 0.58f ? (byte)TileId.ForestFloorB : (byte)TileId.ForestFloorA;
            overlayTileId = (byte)TileId.Void;

            if (feature < 0.11f) overlayTileId = (byte)TileId.TreePine;
            else if (feature < 0.125f) overlayTileId = (byte)TileId.BushDense;
            else if (feature < 0.135f) overlayTileId = (byte)TileId.ResourceWood;
            else if (feature < 0.150f) overlayTileId = (byte)TileId.EnemyTier3;
            else if (feature < 0.156f) overlayTileId = (byte)TileId.ShrineNature;
            else if (feature < 0.162f) overlayTileId = (byte)TileId.TreasureForest;
        }

        private void GenerateSwampTile(float feature, float macroNoise, out byte groundTileId, out byte overlayTileId)
        {
            groundTileId = (byte)TileId.SwampShallow;
            overlayTileId = (byte)TileId.Void;

            if (feature < 0.060f) groundTileId = (byte)TileId.SwampDeep;
            else if (feature < 0.090f) groundTileId = (byte)TileId.WaterShallow;
            else if (feature < 0.120f) overlayTileId = (byte)TileId.EnemyTier3;
            else if (feature < 0.132f) overlayTileId = (byte)TileId.OreIron;
            else if (feature < 0.138f) overlayTileId = (byte)TileId.EventMinor;
            else groundTileId = macroNoise > 0.55f ? (byte)TileId.WaterDeep : (byte)TileId.SwampShallow;
        }

        private void GenerateCaveTile(float feature, float macroNoise, out byte groundTileId, out byte overlayTileId)
        {
            groundTileId = (byte)TileId.CaveFloorA;
            overlayTileId = (byte)TileId.Void;

            if (feature < 0.040f) groundTileId = (byte)TileId.Pit;
            else if (feature < 0.070f) groundTileId = (byte)TileId.MineTrack;
            else if (feature < 0.090f) overlayTileId = (byte)TileId.OreSilver;
            else if (feature < 0.112f) overlayTileId = (byte)TileId.EnemyCave1;
            else if (feature < 0.120f) overlayTileId = (byte)TileId.ForgeUnderground;
            else if (feature < 0.126f) overlayTileId = (byte)TileId.TreasureCave;
            else if (macroNoise > 0.63f) overlayTileId = (byte)TileId.CaveBossGate;
        }

        private void GenerateVolcanoTile(float feature, float macroNoise, out byte groundTileId, out byte overlayTileId)
        {
            groundTileId = (byte)TileId.AshGround;
            overlayTileId = (byte)TileId.Void;

            if (feature < 0.030f) groundTileId = (byte)TileId.LavaDeep;
            else if (feature < 0.070f) groundTileId = (byte)TileId.LavaShallow;
            else if (feature < 0.094f) overlayTileId = (byte)TileId.EnemyFire1;
            else if (feature < 0.102f) overlayTileId = (byte)TileId.ShrineFire;
            else if (feature < 0.110f) overlayTileId = (byte)TileId.OreMythril;
            else groundTileId = macroNoise > 0.58f ? (byte)TileId.LavaShallow : (byte)TileId.AshGround;
        }
    }
}
