using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder.GlowTex;
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
using PlayingAround.Managers.Triggers;
using PlayingAround.Managers.UI;
using PlayingAround.Utils;
using System;

namespace PlayingAround
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private static float _timer = 0f;
        private Effect _smokeFx;
        private Texture2D _noiseA, _noiseB;



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
            CreateNoise.LoadContent(GraphicsDevice);
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
            TriggerLibrary.LoadContent();


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
            TileCellManager.Update(gameTime);
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


            if (DrawTitleScreen(gameTime)) return;
            DrawBackgroundSmoke(gameTime);
            DrawMapBackground();
            DrawMapGlowTextures(gameTime);


            DrawBehindPlayer(gameTime);

            DrawPlayer(gameTime);

            DrawForeground();




            base.Draw(gameTime);
        }
        private void DrawForeground()
        {
            _spriteBatch.Begin();

            DialogueManager.Draw(_spriteBatch);
            UIManager.Draw(_spriteBatch);

            CinematicRuler.Draw(_spriteBatch);
            MapTileTransitionManager.Draw(_spriteBatch, GraphicsDevice);
            DebugBugger.Draw(_spriteBatch);

            _spriteBatch.End();
        }
        private void DrawMapGlowTextures(GameTime gameTime)
        {
            var glowEffect = AssetManager.GetEffect("ColorReplace");
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, glowEffect);
            GlowTextureController.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();
        }
        private void DrawPlayer(GameTime gameTime)
        {
            var fx = AssetManager.GetEffect("Smoke");
            fx.Parameters["GlobalTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            // Good starting values:
            fx.Parameters["Frequency"].SetValue(new Vector2(6f, 4f));
            fx.Parameters["Speed"].SetValue(new Vector2(0.69f, 0.9f));
            fx.Parameters["DistortAmount"].SetValue(0.02f);
            fx.Parameters["Opacity"].SetValue(1.0f);

            _spriteBatch.Begin(SpriteSortMode.Immediate,
                               BlendState.NonPremultiplied,   // typical PNGs; switch to AlphaBlend if premultiplied
                               SamplerState.LinearClamp,
                               null, null, fx, Matrix.Identity);

            PlayerManager.DrawPlayer(_spriteBatch, fx);

            _spriteBatch.End();
            _spriteBatch.Begin();

            PlayerManager.DrawPlayer(_spriteBatch);
           _spriteBatch.End();




        }
        private void DrawMapBackground()
        {
            _spriteBatch.Begin();
            TileManager.DrawBackground(_spriteBatch);
            _spriteBatch.End();
        }
        public void DrawBackgroundSmoke(GameTime gameTime)
        {
            var fx = AssetManager.GetEffect("Smoke");
            fx.Parameters["GlobalTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            // Good starting values:
            fx.Parameters["Frequency"].SetValue(new Vector2(6f, 4f));
            fx.Parameters["Speed"].SetValue(new Vector2(0.6f, 0.45f));
            fx.Parameters["DistortAmount"].SetValue(0.08f);
            fx.Parameters["Opacity"].SetValue(1.0f);

            _spriteBatch.Begin(SpriteSortMode.Immediate,
                               BlendState.NonPremultiplied,   // typical PNGs; switch to AlphaBlend if premultiplied
                               SamplerState.LinearClamp,
                               null, null, fx, Matrix.Identity);

            TileManager.DrawBackgroundSmoke(_spriteBatch, fx);

            _spriteBatch.End();

        }
        private void DrawBehindPlayer(GameTime gameTime)
        {
            _spriteBatch.Begin();
            CombatGuard.Draw(_spriteBatch, GraphicsDevice);
            TileCellManager.Draw(_spriteBatch);
            CombatMonsterManager.Draw(_spriteBatch);
            PlayMonsterManager.Draw(_spriteBatch);
            NPCManager.Draw(_spriteBatch);
            _spriteBatch.End();

        }



        private bool DrawTitleScreen(GameTime gameTime)
        {
            if (SceneManager.CurrentState != SceneState.TitleScreen) return false;
            _spriteBatch.Begin();
            TitleScreenManager.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
            return true;
        }
    }
}
