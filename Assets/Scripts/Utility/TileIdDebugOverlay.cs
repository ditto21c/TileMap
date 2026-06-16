using UnityEngine;
using UnityEngine.InputSystem;
using TileMap.Data;
using TileMap.World;

namespace TileMap.Utility
{
    public class TileIdDebugOverlay : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private bool visible = true;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private int maxLabels = 1400;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.85f);

        private GUIStyle labelStyle;
        private GUIStyle shadowStyle;
        private GUIStyle panelStyle;

        public void Initialize(Camera cameraRef, WorldTileMap map)
        {
            targetCamera = cameraRef;
            worldTileMap = map;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null
                && (keyboard.f3Key.wasPressedThisFrame
                    || keyboard.digit3Key.wasPressedThisFrame
                    || keyboard.backquoteKey.wasPressedThisFrame))
            {
                visible = !visible;
            }
        }

        private void OnGUI()
        {
            if (!visible || targetCamera == null || worldTileMap == null)
            {
                return;
            }

            EnsureStyles();
            DrawVisibleTileIds();
            DrawMouseTilePanel();
        }

        private void DrawVisibleTileIds()
        {
            GetVisibleCellBounds(out int xMin, out int xMax, out int yMin, out int yMax);

            int width = Mathf.Max(1, xMax - xMin + 1);
            int height = Mathf.Max(1, yMax - yMin + 1);
            int step = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(width * height / (float)Mathf.Max(1, maxLabels))));

            for (int y = yMin; y <= yMax; y += step)
            {
                for (int x = xMin; x <= xMax; x += step)
                {
                    if (!worldTileMap.IsInBounds(x, y))
                    {
                        continue;
                    }

                    byte groundTileId = worldTileMap.GetGroundTile(x, y);
                    byte overlayTileId = worldTileMap.GetOverlayTile(x, y);
                    if (groundTileId == (byte)TileId.Void && overlayTileId == (byte)TileId.Void)
                    {
                        continue;
                    }

                    Vector3 screen = targetCamera.WorldToScreenPoint(new Vector3(x + 0.5f, y + 0.5f, 0f));
                    if (screen.z < 0f)
                    {
                        continue;
                    }

                    Rect labelRect = new Rect(screen.x - 34f, Screen.height - screen.y - 11f, 68f, 22f);
                    string label = FormatTileLabel(groundTileId, overlayTileId);
                    GUI.Label(Offset(labelRect, 1f, 1f), label, shadowStyle);
                    GUI.Label(labelRect, label, labelStyle);
                }
            }
        }

        private void DrawMouseTilePanel()
        {
            Vector2? mouseScreen = GetMouseScreenPosition();
            if (!mouseScreen.HasValue)
            {
                GUI.Label(new Rect(12f, 12f, 360f, 24f), "F3 TileId Overlay ON", panelStyle);
                return;
            }

            Vector2Int cell = ScreenToCell(mouseScreen.Value);
            if (!worldTileMap.IsInBounds(cell.x, cell.y))
            {
                GUI.Label(new Rect(12f, 12f, 420f, 24f), "F3 TileId Overlay ON | Mouse outside world", panelStyle);
                return;
            }

            byte groundTileId = worldTileMap.GetGroundTile(cell.x, cell.y);
            byte overlayTileId = worldTileMap.GetOverlayTile(cell.x, cell.y);
            string groundTileName = ((TileId)groundTileId).ToString();
            string overlayTileName = overlayTileId != (byte)TileId.Void ? ((TileId)overlayTileId).ToString() : "None";
            GUI.Label(
                new Rect(12f, 12f, 520f, 24f),
                $"F3 TileId Overlay ON | Cell ({cell.x}, {cell.y}) | Ground {groundTileId}:{groundTileName} | Overlay {overlayTileId}:{overlayTileName}",
                panelStyle);
        }

        private static string FormatTileLabel(byte groundTileId, byte overlayTileId)
        {
            if (overlayTileId == (byte)TileId.Void)
            {
                return groundTileId.ToString();
            }

            if (groundTileId == (byte)TileId.Void)
            {
                return "+" + overlayTileId;
            }

            return groundTileId + "+" + overlayTileId;
        }

        private void GetVisibleCellBounds(out int xMin, out int xMax, out int yMin, out int yMax)
        {
            float halfHeight = targetCamera.orthographicSize;
            float halfWidth = halfHeight * targetCamera.aspect;
            Vector3 cameraPosition = targetCamera.transform.position;

            xMin = Mathf.FloorToInt(cameraPosition.x - halfWidth) - 1;
            xMax = Mathf.CeilToInt(cameraPosition.x + halfWidth) + 1;
            yMin = Mathf.FloorToInt(cameraPosition.y - halfHeight) - 1;
            yMax = Mathf.CeilToInt(cameraPosition.y + halfHeight) + 1;
        }

        private Vector2Int ScreenToCell(Vector2 screenPosition)
        {
            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -targetCamera.transform.position.z));
            return new Vector2Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y));
        }

        private static Vector2? GetMouseScreenPosition()
        {
            var mouse = Mouse.current;
            return mouse != null ? mouse.position.ReadValue() : null;
        }

        private void EnsureStyles()
        {
            if (labelStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
            };

            shadowStyle = new GUIStyle(labelStyle)
            {
                normal = { textColor = shadowColor }
            };

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                normal = { textColor = Color.white }
            };
        }

        private static Rect Offset(Rect rect, float x, float y)
        {
            rect.x += x;
            rect.y += y;
            return rect;
        }
    }
}
