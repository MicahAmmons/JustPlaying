using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Utils;

namespace PlayingAround.Manager
{
    public static class TileManager
    {
        private static Dictionary<string, MapTile> tiles = new();
        public static MapTile CurrentMapTile { get; private set; }

        public static void LoadMapTiles(GraphicsDevice graphicsDevice, ContentManager content)
        {
            var tileDataList = JsonLoader.LoadTileData("World/MapTiles/TileJson/MapTile_0_0_0.json");

            foreach (var data in tileDataList)
            {
                if (string.IsNullOrWhiteSpace(data.Background))
                    throw new Exception($"Tile ID {data.Id} has a missing texturePath in tiles.json.");

                // Load texture via AssetManager
                AssetManager.LoadTexture(data.Background, data.Background); // Key and path are the same
                Texture2D texture = AssetManager.GetTexture(data.Background);
                        
                var tile = new MapTile(data, texture);
                tiles[data.Id] = tile;

                // For now, just set the first tile as the current one
                if (CurrentMapTile == null)
                    CurrentMapTile = tile;
            }
        }
        public static void LoadMapTileById(string id)
        {

        }
    }
}
