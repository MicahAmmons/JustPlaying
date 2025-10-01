using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.MapTile.MapTileSaveDatas;
using PlayingAround.Debug;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Proximity;
using PlayingAround.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace PlayingAround.Managers.Tiles
{
    public static class TileManager
    {
        private static Dictionary<string, MapTile> tiles = new();
        public static MapTile CurrentMapTile { get; private set; }

        public static void Initialize( string id)
        {

            LoadMapTileById(id);
            MapTileTransitionManager.OnFadeToBlackComplete += (NextTileData data) =>
            {
                string nextId = $"{data.NextX}_{data.NextY}_{data.NextZ}";
                LoadMapTileById(nextId);
            };

        }
        public static void LoadMapTileById(string id)
        {
            if (tiles.TryGetValue(id, out var existingTile))
            {
                CurrentMapTile = existingTile;
                return;
            }

            // Try loading from disk
            string path = $"Data/MapTile/MapTile_{id}.json";
            MapTileData data = JsonLoader.LoadTileData(path);
  
            var tile = new MapTile(data);
            tiles[id] = tile; // ✅ Cache the tile by its ID


            CurrentMapTile = tile;
        }
        public static TileCell GetCell(Vector2 pos)
        {
            TileCell closest = null;
            float minDist = float.MaxValue;

            foreach (var cell in CurrentMapTile.AllValidCells)
            {
                Vector2 center = new(cell.X * MapTile.TileWidth, cell.Y * MapTile.TileHeight);

                if (IsPointInDiamond(pos, center, MapTile.TileWidth / 2, MapTile.TileHeight / 2))
                {
                    return cell; // Perfect match
                }

                float dist = Vector2.DistanceSquared(pos, center);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = cell;
                }
            }

            return closest;
        }
        private static bool IsPointInDiamond(Vector2 point, Vector2 center, int halfWidth, int halfHeight)
        {
            float dx = Math.Abs(point.X - center.X);
            float dy = Math.Abs(point.Y - center.Y);
            return (dx / halfWidth + dy / halfHeight) <= 1f;
        }
        public static Vector2 OffSetFromCenterOfDiamond(Vector2 center, int width = 64, int height = 64)
        {

            int xOffset = width / 2;
            int yOffset = height /2;

            return new Vector2(center.X - xOffset, center.Y - yOffset);
        }
        public static List<TileCell> GetWalkableNeighbors(TileCell cell)
        {
            List<TileCell> neighbors = new();

            Point[] directions = new Point[]
            {
        new(1, 1),    // down-right
        new(1, -1),   // up-right
        new(-1, 1),   // down-left
        new(-1, -1)   // up-left
            };

            foreach (Point dir in directions)
            {
                int newX = cell.X + dir.X;
                int newY = cell.Y + dir.Y;

                if (newX < 0 || newY < 0 || newX >= MapTile.GridWidth || newY >= MapTile.GridHeight)
                    continue;

                TileCell neighbor = CurrentMapTile.AllValidCells.FirstOrDefault(c => c.X == newX && c.Y == newY);

                if (neighbor != null && neighbor.IsWalkable)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }
        public static bool IsNeighbor(TileCell target, TileCell current)
        {

                int dx = target.X - current.X;
                int dy = target.Y - current.Y;

                // Valid neighbors are diagonally adjacent: (±1, ±1)
                if (Math.Abs(dx) == 1 && Math.Abs(dy) == 1)
                    return true;

            return false;
        }
        public static Vector2 DirectionTraveledForNewMapTile(NextTileData data)
        {
            int currentTileX = CurrentMapTile.x;
            int currentTileY = CurrentMapTile.y;
            int nextTileX = data.NextX;
            int nextTileY = data.NextY;

            int dx = nextTileX - currentTileX;
            int dy = nextTileY - currentTileY;

            dx = Math.Clamp(dx, -1, 1);
            dy = Math.Clamp(dy, -1, 0);

            return new Vector2(dx, dy);
        }
        public static List<TileCell> GetFloodFillTileWithinRange(TileCell origin, int maxSteps)
        {
            List<TileCell> inRangeCells = new();
            Queue<(TileCell cell, int steps)> queue = new();
            HashSet<TileCell> visited = new();

            queue.Enqueue((origin, 0));
            visited.Add(origin);

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();

                if (steps > maxSteps)
                    continue;

                inRangeCells.Add(current);

                Point[] directions = new Point[]
                {
            new(1, 1),   // down-right
            new(1, -1),  // up-right
            new(-1, 1),  // down-left
            new(-1, -1)  // up-left
                };

                foreach (Point dir in directions)
                {
                    int newX = current.X + dir.X;
                    int newY = current.Y + dir.Y;

                    if (newX < 0 || newY < 0 || newX >= MapTile.GridWidth || newY >= MapTile.GridHeight)
                        continue;

                    TileCell neighbor = CurrentMapTile.AllValidCells.FirstOrDefault(c => c.X == newX && c.Y == newY);
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, steps + 1));
                }
            }

            inRangeCells.Remove(origin); // Optional: if you want to exclude the origin

            return inRangeCells;
        }
        public static MapTileSaveData SaveMapTile()
        {
            return new MapTileSaveData
            {
                CurrentTileId = CurrentMapTile.Id
            };
        }
        internal static int CheckManhattanDistance(TileCell origin, TileCell destination)
        {
            int dx = Math.Abs(origin.X - destination.X);
            int dy = Math.Abs(origin.Y - destination.Y);
            return Math.Max(dx, dy);
        }
        public static List<TileCell> GetReachableCellsFromSubset(TileCell start, List<TileCell> cellSubset, int range)
        {
            List<TileCell> reachableCells = new();
            Queue<(TileCell cell, int steps)> queue = new();
            HashSet<TileCell> visited = new();

            HashSet<TileCell> validCells = new(cellSubset);

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();

                if (steps > range)
                    continue;

                reachableCells.Add(current);

                Point[] directions = new Point[]
                {
            new(1, 1),   // down-right
            new(1, -1),  // up-right
            new(-1, 1),  // down-left
            new(-1, -1)  // up-left
                };

                foreach (Point dir in directions)
                {
                    int newX = current.X + dir.X;
                    int newY = current.Y + dir.Y;

                    TileCell neighbor = validCells.FirstOrDefault(c => c.X == newX && c.Y == newY);
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, steps + 1));
                }
            }
            reachableCells.Remove(start);
            return reachableCells;
        }
        public static bool DoesCellAlreadyContainPlayerMon(TileCell cell)
        {
            foreach (var mon in CurrentMapTile.PlayMonstersList)
            {
                if (mon.CurrentCell != cell) continue;
                return true;
            }
            return false;
        }
        public static void DrawBackground(SpriteBatch spriteBatch)
        {
            foreach (var text in CurrentMapTile.BackgroundBuidldOrder)
            {
                spriteBatch.Draw(text, Vector2.Zero, Color.White);
            }
        }
        internal static void DrawBackgroundSmoke(SpriteBatch spriteBatch, Effect fx)
        {
            var e = CurrentMapTile.BackgroundSmokeTexture;
            fx.Parameters["Frequency"].SetValue(e.FrequencyVec);
            fx.Parameters["Speed"].SetValue(e.SpeedVec);
            fx.Parameters["DistortAmount"].SetValue(e.DistortAmount);
            fx.Parameters["Opacity"].SetValue(e.Opacity);

            var tex = AssetManager.GetTexture("BackgroundSmoke");
            spriteBatch.Draw(tex, Vector2.Zero, Color.White);
        }
    }
}

