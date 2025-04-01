using System;
using System.Collections.Generic;
using PlayingAround.Game.Map;
using Microsoft.Xna.Framework;

namespace PlayingAround.Game.Pathfinding
{
    public static class AStarPathfinder
    {
        public static PathNode[,] BuildNodeGrid(TileCell[,] tileGrid)
        {
            int width = tileGrid.GetLength(0);
            int height = tileGrid.GetLength(1);

            var nodeGrid = new PathNode[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileCell cell = tileGrid[x, y];
                    nodeGrid[x, y] = new PathNode(x, y, cell.IsWalkable);
                }
            }

            return nodeGrid;
        }

        public static List<Vector2> FindPath(TileCell[,] tileGrid, int startX, int startY, int endX, int endY)
        {
            var nodeGrid = BuildNodeGrid(tileGrid);
            var startNode = nodeGrid[startX, startY];
            var endNode = nodeGrid[endX, endY];

            var openSet = new List<PathNode> { startNode };
            var closedSet = new HashSet<PathNode>();

            while (openSet.Count > 0)
            {
                // Find node with lowest F cost
                PathNode current = openSet[0];
                foreach (var node in openSet)
                {
                    if (node.FCost < current.FCost || (node.FCost == current.FCost && node.HCost < current.HCost))
                        current = node;
                }

                if (current == endNode)
                {
                    return RetracePath(startNode, endNode);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current, nodeGrid))
                {
                    if (!neighbor.Walkable || closedSet.Contains(neighbor))
                        continue;

                    int tentativeG = current.GCost + GetDistance(current, neighbor);
                    if (tentativeG < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = tentativeG;
                        neighbor.HCost = GetDistance(neighbor, endNode);
                        neighbor.Parent = current;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null; // no path found
        }

        private static List<Vector2> RetracePath(PathNode start, PathNode end)
        {
            var path = new List<Vector2>();
            PathNode current = end;

            while (current != start)
            {
                path.Add(new Vector2(current.X, current.Y));
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        private static int GetDistance(PathNode a, PathNode b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return dx + dy; // Manhattan distance
        }

        private static List<PathNode> GetNeighbors(PathNode node, PathNode[,] grid)
        {
            var neighbors = new List<PathNode>();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            int[,] directions = new int[,]
            {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } // Up, Down, Left, Right
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int nx = node.X + directions[i, 0];
                int ny = node.Y + directions[i, 1];

                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    neighbors.Add(grid[nx, ny]);
                }
            }

            return neighbors;
        }
    }
}
