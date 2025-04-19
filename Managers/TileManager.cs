using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.MapTile;
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
        public static List<TileCell> GetWalkableNeighbors(TileCell cell)
        {
            List<TileCell> neighbors = new();

            Point[] directions = new Point[]
            {
            new(0, -1), // Up
            new(0, 1),  // Down
            new(-1, 0), // Left
            new(1, 0)   // Right
            };

            foreach (var dir in directions)
            {
                int newX = cell.X + dir.X;
                int newY = cell.Y + dir.Y;

                TileCell neighbor = CurrentMapTile.TileGrid[newX, newY];
                if (neighbor != null && neighbor.IsWalkable)
                    neighbors.Add(neighbor);
            }

            return neighbors;
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
