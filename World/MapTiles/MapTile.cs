using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Managers.Entities;
using System;
using System.Collections.Generic;

namespace PlayingAround.Game.Map
{
    public class MapTile
    {
        public string Id { get; }
        public Texture2D BackgroundTexture { get; }
        public List<Rectangle> Obstacles { get; } = new();
        public TileCell[,] TileGrid { get; private set; }
        public List<TileCell> MonsterSpawnableCells { get; private set; } = new List<TileCell> { };
        public List<TileCell> PlayerSpawnableCells { get; private set; } = new List<TileCell> { };
        public float DifficultyMax { get; }
        public float DifficultyMin { get; }
        public int TotalMonsterSpawns { get; }
        public List<PlayMonsters> PlayMonstersList { get; } = new List<PlayMonsters> ();
        public PlayMonsterManager PlayMonstersManager { get; } = new PlayMonsterManager();



        public const int GridWidth = 120;   // example number of cells per screen
        public const int GridHeight = 34;
        public const int TileWidth = 64;
        public const int TileHeight = 32;


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
                if (cellData.MonsterSpawnable) MonsterSpawnableCells.Add(TileGrid[cellData.X, cellData.Y]);
                if (cellData.HeroSpawnable) PlayerSpawnableCells.Add(TileGrid[cellData.X, cellData.Y]);

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
                    if (cell == null) continue;

                    int screenX = (x - y) * (TileWidth / 2);
                    int screenY = (x + y) * (TileHeight / 2);

                    // Define the 4 corners of the diamond
                    Point top = new(screenX + TileWidth / 2, screenY);
                    Point right = new(screenX + TileWidth, screenY + TileHeight / 2);
                    Point bottom = new(screenX + TileWidth / 2, screenY + TileHeight);
                    Point left = new(screenX, screenY + TileHeight / 2);

                    // Draw lines between the corners
                    DrawLine(spriteBatch, debugPixel, top, right, Color.Black);
                    DrawLine(spriteBatch, debugPixel, right, bottom, Color.Black);
                    DrawLine(spriteBatch, debugPixel, bottom, left, Color.Black);
                    DrawLine(spriteBatch, debugPixel, left, top, Color.Black);
                }
            }
        }
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Point p1, Point p2, Color color)
        {
            float distance = Vector2.Distance(p1.ToVector2(), p2.ToVector2());
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            spriteBatch.Draw(pixel,
                new Rectangle(p1.X, p1.Y, (int)distance, 1),
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
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
                            (x - y) * TileWidth / 2,
                            (x + y) * (TileHeight / 2),
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
