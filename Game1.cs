using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Debug;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Escape.Settings;
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

            ViewportManager.Initialize(GraphicsDevice);


            DrawDiamondTexture.LoadContent(GraphicsDevice);
            SaveManager.LoadAllSaves();
            AssetManager.Initialize(Content);
            JukeBoxManager.InitializeJukeBox();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            DebugBugger.LoadContent(GraphicsDevice);
            DialogueLibrary.LoadContent();
            SettingsSuper.LoadContent();
            TitleScreenManager.LoadContent();
            MapTileTransitionManager.Initialize(GraphicsDevice);
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
            if (EscapeOverseer.ShouldExit) EndGame();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            JukeBoxManager.Update(gameTime);
            InputManager.Update(gameTime);
            EscapeOverseer.Update(gameTime);
            MovementManager.Update(gameTime);
            TileCellManager.Update(delta);
            CinematicRuler.Update(gameTime);
            switch (SceneManager.CurrentState)
            {
                case (SceneState.LoadingScreen):
                    WaitUntilLoadingIsDone(delta);
                    break;
            }
            ProximityManager.Update(gameTime);
            MapTileTransitionManager.Update(gameTime);
            DebugBugger.Update(gameTime);
            CombatGuard.Update(gameTime);
            UIManager.Update(gameTime);
            CombatMonsterManager.Update(gameTime, delta);
            PlayerManager.Update(gameTime, delta);
            PlayMonsterManager.Update(gameTime);
            DialogueManager.Update();
            TitleScreenManager.Update(gameTime);
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
            if (SceneManager.CurrentState == SceneState.TitleScreen)
            {
                TitleScreenManager.Draw(_spriteBatch);
                _spriteBatch.End();
                return;
            }
            TileManager.Draw(_spriteBatch);
            TileCellManager.Draw(_spriteBatch);
            LoadingScreenManager.Draw(_spriteBatch);
            NPCManager.Draw(_spriteBatch);
            PlayMonsterManager.Draw(_spriteBatch);
            UIManager.Draw(_spriteBatch, GraphicsDevice);
            CombatGuard.Draw(_spriteBatch, GraphicsDevice);
            CombatMonsterManager.Draw(_spriteBatch);
            PlayerManager.Draw(_spriteBatch);




            DebugBugger.Draw(_spriteBatch);
            DialogueManager.Draw(_spriteBatch);
            EscapeOverseer.Draw(_spriteBatch);
            MapTileTransitionManager.Draw(_spriteBatch, GraphicsDevice);
            CinematicRuler.Draw(_spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);


        }







    }
}
