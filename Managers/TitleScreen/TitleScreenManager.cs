using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;

namespace PlayingAround.Managers.TitleScreen
{
    public static class TitleScreenManager
    {
        private static Texture2D _backGroundTexture;
        private static Rectangle _backGroundArt;
        private static TitleScreenState _titleScreenState = TitleScreenState.MainPage;

        private static Point _buttonSize;
        private static SpriteFont _mainFont;

        private static Dictionary<string, Rectangle> _buttonRects = new();

        public static void LoadContent()
        {
            _backGroundTexture = AssetManager.GetTexture("TitleScreenBackGround");
            _backGroundArt = new Rectangle(0, 0, ViewportManager.ScreenWidth, ViewportManager.ScreenHeight);

            _buttonSize = new Point(100, 50);
            _mainFont = AssetManager.GetFont("titleScreenButtonFont");

            // Initialize button rectangles
            int centerX = (ViewportManager.ScreenWidth - _buttonSize.X) / 2;
            int centerY = (ViewportManager.ScreenHeight - _buttonSize.Y) / 2;

            _buttonRects["Play"] = new Rectangle(50, 50, _buttonSize.X, _buttonSize.Y);

            int halfScreen = ViewportManager.ScreenWidth / 2;
            int verticalY = centerY;

            _buttonRects["NewGame"] = new Rectangle((halfScreen - _buttonSize.X) / 2, verticalY, _buttonSize.X, _buttonSize.Y);
            _buttonRects["LoadGame"] = new Rectangle(halfScreen + ((halfScreen - _buttonSize.X) / 2), verticalY, _buttonSize.X, _buttonSize.Y);
        }

        public static void Update(GameTime gameTime)
        {
            UpdatePlayerInput(gameTime);
        }

        public static void UpdatePlayerInput(GameTime gameTime)
        {
            switch (_titleScreenState)
            {
                case TitleScreenState.MainPage:
                    UpdatePlayerClickPlay();
                    break;
            }
        }

        private static void UpdatePlayerClickPlay()
        {
            Rectangle playButtonRect = _buttonRects["Play"];
            Point mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);

            if (InputManager.IsLeftClick() && playButtonRect.Contains(mousePoint))
            {
                SetState(TitleScreenState.Play);
            }
        }


        public static void Draw(SpriteBatch spriteBatch)
        {

            DrawTitleScreenBackGround(spriteBatch);
            switch (_titleScreenState)
            {
                case TitleScreenState.MainPage:
                    DrawPlayButton(spriteBatch);
                    break;
                case TitleScreenState.Play:
                    DrawNewOrLoadGameOption(spriteBatch);
                    break;
            }
            DrawCurrentStateLabel(spriteBatch);
        }
        private static void DrawCurrentStateLabel(SpriteBatch spriteBatch)
        {
            string stateText = _titleScreenState.ToString();
            Vector2 textSize = _mainFont.MeasureString(stateText);

            // Position near top-right with 10px padding
            Vector2 position = new Vector2(
                ViewportManager.ScreenWidth - textSize.X - 10,
                10
            );

            // Shadow
            spriteBatch.DrawString(_mainFont, stateText, position + new Vector2(1, 1), Color.Black);
            // Main white text
            spriteBatch.DrawString(_mainFont, stateText, position, Color.White);
        }


        private static void DrawPlayButton(SpriteBatch spriteBatch)
        {
            DrawButton(spriteBatch, _buttonRects["Play"], "Play");
        }

        private static void DrawNewOrLoadGameOption(SpriteBatch spriteBatch)
        {
            DrawButton(spriteBatch, _buttonRects["NewGame"], "New Game");
            DrawButton(spriteBatch, _buttonRects["LoadGame"], "Load Game");
        }

        private static void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text)
        {
            Vector2 textSize = _mainFont.MeasureString(text);
            Vector2 textPos = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2,
                rect.Y + (rect.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(_mainFont, text, textPos + new Vector2(5, 5), ColorPalette.DarkColor);
            spriteBatch.DrawString(_mainFont, text, textPos, ColorPalette.LightColor);
        }

        private static void DrawTitleScreenBackGround(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backGroundTexture, _backGroundArt, Color.White);
        }

        private static void SetState(TitleScreenState state)
        {
            _titleScreenState = state;
        }
        public enum TitleScreenState
        {
            MainPage,
            Play,
            NewGame,
            LoadGame
        }
    }
}
