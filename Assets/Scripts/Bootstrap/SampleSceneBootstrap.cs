using UnityEngine;
using UnityEngine.Tilemaps;
using TileMap.Data;
using TileMap.Generation;
using TileMap.Movement;
using TileMap.Rendering;
using TileMap.Utility;
using TileMap.World;

namespace TileMap.Bootstrap
{
    public class SampleSceneBootstrap : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private int worldWidth = 10000;
        [SerializeField] private int worldHeight = 10000;
        [SerializeField] private int chunkSize = 64;
        [SerializeField] private int pixelsPerUnit = 16;
        [SerializeField] private int renderRadiusInChunks = 2;
        [SerializeField] private AutoTilePalette autoTilePalette;

        private void Awake()
        {
            CreateScene();
        }

        private void CreateScene()
        {
            TileDatabase database = SampleTileDatabaseBuilder.CreateRuntimeDatabase();

            var gridObject = new GameObject("Grid", typeof(Grid));
            var groundObject = new GameObject("GroundTilemap", typeof(Tilemap), typeof(TilemapRenderer));
            groundObject.transform.SetParent(gridObject.transform, false);
            var groundTilemap = groundObject.GetComponent<Tilemap>();
            var groundRenderer = groundObject.GetComponent<TilemapRenderer>();
            groundRenderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            groundRenderer.sortingOrder = 0;

            var overlayObject = new GameObject("OverlayTilemap", typeof(Tilemap), typeof(TilemapRenderer));
            overlayObject.transform.SetParent(gridObject.transform, false);
            var overlayTilemap = overlayObject.GetComponent<Tilemap>();
            var overlayRenderer = overlayObject.GetComponent<TilemapRenderer>();
            overlayRenderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            overlayRenderer.sortingOrder = 10;

            var worldObject = new GameObject("WorldRoot");
            var worldTileMap = worldObject.AddComponent<WorldTileMap>();
            worldTileMap.Initialize(worldWidth, worldHeight, chunkSize, database);

            var biomeGenerator = worldObject.AddComponent<BiomeGenerator>();
            var debugPalette = worldObject.AddComponent<RuntimeDebugTilePalette>();
            var worldRenderer = worldObject.AddComponent<WorldTilemapRenderer>();
            AutoTilePalette resolvedAutoTilePalette = ResolveAutoTilePalette();
            worldRenderer.Initialize(worldTileMap, groundTilemap, overlayTilemap, debugPalette, renderRadiusInChunks, resolvedAutoTilePalette);

            var playerObject = new GameObject("Player", typeof(SpriteRenderer));
            playerObject.transform.position = Vector3.zero;
            var playerRenderer = playerObject.GetComponent<SpriteRenderer>();
            playerRenderer.sprite = RuntimeSpriteFactory.GetSolidSprite(new Color32(255, 235, 84, 255));
            playerRenderer.drawMode = SpriteDrawMode.Simple;
            playerRenderer.sortingOrder = 100;
            playerObject.transform.localScale = Vector3.one * 0.85f;

            var playerMover = playerObject.AddComponent<PlayerGridMover>();
            playerMover.Initialize(worldTileMap);

            var pathfinder = playerObject.AddComponent<AStarPathfinder>();
            pathfinder.Initialize(worldTileMap);

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraObject = new GameObject("Main Camera", typeof(Camera));
                mainCamera = cameraObject.GetComponent<Camera>();
                mainCamera.tag = "MainCamera";
            }
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 12f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color32(18, 18, 24, 255);

            var cameraFollow = mainCamera.GetComponent<CameraFollow2D>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow2D>();
            }
            cameraFollow.SetTarget(playerObject.transform);

            var tileIdDebugOverlay = mainCamera.GetComponent<TileIdDebugOverlay>();
            if (tileIdDebugOverlay == null)
            {
                tileIdDebugOverlay = mainCamera.gameObject.AddComponent<TileIdDebugOverlay>();
            }
            tileIdDebugOverlay.Initialize(mainCamera, worldTileMap);

            var worldOverviewRenderer = mainCamera.GetComponent<WorldOverviewRenderer>();
            if (worldOverviewRenderer == null)
            {
                worldOverviewRenderer = mainCamera.gameObject.AddComponent<WorldOverviewRenderer>();
            }
            worldOverviewRenderer.Initialize(worldTileMap, biomeGenerator, playerMover);

            var inputController = playerObject.AddComponent<TapToMoveController>();
            inputController.Initialize(mainCamera, worldTileMap, playerMover, pathfinder);

            var streamingController = worldObject.AddComponent<WorldStreamingController>();
            streamingController.Initialize(worldTileMap, biomeGenerator, worldRenderer, playerMover);

            Vector2Int spawn = biomeGenerator.GetRecommendedSpawnCell(worldTileMap);
            Vector2Int spawnChunk = worldTileMap.WorldToChunk(spawn.x, spawn.y);
            biomeGenerator.GenerateChunk(worldTileMap, spawnChunk.x, spawnChunk.y);

            playerMover.SetCellPositionInstant(spawn);
            worldRenderer.RenderAroundCell(spawn);
            streamingController.ForceRefresh();
        }

        private AutoTilePalette ResolveAutoTilePalette()
        {
            if (autoTilePalette != null)
            {
                return autoTilePalette;
            }

            AutoTilePalette resourcePalette = Resources.Load<AutoTilePalette>("AutoTilePalette/AutoTilePalette");
            if (resourcePalette == null)
            {
                Debug.LogWarning("AutoTilePalette is not assigned. Falling back to runtime debug color tiles.");
                return null;
            }

            autoTilePalette = resourcePalette;
            return autoTilePalette;
        }
    }
}
