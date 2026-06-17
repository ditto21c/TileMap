using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TileMap.Data;
using TileMap.Generation;
using TileMap.Movement;
using TileMap.World;

namespace TileMap.Rendering
{
    public class WorldOverviewRenderer : MonoBehaviour
    {
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private BiomeGenerator biomeGenerator;
        [SerializeField] private PlayerGridMover player;
        [SerializeField] private int textureMaxSize = 1024;
        [SerializeField] private int rowsPerFrame = 16;

        private Texture2D overviewTexture;
        private Coroutine buildRoutine;
        private bool visible;
        private bool isBuilding;
        private float buildProgress;
        private GUIStyle titleStyle;
        private GUIStyle statusStyle;

        public void Initialize(WorldTileMap map, BiomeGenerator generator, PlayerGridMover playerMover)
        {
            worldTileMap = map;
            biomeGenerator = generator;
            player = playerMover;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.mKey.wasPressedThisFrame)
            {
                return;
            }

            visible = !visible;
            if (visible && overviewTexture == null && !isBuilding)
            {
                buildRoutine = StartCoroutine(BuildOverviewRoutine());
            }
        }

        private void OnDestroy()
        {
            if (buildRoutine != null)
            {
                StopCoroutine(buildRoutine);
                buildRoutine = null;
            }

            if (overviewTexture != null)
            {
                Destroy(overviewTexture);
                overviewTexture = null;
            }
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            EnsureStyles();

            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            Rect mapRect = GetMapRect();
            Rect panelRect = new Rect(mapRect.x - 18f, mapRect.y - 54f, mapRect.width + 36f, mapRect.height + 72f);

            GUI.color = new Color(0.06f, 0.07f, 0.08f, 0.94f);
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 12f, panelRect.width - 32f, 28f), "World Map", titleStyle);

            if (overviewTexture != null)
            {
                GUI.DrawTexture(mapRect, overviewTexture, ScaleMode.StretchToFill, false);
                DrawPlayerMarker(mapRect);
            }
            else
            {
                GUI.color = new Color(0.13f, 0.14f, 0.16f, 1f);
                GUI.DrawTexture(mapRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            if (isBuilding)
            {
                DrawBuildProgress(panelRect);
            }
        }

        private IEnumerator BuildOverviewRoutine()
        {
            if (worldTileMap == null || biomeGenerator == null)
            {
                yield break;
            }

            isBuilding = true;
            buildProgress = 0f;

            int textureWidth;
            int textureHeight;
            CalculateTextureSize(out textureWidth, out textureHeight);

            if (overviewTexture != null)
            {
                Destroy(overviewTexture);
            }

            overviewTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                name = "WorldOverviewTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[textureWidth * textureHeight];
            int safeRowsPerFrame = Mathf.Max(1, rowsPerFrame);

            for (int y = 0; y < textureHeight; y++)
            {
                int worldY = Mathf.Clamp(
                    Mathf.FloorToInt((y + 0.5f) * worldTileMap.WorldHeight / textureHeight),
                    0,
                    worldTileMap.WorldHeight - 1);

                int rowOffset = y * textureWidth;
                for (int x = 0; x < textureWidth; x++)
                {
                    int worldX = Mathf.Clamp(
                        Mathf.FloorToInt((x + 0.5f) * worldTileMap.WorldWidth / textureWidth),
                        0,
                        worldTileMap.WorldWidth - 1);

                    biomeGenerator.SampleTileAt(worldTileMap, worldX, worldY, out byte groundTileId, out byte overlayTileId);
                    pixels[rowOffset + x] = ResolvePreviewColor(groundTileId, overlayTileId);
                }

                buildProgress = (y + 1f) / textureHeight;
                if (y % safeRowsPerFrame == 0)
                {
                    yield return null;
                }
            }

            overviewTexture.SetPixels32(pixels);
            overviewTexture.Apply(false, false);
            buildProgress = 1f;
            isBuilding = false;
            buildRoutine = null;
        }

        private void CalculateTextureSize(out int textureWidth, out int textureHeight)
        {
            int maxSize = Mathf.Clamp(textureMaxSize, 128, 2048);
            float aspect = worldTileMap.WorldWidth / (float)worldTileMap.WorldHeight;

            if (aspect >= 1f)
            {
                textureWidth = maxSize;
                textureHeight = Mathf.Max(1, Mathf.RoundToInt(maxSize / aspect));
            }
            else
            {
                textureHeight = maxSize;
                textureWidth = Mathf.Max(1, Mathf.RoundToInt(maxSize * aspect));
            }
        }

        private Rect GetMapRect()
        {
            float maxWidth = Screen.width * 0.86f;
            float maxHeight = Screen.height * 0.78f;
            float aspect = overviewTexture != null
                ? overviewTexture.width / (float)overviewTexture.height
                : worldTileMap != null
                    ? worldTileMap.WorldWidth / (float)worldTileMap.WorldHeight
                    : 1f;

            float width = maxWidth;
            float height = width / aspect;
            if (height > maxHeight)
            {
                height = maxHeight;
                width = height * aspect;
            }

            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f + 14f, width, height);
        }

        private void DrawPlayerMarker(Rect mapRect)
        {
            if (player == null || worldTileMap == null)
            {
                return;
            }

            float normalizedX = Mathf.Clamp01((player.CellPosition.x + 0.5f) / worldTileMap.WorldWidth);
            float normalizedY = Mathf.Clamp01((player.CellPosition.y + 0.5f) / worldTileMap.WorldHeight);
            float markerX = mapRect.x + normalizedX * mapRect.width;
            float markerY = mapRect.yMax - normalizedY * mapRect.height;

            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(markerX - 5f, markerY - 5f, 10f, 10f), Texture2D.whiteTexture);
            GUI.color = new Color32(255, 235, 84, 255);
            GUI.DrawTexture(new Rect(markerX - 3f, markerY - 3f, 6f, 6f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawBuildProgress(Rect panelRect)
        {
            Rect barBackground = new Rect(panelRect.x + 16f, panelRect.yMax - 28f, panelRect.width - 32f, 10f);
            Rect barFill = new Rect(barBackground.x, barBackground.y, barBackground.width * buildProgress, barBackground.height);

            GUI.color = new Color(0.16f, 0.17f, 0.19f, 1f);
            GUI.DrawTexture(barBackground, Texture2D.whiteTexture);
            GUI.color = new Color32(255, 235, 84, 255);
            GUI.DrawTexture(barFill, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(
                new Rect(panelRect.x + 16f, panelRect.yMax - 50f, panelRect.width - 32f, 18f),
                $"Generating {Mathf.RoundToInt(buildProgress * 100f)}%",
                statusStyle);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            statusStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                normal = { textColor = new Color(0.86f, 0.88f, 0.9f, 1f) }
            };
        }

        private static Color32 ResolvePreviewColor(byte groundTileId, byte overlayTileId)
        {
            Color groundColor = RuntimeDebugTilePalette.GetColor((TileId)groundTileId);
            if (overlayTileId == (byte)TileId.Void || overlayTileId == (byte)TileId.GrassEdgeOverlay)
            {
                return Opaque(groundColor);
            }

            Color overlayColor = RuntimeDebugTilePalette.GetColor((TileId)overlayTileId);
            if (overlayTileId == (byte)TileId.SpawnPlayer || overlayTileId == (byte)TileId.EndPortal)
            {
                return Opaque(overlayColor);
            }

            return Opaque(Color.Lerp(groundColor, overlayColor, 0.45f));
        }

        private static Color32 Opaque(Color color)
        {
            color.a = 1f;
            return color;
        }
    }
}
