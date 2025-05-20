using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Data.SaveData;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;

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
        private static Dictionary<string, Rectangle> _loadGameRects = new();
        private static Dictionary<string, Rectangle> _newGameRects = new();
        private static Rectangle _loadGamePreviewBackground = new Rectangle();
        private static Rectangle _backgroundPopupForLoadFiles;
        private static Texture2D _backgroundTexture;
        


        private static string _currentGameStateDataKey;


        public static void LoadContent()
        {
            _backGroundTexture = AssetManager.GetTexture("TitleScreenBackGround");
            _backGroundArt = new Rectangle(0, 0, ViewportManager.ScreenWidth, ViewportManager.ScreenHeight);

            _buttonSize = new Point(100, 50);
            _mainFont = AssetManager.GetFont("titleScreenButtonFont");

            // Margin from left and top edges
            int marginLeft = 90;
            int marginTop = 50;
            int verticalSpacing = 10;

            int x = marginLeft;
            int y = marginTop;
 
            int centerX = ViewportManager.ScreenWidth / 2;
            int centerY = ViewportManager.ScreenHeight / 2;


            _buttonRects["NewGame"] = new Rectangle(x, y, _buttonSize.X, _buttonSize.Y);
            y += _buttonSize.Y + verticalSpacing;

            _buttonRects["LoadGame"] = new Rectangle(x, y, _buttonSize.X, _buttonSize.Y);
            y += _buttonSize.Y + verticalSpacing;

            _backgroundPopupForLoadFiles = new Rectangle(x, y, 500, 500);
            y += 10;



            int newGameCenterOffSet = 600;
            int newGameHeight = 200;

            _loadGamePreviewBackground = new Rectangle(centerX, centerY - 200, 300, 300);
            _loadGamePreviewOptions["Yes"] = new Rectangle()
            _loadGamePreviewOptions["No"] = new Rectangle()

            _newGameRects["Yes"] = new Rectangle(centerX - (newGameCenterOffSet / 2), centerY, newGameHeight / 3, newGameHeight);
            _newGameRects["No"] = new Rectangle(centerX , centerY, newGameCenterOffSet / 3, newGameHeight);
            _newGameRects["Start a New Game?"] = new Rectangle(centerX - newGameCenterOffSet/2 -70, centerY - newGameHeight/2, newGameCenterOffSet, newGameHeight);


            _loadGameRects.Clear();
            x += 70;
            foreach (var save in SaveManager.SaveFiles.Keys)
            {
                if (save != "saveGameTemplate")
                {
                    _loadGameRects[save] = new Rectangle(x, y, _buttonSize.X, _buttonSize.Y);
                    y += _buttonSize.Y + verticalSpacing;
                }
            }


            _backgroundTexture = AssetManager.GetTexture("fightBackground");

        




        }


        public static void Update(GameTime gameTime)
        {
            UpdatePlayerInput(gameTime);
        }

        public static void UpdatePlayerInput(GameTime gameTime)
        {
            Point mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);

            UpdatePlayerClickedConstantButtons(mousePoint);

            switch (_titleScreenState)
            {
                case TitleScreenState.MainPage:
                    UpdatePlayerClickMainPage(mousePoint);
                    break;
                case TitleScreenState.NewGame:
                    UpdatePlayerClickNewGame(mousePoint);
                    break;
                case TitleScreenState.LoadGame:
                    UpdatePlayerClickLoadGame(mousePoint);
                    break;
                case TitleScreenState.ConfirmLoadGame:
                    UpdatePlayerClickLoadGameConfirmation(mousePoint);
                    break;
            }
        }
        public static void UpdatePlayerClickLoadGameConfirmation(Point mousePoint)
        {

        }
        public static void UpdatePlayerClickedConstantButtons(Point mousePoint)
        {
            Rectangle newButtonRect = _buttonRects["NewGame"];
            Rectangle loadButtonRect = _buttonRects["LoadGame"];
            if (InputManager.IsLeftClick())
            {
                if (newButtonRect.Contains(mousePoint))
                {
                    SetState(_titleScreenState == TitleScreenState.NewGame ? TitleScreenState.MainPage : TitleScreenState.NewGame);

                }
                if (loadButtonRect.Contains(mousePoint))
                {
                    SetState(_titleScreenState == TitleScreenState.LoadGame ? TitleScreenState.MainPage : TitleScreenState.LoadGame);
                }
            }
        }

        public static void UpdatePlayerClickNewGame(Point mousePoint)
        {

            if (!InputManager.IsLeftClick())
                return;
           
                if (_newGameRects["No"].Contains(mousePoint))
                {
                     SetState(TitleScreenState.MainPage);
                }
                else if (_newGameRects["Yes"].Contains(mousePoint))
                {
                PickSaveDataSource("saveGameTemplate");
                    
                }
           
        }
        public static void UpdatePlayerClickLoadGame(Point mousePoint)
        {
                foreach (var kvp in _loadGameRects)
                {
                    string saveKey = kvp.Key;
                    Rectangle rect = kvp.Value;
                    if (InputManager.IsLeftClick() && rect.Contains(mousePoint))
                    {
                        _currentGameStateDataKey = saveKey;
                    SetState(TitleScreenState.ConfirmLoadGame);
                    }
                    return;
                }
            
        }
        private static void UpdatePlayerClickMainPage(Point mousePoint)
        {


        }

        public static void Draw(SpriteBatch spriteBatch)
        {

            DrawTitleScreenBackGround(spriteBatch);
            switch (_titleScreenState)
            {
                case TitleScreenState.MainPage:
                    DrawMainButtons(spriteBatch);
                    break;
                case TitleScreenState.LoadGame:
                    DrawMainButtons(spriteBatch);
                    DrawLoadGameOptions(spriteBatch);
                    break;
                case TitleScreenState.NewGame:
                    DrawMainButtons(spriteBatch);
                    DrawNewGameOption(spriteBatch); 
                    break;
                case TitleScreenState.ConfirmLoadGame:
                    DrawLoadGameConfirmation(spriteBatch);
                    break;
            }
            DrawCurrentStateLabel(spriteBatch);
        }

        public static void DrawLoadGameConfirmation(SpriteBatch spriteBatch)
        {
            DrawSaveStatePreview(spriteBatch);
            DrawLoadYesOrNo(spriteBatch);
        }
        public static void DrawLoadYesOrNo(SpriteBatch spriteBatch)
        {
            var current = _currentGameStateDataKey;
            spriteBatch.Draw(_backgroundTexture, _loadGamePreviewBackground, Color.White);
            DrawButton(spriteBatch, _newGameRects["Yes"], "Yes");
            DrawButton(spriteBatch, _newGameRects["No"], "No");
        }


        public static void DrawSaveStatePreview(SpriteBatch spriteBatch)
        {


        }
        public static void DrawNewGameOption(SpriteBatch spriteBatch)
        {

            foreach (var kpv in _newGameRects)
            {
                string saveKey = kpv.Key;
                Rectangle rect = kpv.Value;
                string label = saveKey;
                DrawButton(spriteBatch, rect, label);
            }
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
        public static void DrawLoadGameOptions(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backgroundTexture , _backgroundPopupForLoadFiles, ColorPalette.DarkColor * 0.8f );
            foreach (var kvp in _loadGameRects)
            {
                string saveKey = kvp.Key;    
                Rectangle rect = kvp.Value;

                string label = saveKey.Replace("gameSave", "");

                DrawButton(spriteBatch, rect, label);
            }
        }



        private static void DrawMainButtons(SpriteBatch spriteBatch)
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

        private static void PickSaveDataSource(string key)
        {
            SaveManager.SetCurrentGameSave("saveGameTemplate");
            SceneManager.SetState(SceneManager.SceneState.LoadingScreen);

        }
        public enum TitleScreenState
        {
            MainPage,
            Play,
            NewGame,
            LoadGame,
            ConfirmLoadGame
        }
    }
}
