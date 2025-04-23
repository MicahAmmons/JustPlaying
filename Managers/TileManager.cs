using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.MapTile;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Proximity;
using PlayingAround.Utils;

namespace PlayingAround.Manager
{
    public static class TileManager
    {
        private static Dictionary<string, MapTile> tiles = new();
        public static MapTile CurrentMapTile { get; private set; }

        public static TileCell PlayerCurrentCell;

       




        public static void Initialize(GraphicsDevice graphicsDevice, string id)
        {

            LoadMapTileById(id);
            ScreenTransitionManager.OnFadeToBlackComplete += () =>
            {
                var next = TileCellManager.PlayerCurrentCell.NextTile;
                string id = $"{next.NextX}_{next.NextY}_{next.NextZ}";

                TileManager.LoadMapTileById(id);
            };

        }
        public static void LoadMapTileById(string id)
        {
            if (tiles.TryGetValue(id, out var existingTile))
            {
                CurrentMapTile = existingTile;
                ProximityManager.UpdateMapTile(CurrentMapTile);
                return;
            }

            // Try loading from disk
            string path = $"World/MapTiles/TileJson/MapTile_{id}.json";
            MapTileData data = JsonLoader.LoadTileData(path);



            if (data == null)
            {
                Debug.WriteLine($"Failed to load tile data for ID '{id}', falling back to '0_0_0'.");
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
            ProximityManager.UpdateMapTile(CurrentMapTile);
        }

        public static bool IsCellWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapTile.GridWidth || y >= MapTile.GridHeight)
                return false;
            return CurrentMapTile.TileGrid[x, y].IsWalkable;
        }
        public static bool IsCellWalkable(Rectangle rec)
        {
            int left = rec.Left / MapTile.TileWidth;
            int right = (rec.Right - 1) / MapTile.TileWidth;
            int top = rec.Top / MapTile.TileHeight;
            int bottom = (rec.Bottom - 1) / MapTile.TileHeight;

            return
                IsCellWalkable(left, top) &&
                IsCellWalkable(right, top) &&
                IsCellWalkable(left, bottom) &&
                IsCellWalkable(right, bottom);
        }

        public static TileCell GetCell(Vector2 cord)
        {
            int x = (int)(cord.X / MapTile.TileWidth);
            int y = (int)(cord.Y / MapTile.TileHeight);

            if (x < 0 || x >= 60 || y < 0 || y >= 34)
            {
                return new TileCell(
                69,               
                69,               
                "Default/Blank",    
                false,             
                0                   
                );
            }

            return CurrentMapTile.TileGrid[x, y];
        }
        public static TileCell GetCell(Rectangle rect)
        {
            Vector2 bottomCenter = new Vector2(
                rect.X + rect.Width / 2f,
                rect.Y + rect.Height
            );

            return GetCell(bottomCenter);
        }
        public static Vector2 GetCellCords(TileCell cell)
        {
            int x = cell.X * MapTile.TileWidth;
            int y = cell.Y * MapTile.TileHeight;
            return new Vector2( x, y );
        }
        public static void OnEnterNewCell(TileCell cell)
        {
                PlayerCurrentCell = cell;
        }
        public static List<TileCell> GetWalkableNeighbors(TileCell cell, TileCell goal = null, CombatMonster self = null)
        {
            List<TileCell> neighbors = new();

            Point[] directions = new Point[]
            {
        new(0, -1),
        new(0, 1),
        new(-1, 0),
        new(1, 0)
            };

            foreach (var dir in directions)
            {
                int newX = cell.X + dir.X;
                int newY = cell.Y + dir.Y;

                if (newX < 0 || newY < 0 || newX >= CurrentMapTile.TileGrid.GetLength(0) || newY >= CurrentMapTile.TileGrid.GetLength(1))
                    continue;

                TileCell neighbor = CurrentMapTile.TileGrid[newX, newY];

                bool isGoal = goal != null && neighbor == goal;

                if (neighbor != null && neighbor.IsWalkable &&
                    (!neighbor.BlockedByMonster || isGoal))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public static List<TileCell> GetWalkableNeighbors(TileCell cell)
        {
            return GetWalkableNeighbors(cell, null);
        }


        public static bool IsNeighbor(List<TileCell> targets, TileCell current)
        {
            foreach (var target in targets)
            {
                int dx = Math.Abs(target.X - current.X);
                int dy = Math.Abs(target.Y - current.Y);

                // Manhattan distance of 1 means direct neighbor (no diagonals)
                if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
                    return true;
            }

            return false;
        }



        public static void AddCombatMonsterToCell(CombatMonster mon, TileCell newCell)
        {
            if (mon == null || newCell == null)
                return;

            if (mon.CurrentCell == newCell)
                return;

            // Remove from old cell
            if (mon.CurrentCell != null)
            {
                mon.CurrentCell.BlockedByMonster = false;
                mon.CurrentCell.CombatMonster = null;
            }

            // Assign to new
            newCell.CombatMonster = mon;
            mon.CurrentCell = newCell;
            mon.CurrentCell.BlockedByMonster = true;
        }

        public static List<TileCell> GetCellsInRange(TileCell origin, int range)
        {
            List<TileCell> result = new();

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if ((dx != 0 || dy != 0) && Math.Abs(dx) + Math.Abs(dy) <= range) // Manhattan only
                    {
                        int x = origin.X + dx;
                        int y = origin.Y + dy;

                        if (x >= 0 && x < MapTile.GridWidth && y >= 0 && y < MapTile.GridHeight)
                        {
                            TileCell cell = TileManager.CurrentMapTile.TileGrid[x, y];
                            result.Add(cell);
                        }
                    }
                }
            }

            return result;
        }
        public static List<TileCell> GetFloodFillTileWithinRange(TileCell origin, int maxSteps)
        {
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

                    foreach (TileCell neighbor in GetWalkableNeighbors(current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, steps + 1));
                        }
                    }
                }

                // Optionally remove the origin if you don't want to include the starting tile
                reachableCells.Remove(origin);

                return reachableCells;
            }
        }




        public static MapTileSaveData Save()
        {
            return new MapTileSaveData
            {
                CurrentTileId = CurrentMapTile.Id
            };
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(CurrentMapTile.BackgroundTexture, Vector2.Zero, Color.White);

            CurrentMapTile.PlayMonstersManager.Draw(spriteBatch);
        }

        public static void Update(GameTime gameTime)
        {
            CurrentMapTile.PlayMonstersManager.Update(gameTime);
        }



    }
}
