using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.EscapeOverseer;
using PlayingAround.Managers.LoadingScreen;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Proximity;
using PlayingAround.Managers.Tiles;
using PlayingAround.Managers.TitleScreen;
using PlayingAround.Managers.UI;
using PlayingAround.Utils;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PlayingAround
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        private KeyboardState previousKeyboardState; // Used to toggle debug info
        private SpriteFont mainFont;

        //Debugging
        Texture2D debugPixel;
        private bool showDebugOutline = true;
        private bool showTileCellOutlines = true;
        private static float _timer = 0f;



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

            //Data that is not dependent on Save State
            SaveManager.LoadAllSaves();
            AssetManager.Initialize(Content);
            AssetLoader.LoadAllAssets();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            debugPixel = new Texture2D(GraphicsDevice, 1, 1);
            debugPixel.SetData(new[] { Color.White });
            mainFont = Content.Load<SpriteFont>("mainFont");
            ViewportManager.Initialize(GraphicsDevice);
            TitleScreenManager.LoadContent();
            ScreenTransitionManager.Initialize(GraphicsDevice);
            EscapeOverseer.LoadContent();


        }

        public static void WaitUntilLoadingIsDone(float delta)
        {
            if (_timer >= 5.0f)
            {
                SceneManager.SetState(SceneManager.SceneState.Play);
                _timer = 0;
                return;
            }
            _timer += delta;
        }
        protected override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            InputManager.Update(gameTime);
            EscapeOverseer.Update(gameTime);
            switch (SceneManager.CurrentState)
            {
                case (SceneManager.SceneState.TitleScreen):

                    TitleScreenManager.Update(gameTime);
                    break;
                case (SceneManager.SceneState.LoadingScreen):
                    WaitUntilLoadingIsDone(delta);
                    break;
                case (SceneManager.SceneState.Play):
                    UIManager.Update(gameTime);
                    PlayerManager.Update(gameTime);
                    TileCellManager.Update(gameTime);
                    TileManager.Update(gameTime);
                    ScreenTransitionManager.Update(gameTime);
                    ProximityManager.Update(gameTime);

                    MovementManager.Update(gameTime);
                    if (InputManager.IsKeyPressed(Keys.F3))
                        showDebugOutline = !showDebugOutline;

                    if (InputManager.IsKeyPressed(Keys.F4))
                        showTileCellOutlines = !showTileCellOutlines;
                    break; 
                case (SceneManager.SceneState.Combat):
                    CombatManager.Update(gameTime);
                    MovementManager.Update(gameTime);
                    break;
                
            }
            base.Update(gameTime);
        }


        public void EndGame()
        {
            this.Exit();
        }
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
           
            switch (SceneManager.CurrentState)
            {
                case (SceneManager.SceneState.TitleScreen):
                    TitleScreenManager.Draw(_spriteBatch);
                    break;
                case (SceneManager.SceneState.Play):
                    TileManager.Draw(_spriteBatch);
                    TileCellManager.Draw(_spriteBatch);
                    PlayerManager.Draw(_spriteBatch);
                    ScreenTransitionManager.Draw(_spriteBatch, GraphicsDevice);
                    UIManager.Draw(_spriteBatch, GraphicsDevice);
 
                    if (showTileCellOutlines)
                        TileManager.CurrentMapTile?.DrawTileCellOutlines(_spriteBatch, debugPixel);
                    if (showDebugOutline)
                        TileManager.CurrentMapTile?.DrawTileCellDebugOverlay(_spriteBatch, debugPixel);
                    if (showDebugOutline)
                    {
                        PlayerManager.CurrentPlayer.DrawDebugPath(_spriteBatch, debugPixel);
                        DrawRectangle(PlayerManager.CurrentPlayer.HitBox, Color.Red);
                        DrawDebugOverlay();

                    }
                    break;
                case (SceneManager.SceneState.LoadingScreen):
                    LoadingScreenManager.Draw(_spriteBatch);
                    break;

                    case (SceneManager.SceneState.Combat):
                    UIManager.Draw(_spriteBatch, GraphicsDevice);
                    CombatManager.Draw(_spriteBatch, GraphicsDevice);
                    break;
            }
            EscapeOverseer.Draw(_spriteBatch);
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

            _spriteBatch.DrawString(mainFont, debugText, new Vector2(10, 10), Color.Blue);
        }




    }
}
