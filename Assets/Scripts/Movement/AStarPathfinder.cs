using System.Collections.Generic;
using UnityEngine;
using TileMap.World;
using TileMap.Utility;

namespace TileMap.Movement
{
    public class AStarPathfinder : MonoBehaviour
    {
        [SerializeField] private WorldTileMap worldTileMap;
        [SerializeField] private int maxExpandedNodes = 4096;
        [SerializeField] private bool allowDiagonalMovement;

        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private static readonly Vector2Int[] DiagonalDirections =
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };

        public void Initialize(WorldTileMap map)
        {
            worldTileMap = map;
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var result = new List<Vector2Int>();
            if (worldTileMap == null || !worldTileMap.IsInBounds(goal.x, goal.y))
            {
                return result;
            }

            if (start == goal)
            {
                result.Add(start);
                return result;
            }

            var open = new List<NodeRecord>(128);
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int>();
            var closed = new HashSet<Vector2Int>();

            gScore[start] = 0;
            open.Add(new NodeRecord(start, NoiseUtility.ManhattanDistance(start, goal)));
            int expanded = 0;

            while (open.Count > 0 && expanded < maxExpandedNodes)
            {
                int bestIndex = 0;
                for (int i = 1; i < open.Count; i++)
                {
                    if (open[i].fScore < open[bestIndex].fScore)
                    {
                        bestIndex = i;
                    }
                }

                NodeRecord current = open[bestIndex];
                open.RemoveAt(bestIndex);
                expanded++;

                if (current.position == goal)
                {
                    return ReconstructPath(cameFrom, current.position);
                }

                if (!closed.Add(current.position))
                {
                    continue;
                }

                EvaluateNeighbors(CardinalDirections, current.position, goal, open, closed, cameFrom, gScore);
                if (allowDiagonalMovement)
                {
                    EvaluateNeighbors(DiagonalDirections, current.position, goal, open, closed, cameFrom, gScore);
                }
            }

            return result;
        }

        private void EvaluateNeighbors(
            Vector2Int[] directions,
            Vector2Int current,
            Vector2Int goal,
            List<NodeRecord> open,
            HashSet<Vector2Int> closed,
            Dictionary<Vector2Int, Vector2Int> cameFrom,
            Dictionary<Vector2Int, int> gScore)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int next = current + directions[i];
                if (closed.Contains(next) || !worldTileMap.IsInBounds(next.x, next.y))
                {
                    continue;
                }

                bool isGoal = next == goal;
                if (!worldTileMap.IsWalkable(next.x, next.y, ignoreOccupancy: isGoal))
                {
                    continue;
                }

                var tileDef = worldTileMap.TileDatabase.Get(worldTileMap.GetTile(next.x, next.y));
                int moveCost = tileDef != null ? Mathf.Max(tileDef.moveCost, (byte)1) : 100;
                int tentativeG = gScore[current] + moveCost;
                if (gScore.TryGetValue(next, out int existingG) && tentativeG >= existingG)
                {
                    continue;
                }

                cameFrom[next] = current;
                gScore[next] = tentativeG;
                int heuristic = NoiseUtility.ManhattanDistance(next, goal) * 100;
                int fScore = tentativeG + heuristic;

                bool alreadyOpen = false;
                for (int openIndex = 0; openIndex < open.Count; openIndex++)
                {
                    if (open[openIndex].position == next)
                    {
                        open[openIndex] = new NodeRecord(next, fScore);
                        alreadyOpen = true;
                        break;
                    }
                }

                if (!alreadyOpen)
                {
                    open.Add(new NodeRecord(next, fScore));
                }
            }
        }

        private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private struct NodeRecord
        {
            public Vector2Int position;
            public int fScore;

            public NodeRecord(Vector2Int position, int fScore)
            {
                this.position = position;
                this.fScore = fScore;
            }
        }
    }
}
