using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.TitleScreen;
using System.Collections.Generic;

namespace PlayingAround.Managers.EscapeOverseer
    {
        public class EscapeOverseer
        {
            private static SceneManager.SceneState _currentSceneState => SceneManager.CurrentState;
            private static EscapeState _currentEscapeState = EscapeState.None;
            public static EscapeState CurrentEscapeState => _currentEscapeState;

        private static readonly Point ButtonSize = new(250, 50);
        private static readonly int ButtonSpacing = 15;
        private static SpriteFont Font => AssetManager.GetFont("titleScreenButtonFont");
        private static Texture2D Background => AssetManager.GetTexture("fightBackground");

        private static string[] _options = new[] { "Save", "Settings", "Exit to Main Menu", "Exit to Desktop", "Return", "Yes", "No"};
        private static Dictionary<string, Rectangle> _menuOptions = new Dictionary<string, Rectangle> () ;

        private static bool _confirmEscape = false;
        private static string _escapeTo = null;

        public static void LoadContent()
        {

            int menuWidth = ButtonSize.X;
            int menuHeight = _options.Length * (ButtonSize.Y + ButtonSpacing) - ButtonSpacing;
            int centerX = ViewportManager.ScreenWidth / 2;
            int centerY = ViewportManager.ScreenHeight / 2;
            int startY = centerY - menuHeight / 2;
            for (int i = 0; i < _options.Length; i++)
            {
                int offset = 0;
                if (_options[i] == "Yes" || _options[i] == "No")
                {
                    offset -= 1000;
                }
                Rectangle buttonRect = new Rectangle(
                    centerX - menuWidth / 2,
                    startY + i * (ButtonSize.Y + ButtonSpacing),
                    ButtonSize.X + offset,
                    ButtonSize.Y
                );

                _menuOptions.Add(_options[i], buttonRect);
            } 

        }
        public static void Draw(SpriteBatch spriteBatch)
            {

                switch (_currentEscapeState)
                {
                    case EscapeState.None:
                        break;

                    case EscapeState.EscapeOutOfCombat:
                        DrawEscapeOutOfCombat(spriteBatch);
                        break;

                    case EscapeState.EscapeInCombat:
                        DrawEscapeInCombat(spriteBatch);
                        break;
                }
                DrawEscapeConfirmation(spriteBatch);

        }

        public static void DrawEscapeConfirmation(SpriteBatch spriteBatch)
        {
            if (!_confirmEscape) return;
            string escapeTo = _escapeTo;
          
            spriteBatch.Draw(Background, _menuOptions["Yes"], ColorPalette.DarkColor);
            spriteBatch.Draw(Background, _menuOptions["No"], ColorPalette.DarkColor);
            spriteBatch.DrawString(Font, $"Would you like to {escapeTo}? Progress will not be saved.", new Vector2(100, 540), ColorPalette.LightColor);
        }
        public static void DrawEscapeInCombat(SpriteBatch spriteBatch)
        {
            DrawEscapeOutOfCombat(spriteBatch); // reuse for now
        }


        public static void DrawEscapeOutOfCombat(SpriteBatch spriteBatch)
        {


            foreach (var kvp in _menuOptions) 
            {
                string saveKey = kvp.Key;
                Rectangle rect = kvp.Value;
                if (saveKey == "Yes" || saveKey == "No")
                {
                    continue;
                }
                DrawEscapeButton(spriteBatch, rect, saveKey);
            }
        }
        private static void DrawEscapeButton(SpriteBatch spriteBatch, Rectangle rect, string text)
{
    // Background box (optional – feel free to style)
    spriteBatch.Draw(Background, rect, ColorPalette.DarkColor * 0.9f);

    // Centered text
    Vector2 textSize = Font.MeasureString(text);
    Vector2 textPos = new Vector2(
        rect.X + (rect.Width - textSize.X) / 2,
        rect.Y + (rect.Height - textSize.Y) / 2
    );

    // Shadow
    spriteBatch.DrawString(Font, text, textPos + new Vector2(2, 2), Color.Black);
    // Foreground
    spriteBatch.DrawString(Font, text, textPos, Color.White);
}



        public static void Update(GameTime gameTime)
            {
                Point mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);
                UpdatePlayerInput(mousePoint);
            }

        public static void UpdatePlayerInput(Point mousePoint)
            {
                UpdatePlayerOpenEscapeMenuPress(mousePoint);
                HandlePlayerConfirmationExitClick(mousePoint);
                switch (_currentEscapeState)
                {
                    case EscapeState.EscapeOutOfCombat:
                        UpdatePlayerOutOfCombatInput(mousePoint);
                        break;

                    case EscapeState.EscapeInCombat:
                        // Handle logic while menu is open in combat
                        break;
                }
            }

        public static void HandlePlayerConfirmationExitClick(Point mousePoint)
        {
            if (!_confirmEscape) return;
             if (InputManager.IsLeftClick())
            {
                if (_menuOptions["Yes"].Contains(mousePoint))
                {
                    ExitGame();
                    _confirmEscape = false;
                }
                if (_menuOptions["No"].Contains(mousePoint))
                {
                    _confirmEscape = false;
                }
            }

        }
        public static void UpdatePlayerOutOfCombatInput(Point mousePoint)
        {
            if (InputManager.IsLeftClick())
            {
                foreach (var kvp in _menuOptions)
                {
                    Rectangle rect = kvp.Value;
                    if (rect.Contains(mousePoint))
                    {
                        HandleMenuOptionClick(kvp.Key);
                    }
                }
            }
        }
        private static void HandleMenuOptionClick(string key)
        {

            switch (key)
            {
                case "Save":

                    break;

                case "Settings":

                    break;
                case "Exit to Main Menu":
                    _escapeTo = $"{key}";
                    _confirmEscape = true;
                    break;

                case "Exit To Desktop":
                    _escapeTo = $"{key}";
                    _confirmEscape = true;
                    break;

                case "Return":
                    _currentEscapeState = EscapeState.None;
                    break;
            }
        }
        public static void UpdatePlayerOpenEscapeMenuPress(Point mousePoint)
            {
                if (InputManager.IsKeyReleased(Keys.Escape))
                {
                    switch (_currentSceneState)
                    {
                        case SceneManager.SceneState.Play:
                            _currentEscapeState = _currentEscapeState == EscapeState.EscapeOutOfCombat
                                ? EscapeState.None
                                : EscapeState.EscapeOutOfCombat;
                            break;

                        case SceneManager.SceneState.Combat:
                            _currentEscapeState = _currentEscapeState == EscapeState.EscapeInCombat
                                ? EscapeState.None
                                : EscapeState.EscapeInCombat;
                            break;
                    }
                }
            }
        public static void ExitGame()
        {
            _currentEscapeState = EscapeState.None;

            switch (_escapeTo)
            {
                case "Exit to Main Menu":
                    ExitToMainMenu();
                    break;
                case ("Exit to Desktop"):
                    ExitToDesktop();
                    break;
            }
        }

        public static void ExitToDesktop()
        {
          
        }
        public static void ExitToMainMenu()
        {
            SceneManager.SetState(SceneManager.SceneState.TitleScreen);
           
        }
    }



        public enum EscapeState
        {
            None,
            EscapeOutOfCombat,
            EscapeInCombat
        }
    }
