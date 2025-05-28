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




        public static void LoadContent(GraphicsDevice graphics)
        {
            debugPixel = new Texture2D(graphics, 1, 1);
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



        private static void DrawDebugOutLines(SpriteBatch spriteBatch)
        {
            if (showDebugOutline)
            {
                TileManager.CurrentMapTile?.DrawTileCellDebugOverlay(spriteBatch, debugPixel);
                PlayerManager.CurrentPlayer.DrawDebugPath(spriteBatch, debugPixel);
                DrawRectangle(PlayerManager.CurrentPlayer.HitBox, Color.Red, spriteBatch);
                DrawDebugOverlay(spriteBatch);
            }

        }
        
        private static void DrawTileCellOutlines(SpriteBatch spriteBatch)
        {
            if (showTileCellOutlines)
                TileManager.CurrentMapTile?.DrawTileCellOutlines(spriteBatch, debugPixel);
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
            Rectangle feetHitbox = PlayerManager.CurrentPlayer.HitBox;
            Vector2 feetCenter = PlayerManager.CurrentPlayer.HitBoxCenter;
            Vector2? clickTarget = PlayerManager.CurrentPlayer.GetDebugClickTarget();

            string debugText =
                $"Feet Hitbox: {feetHitbox}\n" +
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
