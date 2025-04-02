using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Manager;
using PlayingAround.Utils;
using System;
using System.Reflection;
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
            GameState.SaveData = SaveSystem.LoadGame() ?? new GameSaveData();
            TileManager.LoadMapTileById(GameState.SaveData.CurrentTileId);
            // Player.LoadFromSave(GameState.SaveData.Player);

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
            string textureKey = GameState.SaveData.Player.TextureKey;
            AssetManager.LoadTexture(textureKey, $"HeroArt/{textureKey}");
            Texture2D idleTex = AssetManager.GetTexture(textureKey);
            if (GameState.SaveData.Player != null)
            {

               
                player = Player.LoadFromSave(GameState.SaveData.Player);
            }
            else
            {
                idleTex = AssetManager.GetTexture("Hero_Idle");

                player = new Player(idleTex, new Vector2(100, 100), 200f);
            }
        }

        



        private void SaveState()
        {
            GameState.SaveData.Player = player.Save();
            GameState.SaveData.CurrentTileId = TileManager.CurrentMapTile.Id;

            // Collect other data...

            SaveSystem.SaveGame(GameState.SaveData);

        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
    Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                SaveState();
                Exit();
            }

            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3))
            {
                showDebugOutline = !showDebugOutline;
            }
            if (keyboard.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4))
            {
                showTileCellOutlines = !showTileCellOutlines;
            }
            if (mouse.RightButton == ButtonState.Pressed)
            {
                Rectangle start = player.GetFeetHitbox();
                Vector2 target = new Vector2(mouse.X, mouse.Y);
                var path = CustomPathfinder.BuildPixelPath(start, target);
                player.SetPath(path);
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
                player.DrawDebugPath(_spriteBatch, debugPixel);
                DrawRectangle(player.GetFeetHitbox(), Color.Red);
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
            Rectangle feetHitbox = player.GetFeetHitbox();
            Vector2 feetCenter = player.GetFeetCenter();
            Vector2? clickTarget = player.GetDebugClickTarget();

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

            _spriteBatch.DrawString(debugFont, debugText, new Vector2(10, 10), Color.Blue);
        }




    }
}
