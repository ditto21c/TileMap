using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMap.Data;
using TileMap.World;

namespace TileMap.Movement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerGridMover : MonoBehaviour
    {
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private float baseMoveDuration = 0.12f;
        [SerializeField] private int occupantId = 1;

        private readonly Queue<Vector2Int> queuedPath = new Queue<Vector2Int>();
        public Vector2Int CellPosition { get; private set; }
        public bool IsMoving { get; private set; }

        public void Initialize(WorldTileMap map)
        {
            worldTileMap = map;
        }

        public void SetCellPositionInstant(Vector2Int cell)
        {
            if (worldTileMap == null)
            {
                CellPosition = cell;
                transform.position = CellToWorld(cell);
                return;
            }

            if (worldTileMap.IsInBounds(CellPosition.x, CellPosition.y))
            {
                worldTileMap.ClearOccupied(CellPosition.x, CellPosition.y);
            }

            CellPosition = cell;
            worldTileMap.SetOccupied(cell.x, cell.y, occupantId);
            transform.position = CellToWorld(cell);
            queuedPath.Clear();
            IsMoving = false;
        }

        public bool TryMove(Vector2Int direction)
        {
            if (IsMoving || worldTileMap == null)
            {
                return false;
            }

            Vector2Int target = CellPosition + direction;
            if (!worldTileMap.IsWalkable(target.x, target.y))
            {
                return false;
            }

            StartCoroutine(MoveRoutine(target));
            return true;
        }

        public void SetPath(IReadOnlyList<Vector2Int> path)
        {
            queuedPath.Clear();
            if (path == null || path.Count <= 1)
            {
                return;
            }

            for (int i = 1; i < path.Count; i++)
            {
                queuedPath.Enqueue(path[i]);
            }
        }

        public void StopPath()
        {
            queuedPath.Clear();
        }

        private void Update()
        {
            if (IsMoving || queuedPath.Count == 0)
            {
                return;
            }

            Vector2Int next = queuedPath.Peek();
            Vector2Int direction = next - CellPosition;
            if (direction.sqrMagnitude > 2)
            {
                queuedPath.Clear();
                return;
            }

            if (!TryMove(direction))
            {
                queuedPath.Clear();
            }
            else
            {
                queuedPath.Dequeue();
            }
        }

        private IEnumerator MoveRoutine(Vector2Int target)
        {
            IsMoving = true;

            TileDefinition groundTileDef = worldTileMap.GetTileDefinition(target.x, target.y, TileLayer.Ground);
            TileDefinition overlayTileDef = worldTileMap.GetTileDefinition(target.x, target.y, TileLayer.Overlay);
            float moveCostScale = Mathf.Max(worldTileMap.GetMoveCost(target.x, target.y) / 100f, 0.25f);
            float duration = baseMoveDuration * moveCostScale;

            Vector3 start = transform.position;
            Vector3 end = CellToWorld(target);
            Vector2Int previous = CellPosition;

            worldTileMap.ClearOccupied(previous.x, previous.y);
            worldTileMap.SetOccupied(target.x, target.y, occupantId);
            CellPosition = target;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;
            ApplyTileEffect(groundTileDef);
            ApplyTileEffect(overlayTileDef);
            IsMoving = false;
        }

        private void ApplyTileEffect(TileDefinition tileDef)
        {
            if (tileDef == null)
            {
                return;
            }

            if (tileDef.damageType == 1)
            {
                Debug.Log($"Poison tile entered: {tileDef.tileName}, damage {tileDef.damageValue}");
            }
            else if (tileDef.damageType == 2)
            {
                Debug.Log($"Fire tile entered: {tileDef.tileName}, damage {tileDef.damageValue}");
            }
        }

        private static Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
        }
    }
}
