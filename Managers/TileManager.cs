using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.MapTile;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Utils;

namespace PlayingAround.Manager
{
    public static class TileManager
    {
        private static Dictionary<string, MapTile> tiles = new();
        public static MapTile CurrentMapTile { get; private set; }




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
                PlayMonsterManager.UpdateCurrentPlayMonsters(existingTile.PlayMonsters);
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
            if (tile.TotalMonsterSpawns != 0 )
            {
                tile.PlayMonsters = PlayMonsterManager.GeneratePlayMonsters(data);

            }
            tiles[id] = tile; // ✅ Cache the tile by its ID


            CurrentMapTile = tile;
            PlayMonsterManager.ClearMonsters();
            PlayMonsterManager.UpdateCurrentPlayMonsters(tile.PlayMonsters);
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

            if (x < 0 || x >= 30 || y < 0 || y >= 17)
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

            PlayMonsterManager.Draw(spriteBatch);
        }

    }
}
