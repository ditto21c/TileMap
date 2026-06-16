using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TileMap.World;

namespace TileMap.Movement
{
    public class TapToMoveController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private AStarPathfinder pathfinder;
        [SerializeField] private bool enableKeyboardFallback = true;

        public void Initialize(Camera cameraRef, WorldTileMap map, PlayerGridMover mover, AStarPathfinder pathfinderRef)
        {
            targetCamera = cameraRef;
            worldTileMap = map;
            playerMover = mover;
            pathfinder = pathfinderRef;
        }

        private void Update()
        {
            if (enableKeyboardFallback)
            {
                HandleKeyboard();
            }

            if (TryHandleTouch())
            {
                return;
            }

            HandleMouse();
        }

        private void HandleKeyboard()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || playerMover == null || playerMover.IsMoving)
            {
                return;
            }

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                playerMover.StopPath();
                playerMover.TryMove(Vector2Int.up);
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                playerMover.StopPath();
                playerMover.TryMove(Vector2Int.down);
            }
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                playerMover.StopPath();
                playerMover.TryMove(Vector2Int.left);
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                playerMover.StopPath();
                playerMover.TryMove(Vector2Int.right);
            }
        }

        private bool TryHandleTouch()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return false;
            }

            var touch = touchscreen.primaryTouch;
            if (!touch.press.wasPressedThisFrame)
            {
                return false;
            }

            TryRequestPath(touch.position.ReadValue());
            return true;
        }

        private void HandleMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            TryRequestPath(mouse.position.ReadValue());
        }

        private void TryRequestPath(Vector2 screenPosition)
        {
            if (targetCamera == null || worldTileMap == null || playerMover == null || pathfinder == null)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -targetCamera.transform.position.z));
            Vector2Int targetCell = new Vector2Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y));

            if (!worldTileMap.IsInBounds(targetCell.x, targetCell.y))
            {
                return;
            }

            var path = pathfinder.FindPath(playerMover.CellPosition, targetCell);
            if (path.Count > 0)
            {
                playerMover.SetPath(path);
            }
        }
    }
}
