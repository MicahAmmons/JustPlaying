using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.MapTile;
using PlayingAround.Debug;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Proximity;
using PlayingAround.Utils;

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
                
                PlayerManager.CurrentPlayer.NewMapTilePosition(DirectionTraveledForNewMapTile(data));
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
            string path = $"World/MapTiles/TileJson/MapTile_{id}.json";
            MapTileData data = JsonLoader.LoadTileData(path);



            if (data == null)
            {
                DebugBugger.Add($"Failed to load tile data for ID '{id}', falling back to '0_0_0'.");
                if (tiles.TryGetValue("0_0_0", out var fallback))
                    CurrentMapTile = fallback;
                return;
            }

            if (string.IsNullOrWhiteSpace(data.Background))
                throw new Exception($"Tile ID {data.Id} has a missing texture path.");

            if (!AssetManager.TextureExists(data.Background))
                AssetManager.LoadTexture(data.Background, data.Background);

            Texture2D texture = AssetManager.GetTexture(data.Background);
  
            var tile = new MapTile(data, texture);
            tiles[id] = tile; // ✅ Cache the tile by its ID


            CurrentMapTile = tile;
        }
        public static bool IsCellWalkable(int x, int y)
        {
            return CurrentMapTile.WalkableMap.TryGetValue((x, y), out bool isWalkable) && isWalkable;
        }
        public static bool IsCellWalkable(Rectangle rec)
        {
            Vector2 topLeft = new Vector2(rec.Left, rec.Top);
            Vector2 topRight = new Vector2(rec.Right - 1, rec.Top);
            Vector2 bottomLeft = new Vector2(rec.Left, rec.Bottom - 1);
            Vector2 bottomRight = new Vector2(rec.Right - 1, rec.Bottom - 1);

            return
                GetCell(topLeft).IsWalkable &&
                GetCell(topRight).IsWalkable &&
                GetCell(bottomLeft).IsWalkable &&
                GetCell(bottomRight).IsWalkable;
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

        public static Vector2 OffSetFromCenterOfDiamond(TileCell cell)
        {
            Vector2 center = cell.CenterPoint;
            return new Vector2(center.X - MapTile.TileWidth / 2, center.Y - MapTile.TileHeight);
        }

        public static List<TileCell> GetWalkableNeighbors(TileCell cell, TileCell goal = null, CombatMonster self = null, bool includeMonsterTiles = false)
        {
            List<TileCell> combatantCells = new List<TileCell>();
            if (SceneManager.CurrentState == SceneState.Combat)
            {
                foreach (var comCell in CombatGuard.CurrentCombat.AIControlledMonsterMap.Values)
                {
                    combatantCells.Add(comCell);
                }
            }
            List<TileCell> neighbors = new();

            Point[] directions = new Point[]
{
    new(1, 1),   // down-right
    new(1, -1),  // up-right
    new(-1, 1),  // down-left
    new(-1, -1)  // up-left
};

            for (int i = 0; i < directions.Length; i++)
            {
                int newX = cell.X + directions[i].X;
                int newY = cell.Y + directions[i].Y;

                if (newX < 0 || newY < 0 || newX >= MapTile.GridWidth || newY >= MapTile.GridHeight)
                    continue;

                TileCell neighbor = CurrentMapTile.AllValidCells.FirstOrDefault(c => c.X == newX && c.Y == newY);

                if (neighbor == null)
                    continue;

                bool isGoal = goal != null && neighbor == goal;

                if (neighbor != null && neighbor.IsWalkable && !combatantCells.Contains(neighbor)  &&
                    (!neighbor.BlockedByMonster || includeMonsterTiles || isGoal ))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }
        public static bool IsNeighbor(List<TileCell> targets, TileCell current)
        {
            foreach (var target in targets)
            {
                int dx = target.X - current.X;
                int dy = target.Y - current.Y;

                // Valid neighbors are diagonally adjacent: (±1, ±1)
                if (Math.Abs(dx) == 1 && Math.Abs(dy) == 1)
                    return true;
            }

            return false;
        }

        public static List<TileCell> GetCellsInRange(TileCell origin, int range)
        {
            List<TileCell> result = new();

            foreach (var cell in CurrentMapTile.AllValidCells)
            {
                int dx = cell.X - origin.X;
                int dy = cell.Y - origin.Y;

                if (Math.Abs(dx % 2) == Math.Abs(dy % 2) &&
                    Math.Max(Math.Abs(dx), Math.Abs(dy)) <= range)
                {
                    result.Add(cell);
                }
            }

            return result;
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

        public static List<TileCell> GetFloodFillTileWithinRange(TileCell origin, int maxSteps, bool includeMonsterTiles = false)
        {
            List<TileCell> reachableCells = new();
            Queue<(TileCell cell, int steps)> queue = new();
            HashSet<TileCell> visited = new();

            queue.Enqueue((origin, 0));
            visited.Add(origin);

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();

                if (steps > maxSteps)
                    continue;

                reachableCells.Add(current);

                foreach (TileCell neighbor in GetWalkableNeighbors(current, null, null, includeMonsterTiles))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, steps + 1));
                    }
                }
            }

            reachableCells.Remove(origin); // Optional

            return reachableCells;
        }
        public static MapTileSaveData SaveMapTile()
        {
            return new MapTileSaveData
            {
                CurrentTileId = CurrentMapTile.Id
            };
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneState.Combat || SceneManager.CurrentState == SceneState.Play || SceneManager.CurrentState == SceneState.Dialogue)
            {
                spriteBatch.Draw(CurrentMapTile.BackgroundTexture, Vector2.Zero, Color.White);
            }
            
        }



    }
}
