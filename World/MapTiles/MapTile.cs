using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Managers;
using System.Collections.Generic;

namespace PlayingAround.Game.Map
{
    public class MapTile
    {
        public string Id { get; }
        public Texture2D BackgroundTexture { get; }
        public List<Rectangle> Obstacles { get; } = new();
        public TileCell[,] TileGrid { get; private set; }
        public float DifficultyMax { get; }
        public float DifficultyMin { get; }
        public int TotalMonsterSpawns { get; }
        public List<PlayMonsters> PlayMonstersList { get; } = new List<PlayMonsters> ();
        public PlayMonsterManager PlayMonstersManager { get; } = new PlayMonsterManager();



        public const int GridWidth = 30;   // example number of cells per screen
        public const int GridHeight = 17;
        public const int TileWidth = 64;
        public const int TileHeight = 64;


        public MapTile(MapTileData data, Texture2D backgroundTexture)
        {
            Id = $"{data.GridX}_{data.GridY}_{data.GridZ}";
            
            BackgroundTexture = backgroundTexture;
            //Monsters = data.Monsters;
            DifficultyMax = data.DifficultyMax;
            DifficultyMin = data.DifficultyMin;
            TotalMonsterSpawns = data.TotalMonsterSpawns;
            // Initialize grid
            TileGrid = new TileCell[GridWidth, GridHeight];

            // Overwrite with actual data from JSON
            foreach (var cellData in data.Cells)
            {
                if (cellData.X > 29) { continue; }
                if (cellData.Y > 16) {  continue; }
                TileGrid[cellData.X, cellData.Y] = new TileCell(
                    cellData.X,
                    cellData.Y,
                    "default", // You can optionally add TexturePath per cell later
                    cellData.Walkable,
                    cellData.Z,
                    cellData.HeroSpawnable,
                    cellData.MonsterSpawnable,
                    cellData.BehindOverlay,
                    cellData.FrontOverlay,
                    cellData.Npc,
                    cellData.Trigger,
                    cellData.NextTile
                );
            }
            PlayMonstersList = PlayMonstersManager.GeneratePlayMonsters(data);
        }



        public void DrawTileCellOutlines(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    var cell = TileGrid[x, y];

                    // Isometric position (you can replace with top-down if needed for now)
                    int screenX = x * TileWidth;
                    int screenY = y * TileHeight;

                    var rect = new Rectangle(screenX, screenY, TileWidth, TileHeight);

                    // Top
                    spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), Color.Black);
                    // Left
                    spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), Color.Black);
                    // Right
                    spriteBatch.Draw(debugPixel, new Rectangle(rect.Right, rect.Y, 1, rect.Height), Color.Black);
                    // Bottom
                    spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Bottom, rect.Width, 1), Color.Black);
                }
            }
        }
        public void DrawTileCellDebugOverlay(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    var cell = TileGrid[x, y];
                    if (cell == null)
                        continue;

                    Rectangle rect = new Rectangle(
                        x * TileWidth,
                        y * TileHeight,
                        TileWidth,
                        TileHeight
                    );

                    Color overlayColor = cell.IsWalkable
                        ? new Color(0, 255, 0, 60)  // light green
                        : new Color(255, 0, 0, 60); // light red

                    spriteBatch.Draw(debugPixel, rect, overlayColor);
                }
            }
        }



    }
}
