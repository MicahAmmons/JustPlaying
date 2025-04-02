using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster;
using System.Collections.Generic;

namespace PlayingAround.Game.Map
{
    public class MapTile
    {
        public string Id { get; }
        public Texture2D BackgroundTexture { get; }
        public List<Monster> Monsters { get; } = new();
        public List<Rectangle> Obstacles { get; } = new();

        public TileCell[,] TileGrid { get; private set; }


        public const int GridWidth = 30;   // example number of cells per screen
        public const int GridHeight = 17;
        public const int TileWidth = 64;
        public const int TileHeight = 64;


        public MapTile(MapTileData data, Texture2D backgroundTexture)
        {
            Id = data.Id;

            BackgroundTexture = backgroundTexture;

            // Initialize grid
            TileGrid = new TileCell[GridWidth, GridHeight];

            // Overwrite with actual data from JSON
            foreach (var cellData in data.Cells)
            {
                TileGrid[cellData.X, cellData.Y] = new TileCell(
                    cellData.X,
                    cellData.Y,
                    "default", // You can optionally add TexturePath per cell later
                    cellData.Walkable,
                    cellData.Z,
                    cellData.BehindOverlay,
                    cellData.FrontOverlay,
                    cellData.Npc,
                    cellData.Monster,
                    cellData.Trigger
                );
            }
        }


            public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BackgroundTexture, Vector2.Zero, Color.White);
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
