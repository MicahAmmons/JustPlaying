using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingAround.Managers.Movement
{
    public static class GridMovement
    {
        private const int MaxSteps = 500;
        private const int ORTHOGONAL_COST = 10;
        private const int DIAGONAL_COST = 14;

        public static List<Vector2> BuildVectorPath(Vector2 currentPos, Vector2 destinationPoint)
        {
            TileCell startingCell = TileManager.GetCell(currentPos);
            TileCell endCell = TileManager.GetCell(destinationPoint);

            // Fallbacks if we can't resolve cells
            if (startingCell == null || endCell == null)
                return new List<Vector2> { currentPos, destinationPoint };

            // 1) Get the raw (valid) cell path (zig-zag)
            List<TileCell> cellPath = GetCellToCellPath(currentPos, destinationPoint);
            if (cellPath == null || cellPath.Count <= 1)
                return new List<Vector2> { currentPos, destinationPoint };

            // 2) Smooth the path with a line-of-sight "string pulling" pass
            //    If the whole corridor is clear, this will collapse to [startCenter, endCenter].
            List<Vector2> smoothedWaypoints = ComputeSmoothedWaypoints(cellPath);

            // 3) Expand waypoints into a fine-grained pixel path (using your existing helper)
            List<Vector2> vectorPath = new List<Vector2>();
            for (int i = 0; i < smoothedWaypoints.Count - 1; i++)
            {
                var a = smoothedWaypoints[i];
                var b = smoothedWaypoints[i + 1];
                var seg = BuildStraightLinePath(a, b);

                // Avoid duplicating the junction point
                if (i > 0 && seg.Count > 0) seg.RemoveAt(0);
                vectorPath.AddRange(seg);
            }

            // 4) Deduplicate consecutive identical points
            if (vectorPath.Count > 1)
            {
                var dedup = new List<Vector2>(vectorPath.Count);
                dedup.Add(vectorPath[0]);
                for (int i = 1; i < vectorPath.Count; i++)
                {
                    if (vectorPath[i] != dedup[^1]) dedup.Add(vectorPath[i]);
                }
                vectorPath = dedup;
            }

            // 5) Apply your start/end offset blending so endpoints match exact pixels
            Vector2 startingOffset = startingCell.CenterPoint - currentPos;
            Vector2 endingOffset = endCell.CenterPoint - destinationPoint;

            int n = vectorPath.Count;
            if (n == 0) return new List<Vector2> { currentPos, destinationPoint };

            for (int i = 0; i < n; i++)
            {
                float t = (n == 1) ? 1f : (i / (float)(n - 1));
                Vector2 offset = Vector2.Lerp(startingOffset, endingOffset, t);
                vectorPath[i] -= offset;
            }

            // 6) Lock exact endpoints (avoid float drift)
            vectorPath[0] = currentPos;
            vectorPath[^1] = destinationPoint;

            return vectorPath;
        }
        private static List<Vector2> ComputeSmoothedWaypoints(List<TileCell> cellPath, float sampleStepPixels = 16f, bool forbidCornerCutting = true)
        {
            var waypoints = new List<Vector2>();
            if (cellPath == null || cellPath.Count == 0)
                return waypoints;

            int anchorIndex = 0;
            waypoints.Add(cellPath[anchorIndex].CenterPoint);

            while (anchorIndex < cellPath.Count - 1)
            {
                int furthestVisible = anchorIndex + 1;

                // Scan forward as far as we can see from the current anchor
                for (int j = anchorIndex + 1; j < cellPath.Count; j++)
                {
                    Vector2 a = cellPath[anchorIndex].CenterPoint;
                    Vector2 b = cellPath[j].CenterPoint;

                    if (HasLineOfSight(a, b, sampleStepPixels, forbidCornerCutting))
                        furthestVisible = j;
                    else
                        break;
                }

                // If we can see to the end, place final waypoint and stop
                if (furthestVisible == cellPath.Count - 1)
                {
                    waypoints.Add(cellPath[furthestVisible].CenterPoint);
                    break;
                }

                // Otherwise, step to the last visible cell and continue
                waypoints.Add(cellPath[furthestVisible].CenterPoint);
                anchorIndex = furthestVisible;
            }

            // Safety: ensure end is included
            if (waypoints[^1] != cellPath[^1].CenterPoint)
                waypoints.Add(cellPath[^1].CenterPoint);

            return waypoints;
        }

        /// <summary>
        /// Checks straight-line visibility between pixel points a->b by sampling along the segment.
        /// Rejects if any sampled point falls in a non-walkable cell.
        /// Optionally forbids "corner cutting" by checking 4-neighborhood when crossing diagonals.
        /// </summary>
        private static bool HasLineOfSight(Vector2 a, Vector2 b, float stepPixels = 16f, bool forbidCornerCutting = true)
        {
            Vector2 delta = b - a;
            float dist = delta.Length();
            if (dist <= float.Epsilon) return true;

            Vector2 dir = delta / dist;
            int steps = Math.Max(1, (int)Math.Ceiling(dist / Math.Max(1f, stepPixels)));

            // Sample along the line (including end)
            TileCell prevCell = null;
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = (i == steps) ? b : a + dir * (i * (dist / steps));
                TileCell c = TileManager.GetCell(p);
                if (c == null || !c.IsWalkable) return false;

                if (forbidCornerCutting && prevCell != null)
                {
                    // If we moved diagonally between cells that share only a corner,
                    // ensure at least one of the orthogonal neighbors is open to avoid squeezing.
                    // This is a conservative check; adapt to your grid/neighbors if needed.
                    if (MovedDiagonally(prevCell, c))
                    {
                        // Check that we are not cutting through two blocking orthogonals.
                        // You’ll need a way to query neighbors; this is a safe fallback:
                        // sample a midpoint slightly nudged toward X and toward Y.
                        // If either nudged sample is non-walkable, consider it a corner cut.
                        Vector2 mid = (p + (i == 0 ? a : a + dir * ((i - 1) * (dist / steps)))) * 0.5f;
                        Vector2 nudgeX = mid + new Vector2(stepPixels * 0.5f * Math.Sign(dir.X), 0);
                        Vector2 nudgeY = mid + new Vector2(0, stepPixels * 0.5f * Math.Sign(dir.Y));

                        var cx = TileManager.GetCell(nudgeX);
                        var cy = TileManager.GetCell(nudgeY);
                        if ((cx != null && !cx.IsWalkable) && (cy != null && !cy.IsWalkable))
                            return false;
                    }
                }

                prevCell = c;
            }

            return true;
        }
        private static bool MovedDiagonally(TileCell a, TileCell b)
        {
            // Fallback: compare center deltas; any non-zero in both axes implies diagonal step.
            var d = b.CenterPoint - a.CenterPoint;
            return Math.Abs(d.X) > float.Epsilon && Math.Abs(d.Y) > float.Epsilon;
        }
        public static List<TileCell> BestPathToClosestCell(TileCell current, List<TileCell> targets, int maxSteps)
        {
            List<TileCell> bestPath = null;
            int shortestPathLength = 30;

            foreach (var target in targets)
            {
                List<TileCell> path = GetCellToCellPath(current.CenterPoint, target.CenterPoint);
                if (path.Count > 0 && path.Count < shortestPathLength)
                {
                    shortestPathLength = path.Count;
                    bestPath = path;
                    if (bestPath.Contains(target)) bestPath.Remove(target);
                    if (bestPath.Contains(current)) bestPath.Remove(current);
                }
            }
            
            return bestPath.Count > maxSteps ? bestPath.Take(maxSteps).ToList() : bestPath;
        }
        public static List<TileCell> GetCellToCellPath(Vector2 startPixel, Vector2 endPixel)
        {
            TileCell startCell = TileManager.GetCell(startPixel);
            TileCell goalCell = TileManager.GetCell(endPixel);

            if (startCell == null || goalCell == null)
                return new();

            List<TileCell> cellPath = FindCellPath(startCell, goalCell);

            return cellPath.ToList();
        }

        // This takes into consideration fi the cell is walkable or not
        public static List<Vector2> BuildStraightLinePath(Vector2 start, Vector2 end, int stepSize = 15)
        {
            var path = new List<Vector2>();

            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0)
                return path;

            direction.Normalize();
            int steps = (int)(distance / stepSize);

            for (int i = 0; i <= steps; i++)
            {
                Vector2 current = start + direction * (i * stepSize);

                var cell = TileManager.GetCell(current);
                if (cell == null || !cell.IsWalkable)
                    break;

                path.Add(current);
            }

            return path;
        }
        // This will provide all vectors - doesn't take into consideration walkable or not
        public static List<Vector2> BuildStraightLinePath(Vector2 start, Vector2 end, bool all, int stepSize = 15)
        {
            var path = new List<Vector2>();

            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0)
                return path;

            direction.Normalize();
            int steps = (int)(distance / stepSize);

            for (int i = 0; i <= steps; i++)
            {
                Vector2 current = start + direction * (i * stepSize);

                var cell = TileManager.GetCell(current);
                path.Add(current);
            }

            return path;
        }

        private static List<TileCell> FindCellPath(TileCell start, TileCell goal)
        {
            var openSet = new PriorityQueue<TileCell, int>();
            var cameFrom = new Dictionary<TileCell, TileCell>();
            var gScore = new Dictionary<TileCell, int>();
            var fScore = new Dictionary<TileCell, int>();

            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);

            openSet.Enqueue(start, fScore[start]);

            int steps = 0;

            while (openSet.Count > 0 && steps++ < MaxSteps)
            {
                var current = openSet.Dequeue();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                foreach (var neighbor in TileManager.GetWalkableNeighbors(current))
                {
                    if (neighbor.BlockedByCombatant && neighbor != goal) 
                        continue;
                    int moveCost = IsDiagonal(current, neighbor) ? DIAGONAL_COST : ORTHOGONAL_COST;
                    int tentativeG = gScore[current] + moveCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return new(); // no path
        }
        private static List<TileCell> ReconstructPath(Dictionary<TileCell, TileCell> cameFrom, TileCell current)
        {
            var path = new List<TileCell> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
        private static int Heuristic(TileCell a, TileCell b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return 10 * Math.Max(dx, dy); // Chebyshev distance
        }
        private static bool IsDiagonal(TileCell a, TileCell b)
        {
            return Math.Abs(a.X - b.X) == 1 && Math.Abs(a.Y - b.Y) == 1;
        }
    }
}
