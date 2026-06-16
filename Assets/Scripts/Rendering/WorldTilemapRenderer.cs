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
        [SerializeField] private RuntimeDebugTilePalette palette;
        [SerializeField] private AutoTilePalette autoTilePalette;
        [SerializeField] private int renderRadiusInChunks = 2;

        private Vector2Int lastChunkCenter = new Vector2Int(int.MinValue, int.MinValue);
        private readonly HashSet<Vector2Int> renderedChunks = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> targetChunks = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        private TileBase[] tileBuffer;
        private TileBase[] clearBuffer;

        public void Initialize(
            WorldTileMap map,
            Tilemap tilemap,
            RuntimeDebugTilePalette runtimePalette,
            int radiusInChunks = 2,
            AutoTilePalette bitmaskPalette = null)
        {
            worldTileMap = map;
            groundTilemap = tilemap;
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
            if (tileBuffer != null && tileBuffer.Length == tileCount)
            {
                return;
            }

            tileBuffer = new TileBase[tileCount];
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
                    byte tileId = chunk.GetLocal(localX, localY);
                    int tileX = chunkCoord.x * chunkSize + localX;
                    int tileY = chunkCoord.y * chunkSize + localY;
                    tileBuffer[rowOffset + localX] = ResolveTile(tileX, tileY, tileId);
                }
            }

            groundTilemap.SetTilesBlock(GetChunkBounds(chunkCoord, chunkSize), tileBuffer);
        }

        private TileBase ResolveTile(int tileX, int tileY, byte tileId)
        {
            if (tileId == 0)
            {
                return null;
            }

            if (autoTilePalette != null && autoTilePalette.TryGetSet(tileId, out AutoTileSet tileSet))
            {
                int mask = CalculateEightDirectionMask(tileX, tileY, tileSet);
                TileBase autoTile = tileSet.GetTile(mask);
                if (autoTile != null)
                {
                    return autoTile;
                }
            }

            return palette != null ? palette.GetTile(tileId) : null;
        }

        private int CalculateEightDirectionMask(int tileX, int tileY, AutoTileSet tileSet)
        {
            bool up = MatchesAutoTile(tileX, tileY + 1, tileSet);
            bool right = MatchesAutoTile(tileX + 1, tileY, tileSet);
            bool down = MatchesAutoTile(tileX, tileY - 1, tileSet);
            bool left = MatchesAutoTile(tileX - 1, tileY, tileSet);

            int mask = 0;
            if (up) mask |= 1;
            if (right) mask |= 4;
            if (down) mask |= 16;
            if (left) mask |= 64;

            if (up && right && MatchesAutoTile(tileX + 1, tileY + 1, tileSet)) mask |= 2;
            if (down && right && MatchesAutoTile(tileX + 1, tileY - 1, tileSet)) mask |= 8;
            if (down && left && MatchesAutoTile(tileX - 1, tileY - 1, tileSet)) mask |= 32;
            if (up && left && MatchesAutoTile(tileX - 1, tileY + 1, tileSet)) mask |= 128;

            return mask;
        }

        private bool MatchesAutoTile(int tileX, int tileY, AutoTileSet tileSet)
        {
            if (!worldTileMap.IsInBounds(tileX, tileY))
            {
                return false;
            }

            return tileSet.Matches(worldTileMap.GetTile(tileX, tileY));
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
            groundTilemap.SetTilesBlock(GetChunkBounds(chunkCoord, chunkSize), clearBuffer);
        }

        private static BoundsInt GetChunkBounds(Vector2Int chunkCoord, int chunkSize)
        {
            return new BoundsInt(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0, chunkSize, chunkSize, 1);
        }
    }
}
