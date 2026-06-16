using System.Collections.Generic;
using UnityEngine;
using TileMap.Data;

namespace TileMap.World
{
    public class WorldTileMap : MonoBehaviour
    {
        [SerializeField] private int worldWidth = 10000;
        [SerializeField] private int worldHeight = 10000;
        [SerializeField] private int chunkSize = 64;
        [SerializeField] private TileDatabase tileDatabase;

        private readonly Dictionary<long, MapChunk> chunks = new Dictionary<long, MapChunk>();
        private readonly Dictionary<long, int> occupiedCells = new Dictionary<long, int>();

        public int WorldWidth => worldWidth;
        public int WorldHeight => worldHeight;
        public int ChunkSize => chunkSize;
        public int ChunkCountX => Mathf.CeilToInt(worldWidth / (float)chunkSize);
        public int ChunkCountY => Mathf.CeilToInt(worldHeight / (float)chunkSize);
        public TileDatabase TileDatabase => tileDatabase;

        public void Initialize(int width, int height, int newChunkSize, TileDatabase database)
        {
            worldWidth = Mathf.Max(1, width);
            worldHeight = Mathf.Max(1, height);
            chunkSize = Mathf.Clamp(newChunkSize, 8, 256);
            tileDatabase = database;
            chunks.Clear();
            occupiedCells.Clear();
        }

        public void SetTileDatabase(TileDatabase database)
        {
            tileDatabase = database;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < worldWidth && y < worldHeight;
        }

        public byte GetTile(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return (byte)TileId.Void;
            }

            GetChunkAndLocal(x, y, out int chunkX, out int chunkY, out int localX, out int localY);
            if (!TryGetChunk(chunkX, chunkY, out var chunk))
            {
                return (byte)TileId.Void;
            }

            return chunk.GetLocal(localX, localY);
        }

        public void SetTile(int x, int y, byte tileId)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            GetChunkAndLocal(x, y, out int chunkX, out int chunkY, out int localX, out int localY);
            var chunk = GetOrCreateChunk(chunkX, chunkY);
            chunk.SetLocal(localX, localY, tileId);
        }

        public bool IsWalkable(int x, int y, bool ignoreOccupancy = false)
        {
            if (!IsInBounds(x, y) || tileDatabase == null)
            {
                return false;
            }

            byte tileId = GetTile(x, y);
            if (!tileDatabase.IsWalkable(tileId))
            {
                return false;
            }

            return ignoreOccupancy || !IsOccupied(x, y);
        }

        public bool IsOccupied(int x, int y)
        {
            return occupiedCells.ContainsKey(CellKey(x, y));
        }

        public void SetOccupied(int x, int y, int unitId)
        {
            occupiedCells[CellKey(x, y)] = unitId;
        }

        public void ClearOccupied(int x, int y)
        {
            occupiedCells.Remove(CellKey(x, y));
        }

        public int GetOccupant(int x, int y)
        {
            return occupiedCells.TryGetValue(CellKey(x, y), out var unitId) ? unitId : 0;
        }

        public Vector2Int WorldToChunk(int x, int y)
        {
            return new Vector2Int(x / chunkSize, y / chunkSize);
        }

        public RectInt GetChunkBounds(int chunkX, int chunkY)
        {
            return new RectInt(chunkX * chunkSize, chunkY * chunkSize, chunkSize, chunkSize);
        }

        public bool HasChunk(int chunkX, int chunkY)
        {
            return chunks.ContainsKey(ChunkKey(chunkX, chunkY));
        }

        public bool IsChunkGenerated(int chunkX, int chunkY)
        {
            return TryGetChunk(chunkX, chunkY, out var chunk) && chunk.generated;
        }

        public void MarkChunkGenerated(int chunkX, int chunkY)
        {
            var chunk = GetOrCreateChunk(chunkX, chunkY);
            chunk.generated = true;
        }

        public bool TryGetChunk(int chunkX, int chunkY, out MapChunk chunk)
        {
            return chunks.TryGetValue(ChunkKey(chunkX, chunkY), out chunk);
        }

        public MapChunk GetOrCreateChunk(int chunkX, int chunkY)
        {
            long key = ChunkKey(chunkX, chunkY);
            if (!chunks.TryGetValue(key, out var chunk))
            {
                chunk = new MapChunk(chunkX, chunkY, chunkSize);
                chunks.Add(key, chunk);
            }

            return chunk;
        }

        public IEnumerable<MapChunk> GetLoadedChunks()
        {
            return chunks.Values;
        }

        public void FillRect(RectInt rect, byte tileId)
        {
            int xMin = Mathf.Max(0, rect.xMin);
            int yMin = Mathf.Max(0, rect.yMin);
            int xMax = Mathf.Min(worldWidth, rect.xMax);
            int yMax = Mathf.Min(worldHeight, rect.yMax);

            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    SetTile(x, y, tileId);
                }
            }
        }

        public void StampCircle(Vector2Int center, int radius, byte tileId)
        {
            int radiusSquared = radius * radius;
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    int dx = x - center.x;
                    int dy = y - center.y;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        SetTile(x, y, tileId);
                    }
                }
            }
        }

        private void GetChunkAndLocal(int x, int y, out int chunkX, out int chunkY, out int localX, out int localY)
        {
            chunkX = x / chunkSize;
            chunkY = y / chunkSize;
            localX = x % chunkSize;
            localY = y % chunkSize;
        }

        private static long ChunkKey(int chunkX, int chunkY)
        {
            return ((long)chunkX << 32) ^ (uint)chunkY;
        }

        private static long CellKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}
