using System;

namespace TileMap.World
{
    [Serializable]
    public class MapChunk
    {
        public int chunkX;
        public int chunkY;
        public int chunkSize;
        public byte[] tiles;
        public bool generated;

        public MapChunk(int chunkX, int chunkY, int chunkSize)
        {
            this.chunkX = chunkX;
            this.chunkY = chunkY;
            this.chunkSize = chunkSize;
            tiles = new byte[chunkSize * chunkSize];
            generated = false;
        }

        public byte GetLocal(int localX, int localY)
        {
            return tiles[localY * chunkSize + localX];
        }

        public void SetLocal(int localX, int localY, byte tileId)
        {
            tiles[localY * chunkSize + localX] = tileId;
        }
    }
}
