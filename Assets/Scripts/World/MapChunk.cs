using System;

namespace TileMap.World
{
    [Serializable]
    public class MapChunk
    {
        public int chunkX;
        public int chunkY;
        public int chunkSize;
        public byte[] groundTiles;
        public byte[] overlayTiles;
        public bool generated;

        public MapChunk(int chunkX, int chunkY, int chunkSize)
        {
            this.chunkX = chunkX;
            this.chunkY = chunkY;
            this.chunkSize = chunkSize;
            groundTiles = new byte[chunkSize * chunkSize];
            overlayTiles = new byte[chunkSize * chunkSize];
            generated = false;
        }

        public byte GetLocal(int localX, int localY, TileLayer layer = TileLayer.Effective)
        {
            int index = localY * chunkSize + localX;
            switch (layer)
            {
                case TileLayer.Ground:
                    return groundTiles[index];
                case TileLayer.Overlay:
                    return overlayTiles[index];
                default:
                    return overlayTiles[index] != 0 ? overlayTiles[index] : groundTiles[index];
            }
        }

        public void SetLocal(int localX, int localY, byte tileId, TileLayer layer = TileLayer.Ground)
        {
            int index = localY * chunkSize + localX;
            if (layer == TileLayer.Overlay)
            {
                overlayTiles[index] = tileId;
                return;
            }

            groundTiles[index] = tileId;
        }

        public void SetLayeredLocal(int localX, int localY, byte groundTileId, byte overlayTileId)
        {
            int index = localY * chunkSize + localX;
            groundTiles[index] = groundTileId;
            overlayTiles[index] = overlayTileId;
        }
    }
}
