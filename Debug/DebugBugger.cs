using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Debug
{
    public static class DebugBugger
    {
        static Texture2D debugPixel;
        static private MapTile _currentMapTile => TileManager.CurrentMapTile;
        private static bool showDebugOutline = false;
        private static bool showTileCellOutlines = false;
        private static bool showDebugLines = false;
        private static KeyboardState previousKeyboardState; // Used to toggle debug info
        private static SpriteFont _mainFont;
        public static bool EnableLogging { get; set; } = true;
        private static List<String> debugLines = new List<String>();
        private static Vector2 debugBoxPosition;
        private static int debugBoxWidth = 300;
        private const int debugBoxMargin = 10;
        private const int debugMaxLines = 5;
        private static List<Vector2> cachedLinePositions = new();
        private static Rectangle cachedBoxRect;
        private static bool _logDirty = true;
        private static List<string> cachedLogLines = new();
        private static Texture2D _diamondHighlightTexture { get; set; }
        private static GraphicsDevice _graphics;




        public static void LoadContent(GraphicsDevice graphics)
        {
            _graphics = graphics;
            debugPixel = new Texture2D(graphics, 1, 1);
            _diamondHighlightTexture = CreateDiamondTexture(128, 64, Color.Yellow * 0.5f);
            debugPixel.SetData(new[] { Color.White });
            _mainFont = AssetManager.GetFont("mainFont");
            int screenHeight = ViewportManager.ScreenHeight;
            debugBoxPosition = new Vector2(debugBoxMargin, screenHeight - debugBoxMargin);


        }
        public static void Add(string message)
        {
            debugLines.Add(message);
            _logDirty = true;
        }

        public static void Update(GameTime gameTime)
        {
            ToggleDebugLines();
            ToggleCellGridLines();
            ToggleDebugText();

        }
        public static void ToggleDebugText()
        {
            if (InputManager.IsKeyPressed(Keys.F1))
                showDebugLines = !showDebugLines;
        }
        private static void ToggleDebugLines()
        {
            if (InputManager.IsKeyPressed(Keys.F3))
                showDebugOutline = !showDebugOutline;
        }
        private static void ToggleCellGridLines()
        {
            if (InputManager.IsKeyPressed(Keys.F4))
                showTileCellOutlines = !showTileCellOutlines;
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            DrawTileCellOutlines(spriteBatch);
            DrawDebugOutLines(spriteBatch);
            DrawDebugLines(spriteBatch);

        }
        private static void DrawDebugLines(SpriteBatch spriteBatch)
        {
            if (!showDebugLines || debugLines.Count == 0)
                return;

            if (_logDirty)
                RecalculateLogLayout();

            spriteBatch.Draw(debugPixel, cachedBoxRect, Color.Green * 0.5f);

            for (int i = 0; i < cachedLogLines.Count; i++)
            {
                spriteBatch.DrawString(_mainFont, cachedLogLines[i], cachedLinePositions[i], Color.White);
            }
        }


        private static Texture2D CreateDiamondTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(_graphics, width, height);
            Color[] data = new Color[width * height];

            int centerX = width / 2;
            int centerY = height / 2;

            for (int y = 0; y < height; y++)
            {
                // Normalized distance from vertical center
                float normY = Math.Abs(y - centerY) / (float)centerY;
                int halfRowWidth = (int)(centerX * (1 - normY));

                int startX = centerX - halfRowWidth;
                int endX = centerX + halfRowWidth;

                for (int x = startX; x <= endX; x++)
                {
                    if (x >= 0 && x < width)
                        data[y * width + x] = color;
                }
            }

            texture.SetData(data);
            return texture;
        }

        public static void DrawTileCellOutlines(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            foreach (var cell in _currentMapTile.AllValidCells)
            {
                if (cell == null) continue;
                int drawX = cell.X * 64;
                int drawY = cell.Y * 32;


                // Diamond corners
                Vector2 top = new Vector2(drawX, drawY - 32);
                Vector2 right = new Vector2(drawX + 64, drawY);
                Vector2 bottom = new Vector2(drawX, drawY + 32);
                Vector2 left = new Vector2(drawX - 64, drawY);

                DrawLine(spriteBatch, debugPixel, top, right, Color.Black);
                DrawLine(spriteBatch, debugPixel, right, bottom, Color.Black);
                DrawLine(spriteBatch, debugPixel, bottom, left, Color.Black);
                DrawLine(spriteBatch, debugPixel, left, top, Color.Black);
                if (cell.IsWalkable)
                {
                    DrawHighlightCell(spriteBatch, cell, Color.Green);
                }
                else if (!cell.IsWalkable)
                {
                    DrawHighlightCell(spriteBatch, cell, Color.Red);
                }
            }
        }
        public static void DrawHighlightCell(SpriteBatch spriteBatch, TileCell cell, Color col)
        {
            int drawX = cell.X * 64;
            int drawY = cell.Y * 32;

            Vector2 origin = new Vector2(_diamondHighlightTexture.Width / 2, _diamondHighlightTexture.Height / 2);
            spriteBatch.Draw(_diamondHighlightTexture, new Vector2(drawX, drawY), null, col, 0f, origin, 1f, SpriteEffects.None, 0f);

        }
        private static void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(pixel, new Rectangle((int)start.X, (int)start.Y, (int)length, 1), null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        private static void DrawDebugOutLines(SpriteBatch spriteBatch)
        {
            if (!showDebugOutline) return;

            var diamond = PlayerManager.CurrentPlayer.HitBox;

            // Draw lines between diamond corners
            for (int i = 0; i < diamond.Length; i++)
            {
                Vector2 start = diamond[i];
                Vector2 end = diamond[(i + 1) % diamond.Length];
                DrawLine(spriteBatch, debugPixel, start, end, Color.Red);
            }
            DrawRectangle(PlayerManager.CurrentPlayer.GetRectangleHitBox(), Color.Red, spriteBatch);
            DrawDebugOverlay(spriteBatch);
        }


        private static void DrawTileCellOutlines(SpriteBatch spriteBatch)
        {
            if (showTileCellOutlines)
               DrawTileCellOutlines(spriteBatch, debugPixel);
        }
         private static void DrawRectangle(Rectangle rect, Color color, SpriteBatch spriteBatch)
        {
            // Top
            spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            // Left
            spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            // Right
            spriteBatch.Draw(debugPixel, new Rectangle(rect.Right, rect.Y, 1, rect.Height), color);
            // Bottom
            spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Bottom, rect.Width + 1, 1), color);
        } // Debugging Border Rectangle 
        private static void DrawDebugOverlay(SpriteBatch spriteBatch)
        {
            Vector2[] feetHitbox = PlayerManager.CurrentPlayer.HitBox;
            Vector2 feetCenter = PlayerManager.CurrentPlayer.HitBoxCenter;
            Vector2? clickTarget = PlayerManager.CurrentPlayer.GetDebugClickTarget();
            string hitboxStr = string.Join(", ", feetHitbox.Select(v => $"({v.X:0},{v.Y:0})"));

            string debugText =
                $"Feet Hitbox: {hitboxStr}\n" +
                $"Feet Center: X={feetCenter.X:0}, Y={feetCenter.Y:0}\n" +
                $"Feet Tile: X={(int)(feetCenter.X / MapTile.TileWidth)}, Y={(int)(feetCenter.Y / MapTile.TileHeight)}\n" +
                $"Outline: {(showDebugOutline ? "ON" : "OFF")}\n";

            if (clickTarget.HasValue)
            {
                debugText += $"Target Pos: X={clickTarget.Value.X:0}, Y={clickTarget.Value.Y:0}\n";
                debugText += $"Target Tile: X={(int)(clickTarget.Value.X / MapTile.TileWidth)}, Y={(int)(clickTarget.Value.Y / MapTile.TileHeight)}";
            }

            spriteBatch.DrawString(_mainFont, debugText, new Vector2(10, 10), Color.Blue);
        }

        private static void RecalculateLogLayout()
        {
            int lineCount = Math.Min(debugMaxLines, debugLines.Count);
            cachedLogLines = debugLines.Skip(Math.Max(0, debugLines.Count - lineCount)).ToList();

            int lineHeight = (int)_mainFont.MeasureString("A").Y;
            int boxHeight = lineHeight * lineCount + debugBoxMargin * 2;

            // Optional: auto-width based on widest line
            int dynamicWidth = cachedLogLines.Count > 0
                ? (int)cachedLogLines.Max(line => _mainFont.MeasureString(line).X)
                : 0;

            debugBoxWidth = Math.Max(debugBoxWidth, dynamicWidth + debugBoxMargin * 2);

            Vector2 boxPos = debugBoxPosition - new Vector2(0, boxHeight);
            cachedBoxRect = new Rectangle((int)boxPos.X, (int)boxPos.Y, debugBoxWidth, boxHeight);

            // Cache string positions
            cachedLinePositions.Clear();
            for (int i = 0; i < cachedLogLines.Count; i++)
            {
                cachedLinePositions.Add(new Vector2(boxPos.X + debugBoxMargin, boxPos.Y + debugBoxMargin + i * lineHeight));
            }

            _logDirty = false;
        }


    }
}
