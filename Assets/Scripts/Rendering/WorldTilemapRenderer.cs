using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMap.Data;
using TileMap.World;

namespace TileMap.Rendering
{
    public class WorldTilemapRenderer : MonoBehaviour
    {
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap overlayTilemap;
        [SerializeField] private RuntimeDebugTilePalette palette;
        [SerializeField] private AutoTilePalette autoTilePalette;
        [SerializeField] private int renderRadiusInChunks = 2;

        private Vector2Int lastChunkCenter = new Vector2Int(int.MinValue, int.MinValue);
        private readonly HashSet<Vector2Int> renderedChunks = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> targetChunks = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        private TileBase[] groundTileBuffer;
        private TileBase[] overlayTileBuffer;
        private TileBase[] clearBuffer;

        public void Initialize(
            WorldTileMap map,
            Tilemap tilemap,
            RuntimeDebugTilePalette runtimePalette,
            int radiusInChunks = 2,
            AutoTilePalette bitmaskPalette = null)
        {
            Initialize(map, tilemap, null, runtimePalette, radiusInChunks, bitmaskPalette);
        }

        public void Initialize(
            WorldTileMap map,
            Tilemap groundLayer,
            Tilemap overlayLayer,
            RuntimeDebugTilePalette runtimePalette,
            int radiusInChunks = 2,
            AutoTilePalette bitmaskPalette = null)
        {
            worldTileMap = map;
            groundTilemap = groundLayer;
            overlayTilemap = overlayLayer;
            palette = runtimePalette;
            autoTilePalette = bitmaskPalette;
            renderRadiusInChunks = Mathf.Max(1, radiusInChunks);

            if (autoTilePalette != null && autoTilePalette.IsEmpty)
            {
                Debug.LogWarning("AutoTilePalette is assigned, but Tile Sets is empty. Falling back to runtime debug color tiles.");
            }
        }

        public void SetAutoTilePalette(AutoTilePalette bitmaskPalette)
        {
            autoTilePalette = bitmaskPalette;
        }

        public void RenderAroundCell(Vector2Int cell)
        {
            RenderAroundCell(cell, false);
        }

        public void RenderAroundCell(Vector2Int cell, bool force)
        {
            if (worldTileMap == null || groundTilemap == null)
            {
                return;
            }

            Vector2Int chunkCenter = worldTileMap.WorldToChunk(cell.x, cell.y);
            if (!force && chunkCenter == lastChunkCenter)
            {
                return;
            }

            lastChunkCenter = chunkCenter;
            int chunkSize = worldTileMap.ChunkSize;

            EnsureBuffers(chunkSize);
            targetChunks.Clear();
            for (int cy = chunkCenter.y - renderRadiusInChunks; cy <= chunkCenter.y + renderRadiusInChunks; cy++)
            {
                for (int cx = chunkCenter.x - renderRadiusInChunks; cx <= chunkCenter.x + renderRadiusInChunks; cx++)
                {
                    if (cx < 0 || cy < 0 || cx >= worldTileMap.ChunkCountX || cy >= worldTileMap.ChunkCountY)
                    {
                        continue;
                    }

                    if (worldTileMap.TryGetChunk(cx, cy, out _))
                    {
                        targetChunks.Add(new Vector2Int(cx, cy));
                    }
                }
            }

            chunksToRemove.Clear();
            foreach (Vector2Int renderedChunk in renderedChunks)
            {
                if (!targetChunks.Contains(renderedChunk))
                {
                    chunksToRemove.Add(renderedChunk);
                }
            }

            foreach (Vector2Int chunk in chunksToRemove)
            {
                ClearChunkBlock(chunk, chunkSize);
                renderedChunks.Remove(chunk);
            }

            foreach (Vector2Int chunk in targetChunks)
            {
                RenderChunkBlock(chunk, chunkSize);
                renderedChunks.Add(chunk);
            }
        }

        public void RenderChunkIfVisible(Vector2Int chunkCoord, Vector2Int referenceCell)
        {
            if (worldTileMap == null || groundTilemap == null)
            {
                return;
            }

            Vector2Int chunkCenter = worldTileMap.WorldToChunk(referenceCell.x, referenceCell.y);
            if (chunkCenter != lastChunkCenter)
            {
                RenderAroundCell(referenceCell);
            }

            if (!IsChunkInsideRenderRadius(chunkCoord, chunkCenter))
            {
                return;
            }

            int chunkSize = worldTileMap.ChunkSize;
            EnsureBuffers(chunkSize);
            RenderChunkBlock(chunkCoord, chunkSize);
            renderedChunks.Add(chunkCoord);
            RenderVisibleNeighborChunks(chunkCoord, chunkCenter, chunkSize);
        }

        private bool IsChunkInsideRenderRadius(Vector2Int chunkCoord, Vector2Int chunkCenter)
        {
            return Mathf.Abs(chunkCoord.x - chunkCenter.x) <= renderRadiusInChunks
                && Mathf.Abs(chunkCoord.y - chunkCenter.y) <= renderRadiusInChunks;
        }

        private void EnsureBuffers(int chunkSize)
        {
            int tileCount = chunkSize * chunkSize;
            if (groundTileBuffer != null && groundTileBuffer.Length == tileCount)
            {
                return;
            }

            groundTileBuffer = new TileBase[tileCount];
            overlayTileBuffer = new TileBase[tileCount];
            clearBuffer = new TileBase[tileCount];
        }

        private void RenderChunkBlock(Vector2Int chunkCoord, int chunkSize)
        {
            if (!worldTileMap.TryGetChunk(chunkCoord.x, chunkCoord.y, out var chunk))
            {
                return;
            }

            for (int localY = 0; localY < chunkSize; localY++)
            {
                int rowOffset = localY * chunkSize;
                for (int localX = 0; localX < chunkSize; localX++)
                {
                    int tileX = chunkCoord.x * chunkSize + localX;
                    int tileY = chunkCoord.y * chunkSize + localY;
                    byte groundTileId = chunk.GetLocal(localX, localY, TileLayer.Ground);
                    byte overlayTileId = chunk.GetLocal(localX, localY, TileLayer.Overlay);
                    int bufferIndex = rowOffset + localX;
                    groundTileBuffer[bufferIndex] = ResolveTile(tileX, tileY, groundTileId, TileLayer.Ground);
                    overlayTileBuffer[bufferIndex] = ResolveTile(tileX, tileY, overlayTileId, TileLayer.Overlay);
                }
            }

            BoundsInt bounds = GetChunkBounds(chunkCoord, chunkSize);
            groundTilemap.SetTilesBlock(bounds, groundTileBuffer);
            if (overlayTilemap != null)
            {
                overlayTilemap.SetTilesBlock(bounds, overlayTileBuffer);
            }
        }

        private TileBase ResolveTile(int tileX, int tileY, byte tileId, TileLayer layer)
        {
            if (tileId == 0)
            {
                return null;
            }

            if (autoTilePalette != null && autoTilePalette.TryGetSet(tileId, out AutoTileSet tileSet))
            {
                TileLayer maskLayer = ResolveMaskLayer(layer, tileSet.MaskSource);
                int mask = CalculateEightDirectionMask(tileX, tileY, tileSet, maskLayer);
                TileBase autoTile = tileSet.GetTile(mask);
                if (autoTile != null)
                {
                    return autoTile;
                }
            }

            return palette != null ? palette.GetTile(tileId) : null;
        }

        private static TileLayer ResolveMaskLayer(TileLayer renderLayer, AutoTileMaskSource maskSource)
        {
            switch (maskSource)
            {
                case AutoTileMaskSource.GroundLayer:
                    return TileLayer.Ground;
                case AutoTileMaskSource.OverlayLayer:
                    return TileLayer.Overlay;
                case AutoTileMaskSource.EffectiveLayer:
                    return TileLayer.Effective;
                default:
                    return renderLayer;
            }
        }

        private int CalculateEightDirectionMask(int tileX, int tileY, AutoTileSet tileSet, TileLayer layer)
        {
            bool up = MatchesAutoTile(tileX, tileY + 1, tileSet, layer);
            bool right = MatchesAutoTile(tileX + 1, tileY, tileSet, layer);
            bool down = MatchesAutoTile(tileX, tileY - 1, tileSet, layer);
            bool left = MatchesAutoTile(tileX - 1, tileY, tileSet, layer);

            int mask = 0;
            if (up) mask |= 1;
            if (right) mask |= 4;
            if (down) mask |= 16;
            if (left) mask |= 64;

            if (up && right && MatchesAutoTile(tileX + 1, tileY + 1, tileSet, layer)) mask |= 2;
            if (down && right && MatchesAutoTile(tileX + 1, tileY - 1, tileSet, layer)) mask |= 8;
            if (down && left && MatchesAutoTile(tileX - 1, tileY - 1, tileSet, layer)) mask |= 32;
            if (up && left && MatchesAutoTile(tileX - 1, tileY + 1, tileSet, layer)) mask |= 128;

            return mask;
        }

        private bool MatchesAutoTile(int tileX, int tileY, AutoTileSet tileSet, TileLayer layer)
        {
            if (!worldTileMap.IsInBounds(tileX, tileY))
            {
                return false;
            }

            return tileSet.Matches(worldTileMap.GetTile(tileX, tileY, layer));
        }

        private void RenderVisibleNeighborChunks(Vector2Int chunkCoord, Vector2Int chunkCenter, int chunkSize)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    if (offsetX == 0 && offsetY == 0)
                    {
                        continue;
                    }

                    Vector2Int neighbor = new Vector2Int(chunkCoord.x + offsetX, chunkCoord.y + offsetY);
                    if (!renderedChunks.Contains(neighbor) || !IsChunkInsideRenderRadius(neighbor, chunkCenter))
                    {
                        continue;
                    }

                    RenderChunkBlock(neighbor, chunkSize);
                }
            }
        }

        private void ClearChunkBlock(Vector2Int chunkCoord, int chunkSize)
        {
            BoundsInt bounds = GetChunkBounds(chunkCoord, chunkSize);
            groundTilemap.SetTilesBlock(bounds, clearBuffer);
            if (overlayTilemap != null)
            {
                overlayTilemap.SetTilesBlock(bounds, clearBuffer);
            }
        }

        private static BoundsInt GetChunkBounds(Vector2Int chunkCoord, int chunkSize)
        {
            return new BoundsInt(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0, chunkSize, chunkSize, 1);
        }
    }
}
