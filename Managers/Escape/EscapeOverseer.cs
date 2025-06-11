using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Escape.Settings;
using PlayingAround.Managers.TitleScreen;
using System;
using System.Collections.Generic;

namespace PlayingAround.Managers.EscapeOverseer
    {
        public class EscapeOverseer
        {
            private static EscapeState _currentEscapeState = EscapeState.None;
            public static EscapeState CurrentEscapeState => _currentEscapeState;
        private static EscapeMenuState _currentEscapeMenuState = EscapeMenuState.None;
        public static bool ShouldExit = false;
        private static readonly Point ButtonSize = new(250, 50);
        private static readonly int ButtonSpacing = 15;
        private static SpriteFont Font => AssetManager.GetFont("titleScreenButtonFont");
        private static Texture2D Background => AssetManager.GetTexture("fightBackground");

        private static readonly EscapeMenuState[] _options = new[]
            {
              EscapeMenuState.Save,
              EscapeMenuState.Settings,
              EscapeMenuState.ExitToMainMenu,
              EscapeMenuState.ExitToDeskTop,
             EscapeMenuState.Return,
             EscapeMenuState.Yes,
             EscapeMenuState.No
            };

        private static Dictionary<EscapeMenuState, Rectangle> _menuOptions = new();


        private static bool _confirmEscape = false;
        private static string _escapeTo = null;

        public static void LoadContent()
        {
            _menuOptions.Clear(); // Ensure previous positions are cleared

            int menuWidth = ButtonSize.X;
            int menuHeight = (_options.Length - 2) * (ButtonSize.Y + ButtonSpacing) - ButtonSpacing; // exclude Yes/No
            int centerX = ViewportManager.ScreenWidth / 2;
            int centerY = ViewportManager.ScreenHeight / 2;
            int startY = centerY - menuHeight / 2;

            int normalButtonIndex = 0;
            int yesNoStartY = centerY + 150; // adjust as needed

            foreach (var option in _options)
            {
                Rectangle buttonRect;

                if (option == EscapeMenuState.Yes)
                {
                    buttonRect = new Rectangle(
                        centerX - menuWidth / 2 - 200, // offset left
                        yesNoStartY,
                        ButtonSize.X,
                        ButtonSize.Y
                    );
                }
                else if (option == EscapeMenuState.No)
                {
                    buttonRect = new Rectangle(
                        centerX - menuWidth / 2 - 200, // offset left
                        yesNoStartY + 100, // 100 pixels below Yes
                        ButtonSize.X,
                        ButtonSize.Y
                    );
                }
                else
                {
                    buttonRect = new Rectangle(
                        centerX - menuWidth / 2,
                        startY + normalButtonIndex * (ButtonSize.Y + ButtonSpacing),
                        ButtonSize.X,
                        ButtonSize.Y
                    );
                    normalButtonIndex++;
                }

                _menuOptions[option] = buttonRect;
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
            if (_currentEscapeMenuState == EscapeMenuState.Settings)
            {
                SettingsSuper.Draw(spriteBatch);
            }
                DrawEscapeConfirmation(spriteBatch);

        }

        public static void DrawEscapeConfirmation(SpriteBatch spriteBatch)
        {
            if (!_confirmEscape) return;

            spriteBatch.Draw(Background, _menuOptions[EscapeMenuState.Yes], ColorPalette.DarkColor);
            spriteBatch.Draw(Background, _menuOptions[EscapeMenuState.No], ColorPalette.DarkColor);

            spriteBatch.DrawString(
                Font,
                $"Would you like to {_escapeTo}? Progress will not be saved.",
                new Vector2(100, 540),
                ColorPalette.LightColor
            );

            DrawEscapeButton(spriteBatch, _menuOptions[EscapeMenuState.Yes], "Yes");
            DrawEscapeButton(spriteBatch, _menuOptions[EscapeMenuState.No], "No");
        }

        public static void DrawEscapeInCombat(SpriteBatch spriteBatch)
        {
            DrawEscapeOutOfCombat(spriteBatch); // reuse for now
        }


        public static void DrawEscapeOutOfCombat(SpriteBatch spriteBatch)
        {
            foreach (var kvp in _menuOptions)
            {
                EscapeMenuState state = kvp.Key;
                if (state == EscapeMenuState.Yes || state == EscapeMenuState.No)
                    continue;

                DrawEscapeButton(spriteBatch, kvp.Value, state.ToString().Replace("ExitToDeskTop", "Exit to Desktop").Replace("ExitToMainMenu", "Exit to Main Menu"));
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
            if (_currentEscapeMenuState == EscapeMenuState.Settings)
            {
                SettingsSuper.Update();
            }
        }



        public static void UpdatePlayerInput(Point mousePoint)
        {
            UpdatePlayerOpenEscapeMenuPress(mousePoint);
            if (_currentEscapeMenuState != EscapeMenuState.Settings)
            {
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
        }

        public static void HandlePlayerConfirmationExitClick(Point mousePoint)
        {
            if (!_confirmEscape || !InputManager.IsLeftClick()) return;

            if (_menuOptions[EscapeMenuState.Yes].Contains(mousePoint))
            {
                ExitGame();
                _confirmEscape = false;
            }
            else if (_menuOptions[EscapeMenuState.No].Contains(mousePoint))
            {
                _confirmEscape = false;
                _currentEscapeMenuState = EscapeMenuState.None;
                _currentEscapeState = EscapeState.None;
            }
        }

        public static void UpdatePlayerOutOfCombatInput(Point mousePoint)
        {
            if (!InputManager.IsLeftClick()) return;

            foreach (var kvp in _menuOptions)
            {
                if (kvp.Key == EscapeMenuState.Yes || kvp.Key == EscapeMenuState.No)
                    continue; // skip yes and no

                if (kvp.Value.Contains(mousePoint))
                {
                    HandleMenuOptionClick(kvp.Key);
                    break;
                }
            }
        }

        private static void HandleMenuOptionClick(EscapeMenuState key)
        {

            switch (key)
            {
                case EscapeMenuState.Save:
                    SaveManager.SaveGame();
                    SetEscapeMenuState(EscapeMenuState.None);
                    _currentEscapeState = EscapeState.None;
                    break;

                case EscapeMenuState.Settings:
                    SetEscapeMenuState(EscapeMenuState.Settings);
                    break;
                case EscapeMenuState.ExitToMainMenu:
                    _escapeTo = $"{key}";
                    _confirmEscape = true;
                    break;

                case EscapeMenuState.ExitToDeskTop:
                    _escapeTo = $"{key}";
                    _confirmEscape = true;
                    break;

                case EscapeMenuState.Return:
                    _currentEscapeState = EscapeState.None;
                    _confirmEscape = false;
                    break;
            }
        }
        private static void SetEscapeMenuState(EscapeMenuState state)
        {
            _currentEscapeMenuState = state;
        }
        private static void SetEscapeState(EscapeState state)
        {
            _currentEscapeState = state;
        }
        public static void UpdatePlayerOpenEscapeMenuPress(Point mousePoint)
            {
                if (InputManager.IsKeyReleased(Keys.Escape))
                {
                    switch (SceneManager.CurrentState)
                    {
                        case SceneState.Play:
                             _currentEscapeState = _currentEscapeState == EscapeState.EscapeOutOfCombat
                                ? EscapeState.None
                                : EscapeState.EscapeOutOfCombat;
                        _confirmEscape = false;
                        SettingsSuper.SetSettingSuperState(SettingSuperState.None);
                            break;

                        case SceneState.Combat:
                            _currentEscapeState = _currentEscapeState == EscapeState.EscapeInCombat
                                ? EscapeState.None
                                : EscapeState.EscapeInCombat;
                        _confirmEscape = false;
                        SettingsSuper.SetSettingSuperState(SettingSuperState.None);
                        break;
                    }
                }
            }
        public static void ExitGame()
        {
            _currentEscapeState = EscapeState.None;

            switch (_escapeTo)
            {
                case "ExitToMainMenu":
                    ExitToMainMenu();
                    break;
                case ("ExitToDeskTop"):
                    ExitToDesktop();
                    break;
            }
        }

        public static void ExitToDesktop()
        {
            ShouldExit = true;
        }
        public static void ExitToMainMenu()
        {
            SceneManager.SetState(SceneState.TitleScreen);
           
        }
    }




    }
public enum EscapeState
{
    None,
    EscapeOutOfCombat,
    EscapeInCombat
}
public enum EscapeMenuState
{
    None,
    Save,
    Settings,
    ExitToDeskTop,
    ExitToMainMenu,
    Return,
    Yes,
    No

}