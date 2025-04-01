using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Assets;
using PlayingAround.Manager;
using PlayingAround.Utils;
using System;
using System.Text.Json;

namespace PlayingAround
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player player;

        private KeyboardState previousKeyboardState; // Used to toggle debug info
        private SpriteFont debugFont;

        //Debugging
        Texture2D debugPixel;
        private bool showDebugOutline = true;
        private bool showTileCellOutlines = true;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set the initial window size
            _graphics.PreferredBackBufferWidth = 1920;  // Width in pixels
            _graphics.PreferredBackBufferHeight = 1080;  // Height in pixels
            _graphics.ApplyChanges();                   // Apply the changes
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var tileDataList = JsonLoader.LoadTileData("World/MapTiles/TileJson/MapTile_0_0.json");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            debugPixel = new Texture2D(GraphicsDevice, 1, 1);
            debugPixel.SetData(new[] { Color.White });
            debugFont = Content.Load<SpriteFont>("debug");

            // Initialize AssetManager
            AssetManager.Initialize(Content);

            // Load hero textures
            AssetManager.LoadTexture("Hero_Idle", "HeroArt/BlonderHero");

            // Load tiles
            TileManager.LoadMapTiles(GraphicsDevice, Content);

            // Set up player with animation frames
            var idleTex = AssetManager.GetTexture("Hero_Idle");
            player = new Player(idleTex, new Vector2(100, 100), 200f);
        }




        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3))
            {
                showDebugOutline = !showDebugOutline;
            }
            if (keyboard.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4))
            {
                showTileCellOutlines = !showTileCellOutlines;
            }

            player.Update(gameTime);

            previousKeyboardState = keyboard;
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            TileManager.CurrentMapTile?.Draw(_spriteBatch);

            if (showTileCellOutlines)
                TileManager.CurrentMapTile?.DrawTileCellOutlines(_spriteBatch, debugPixel);
            if (showDebugOutline)
                TileManager.CurrentMapTile?.DrawTileCellDebugOverlay(_spriteBatch, debugPixel);

            player.Draw(_spriteBatch);

            if (showDebugOutline)
            {
                DrawRectangle(player.GetDrawRectangle(), Color.Red);
                DrawDebugOverlay();
            }



            _spriteBatch.End();
            base.Draw(gameTime);
        }



        private void DrawRectangle(Rectangle rect, Color color)
        {
            // Top
            _spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            // Left
            _spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            // Right
            _spriteBatch.Draw(debugPixel, new Rectangle(rect.Right, rect.Y, 1, rect.Height), color);
            // Bottom
            _spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Bottom, rect.Width + 1, 1), color);
        } // Debugging Border Rectangle 
        private void DrawDebugOverlay()
        {
            Vector2 playerPos = player.Position;

            string debugText =
                $"Position: X={playerPos.X:0}, Y={playerPos.Y:0}\n" +
                $"Draw Rect: {player.GetDrawRectangle()}\n" +
                $"Outline: {(showDebugOutline ? "ON" : "OFF")}";


            _spriteBatch.DrawString(debugFont, debugText, new Vector2(10, 10), Color.Red);
        }// houses the debug overlay text info



    }
}
