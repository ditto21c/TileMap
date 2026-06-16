using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMap.Rendering;
using TileMap.World;
using TileMap.Movement;

namespace TileMap.Generation
{
    public class WorldStreamingController : MonoBehaviour
    {
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private BiomeGenerator biomeGenerator;
        [SerializeField] private WorldTilemapRenderer tilemapRenderer;
        [SerializeField] private PlayerGridMover player;
        [SerializeField] private int generationRadiusInChunks = 2;
        [SerializeField] private int chunksGeneratedPerFrame = 1;

        private Vector2Int lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
        private readonly Queue<Vector2Int> chunkGenerationQueue = new Queue<Vector2Int>();
        private readonly HashSet<Vector2Int> queuedChunks = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> candidateChunks = new List<Vector2Int>();
        private Coroutine generationCoroutine;

        public void Initialize(WorldTileMap map, BiomeGenerator generator, WorldTilemapRenderer renderer, PlayerGridMover targetPlayer)
        {
            worldTileMap = map;
            biomeGenerator = generator;
            tilemapRenderer = renderer;
            player = targetPlayer;
        }

        public void ForceRefresh()
        {
            lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
            chunkGenerationQueue.Clear();
            queuedChunks.Clear();
            RefreshAroundPlayer();
        }

        private void Update()
        {
            RefreshAroundPlayer();
        }

        private void RefreshAroundPlayer()
        {
            if (worldTileMap == null || biomeGenerator == null || tilemapRenderer == null || player == null)
            {
                return;
            }

            Vector2Int currentChunk = worldTileMap.WorldToChunk(player.CellPosition.x, player.CellPosition.y);
            if (currentChunk == lastPlayerChunk)
            {
                EnsureGenerationCoroutine();
                return;
            }

            lastPlayerChunk = currentChunk;
            RebuildGenerationQueue(currentChunk);
            tilemapRenderer.RenderAroundCell(player.CellPosition);
            EnsureGenerationCoroutine();
        }

        private void RebuildGenerationQueue(Vector2Int centerChunk)
        {
            chunkGenerationQueue.Clear();
            queuedChunks.Clear();
            candidateChunks.Clear();

            for (int cy = centerChunk.y - generationRadiusInChunks; cy <= centerChunk.y + generationRadiusInChunks; cy++)
            {
                for (int cx = centerChunk.x - generationRadiusInChunks; cx <= centerChunk.x + generationRadiusInChunks; cx++)
                {
                    if (cx < 0 || cy < 0 || cx >= worldTileMap.ChunkCountX || cy >= worldTileMap.ChunkCountY)
                    {
                        continue;
                    }

                    if (worldTileMap.IsChunkGenerated(cx, cy))
                    {
                        continue;
                    }

                    Vector2Int chunk = new Vector2Int(cx, cy);
                    if (queuedChunks.Add(chunk))
                    {
                        candidateChunks.Add(chunk);
                    }
                }
            }

            candidateChunks.Sort((left, right) =>
            {
                int leftDistance = ChunkDistanceSquared(left, centerChunk);
                int rightDistance = ChunkDistanceSquared(right, centerChunk);
                return leftDistance.CompareTo(rightDistance);
            });

            foreach (Vector2Int chunk in candidateChunks)
            {
                chunkGenerationQueue.Enqueue(chunk);
            }
        }

        private void EnsureGenerationCoroutine()
        {
            if (generationCoroutine == null && chunkGenerationQueue.Count > 0)
            {
                generationCoroutine = StartCoroutine(ProcessChunkGenerationQueue());
            }
        }

        private IEnumerator ProcessChunkGenerationQueue()
        {
            while (chunkGenerationQueue.Count > 0)
            {
                if (worldTileMap == null || biomeGenerator == null || tilemapRenderer == null || player == null)
                {
                    break;
                }

                int chunksPerFrame = Mathf.Max(1, chunksGeneratedPerFrame);
                int generatedThisFrame = 0;
                while (generatedThisFrame < chunksPerFrame && chunkGenerationQueue.Count > 0)
                {
                    Vector2Int chunk = chunkGenerationQueue.Dequeue();
                    queuedChunks.Remove(chunk);

                    if (!IsChunkInBounds(chunk) || worldTileMap.IsChunkGenerated(chunk.x, chunk.y))
                    {
                        continue;
                    }

                    biomeGenerator.GenerateChunk(worldTileMap, chunk.x, chunk.y);
                    tilemapRenderer.RenderChunkIfVisible(chunk, player.CellPosition);
                    generatedThisFrame++;
                }

                yield return null;
            }

            generationCoroutine = null;
        }

        private bool IsChunkInBounds(Vector2Int chunk)
        {
            return chunk.x >= 0
                && chunk.y >= 0
                && chunk.x < worldTileMap.ChunkCountX
                && chunk.y < worldTileMap.ChunkCountY;
        }

        private static int ChunkDistanceSquared(Vector2Int left, Vector2Int right)
        {
            int dx = left.x - right.x;
            int dy = left.y - right.y;
            return dx * dx + dy * dy;
        }
    }
}
