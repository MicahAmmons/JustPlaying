using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data.SaveData;
using PlayingAround.Debug;
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
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.EscapeOverseer;
using PlayingAround.Managers.JukeBox;
using PlayingAround.Managers.LoadingScreen;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.NPCHouse;
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

            DrawDiamondTexture.LoadContent(GraphicsDevice);
            SaveManager.LoadAllSaves();
            AssetManager.Initialize(Content);
            AssetLoader.LoadAllFonts();
            AssetLoader.LoadAllTextures();
            JukeBoxManager.InitializeJukeBox();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            DebugBugger.LoadContent(GraphicsDevice);
            DialogueLibrary.LoadContent();
          
            ViewportManager.Initialize(GraphicsDevice);
            TitleScreenManager.LoadContent();
            ScreenTransitionManager.Initialize(GraphicsDevice);
            EscapeOverseer.LoadContent();
            NPCManager.LoadContent();
            DialogueBox.LoadContent();

        }

        public static void WaitUntilLoadingIsDone(float delta)
        {
            if (_timer >= 5.0f)
            {
                SceneManager.SetState(SceneState.Play);
                _timer = 0;
                return;
            }
            _timer += delta;
        }
        protected override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            JukeBoxManager.Update(gameTime);
            InputManager.Update(gameTime);
            EscapeOverseer.Update(gameTime);
            MovementManager.Update(gameTime);
            switch (SceneManager.CurrentState)
            {
                case (SceneState.Cinematic):
                    CinematicRuler.Update(gameTime);
                    break;
                case (SceneState.TitleScreen):

                    TitleScreenManager.Update(gameTime);
                    break;
                case (SceneState.LoadingScreen):
                    WaitUntilLoadingIsDone(delta);
                    break;
                case (SceneState.Play):
                    UIManager.Update(gameTime);
                    PlayerManager.Update(gameTime);
                    TileCellManager.Update(gameTime);
                    TileManager.Update(gameTime);
                    ScreenTransitionManager.Update(gameTime);
                    ProximityManager.Update(gameTime);
                    DebugBugger.Update(gameTime);
                  
                    break; 
                case (SceneState.Combat):
                    CombatGuard.Update(gameTime);
                    DebugBugger.Update(gameTime);
                    break;
                case (SceneState.Dialogue):
                    DialogueManager.Update();
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
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            switch (SceneManager.CurrentState)
            {
                case (SceneState.TitleScreen):
                    TitleScreenManager.Draw(_spriteBatch);
                    break;
                case (SceneState.Play):
                    TileManager.Draw(_spriteBatch);
                    TileCellManager.Draw(_spriteBatch);
                    ScreenTransitionManager.Draw(_spriteBatch, GraphicsDevice);
                    DebugBugger.Draw(_spriteBatch);
                    PlayMonsterManager.Draw(_spriteBatch);
                    PlayerManager.Draw(_spriteBatch);
                    UIManager.Draw(_spriteBatch, GraphicsDevice);
                    break;
                case (SceneState.LoadingScreen):
                    LoadingScreenManager.Draw(_spriteBatch);
                    break;

                    case (SceneState.Combat):
                    CombatGuard.Draw(_spriteBatch, GraphicsDevice);
                    UIManager.Draw(_spriteBatch, GraphicsDevice);
                    DebugBugger.Draw(_spriteBatch);

                    break;
                case SceneState.Cinematic:
                    CinematicRuler.Draw(_spriteBatch);
                    break;
                    case SceneState.Dialogue:
                    TileManager.Draw(_spriteBatch);
                    TileCellManager.Draw(_spriteBatch);
                    DebugBugger.Draw(_spriteBatch);
                    PlayMonsterManager.Draw(_spriteBatch);
                    PlayerManager.Draw(_spriteBatch);
                    UIManager.Draw(_spriteBatch, GraphicsDevice);
                    DialogueManager.Draw(_spriteBatch);
                    
                    break;
            }
            EscapeOverseer.Draw(_spriteBatch);
            
            _spriteBatch.End();
            base.Draw(gameTime);


        }







    }
}
