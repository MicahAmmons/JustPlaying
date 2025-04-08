// File: Manager/InputManager.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace PlayingAround.Manager
{
    public static class InputManager
    {
        private static KeyboardState _currentKeyboard;
        private static KeyboardState _previousKeyboard;

        private static MouseState _currentMouse;
        private static MouseState _previousMouse;

        public static event Action OnMoveLeft;
        public static event Action OnMoveRight;
        public static event Action OnMoveUp;
        public static event Action OnMoveDown;




        public static void Update(GameTime gameTime)
        {
            _previousKeyboard = _currentKeyboard;
            _currentKeyboard = Keyboard.GetState();

            _previousMouse = _currentMouse;
            _currentMouse = Microsoft.Xna.Framework.Input.Mouse.GetState();

            if (IsKeyHeld(Keys.Left) || IsKeyHeld(Keys.A))
                OnMoveLeft?.Invoke();

            if (IsKeyHeld(Keys.Right) || IsKeyHeld(Keys.D))
                OnMoveRight?.Invoke();

            if (IsKeyHeld(Keys.Up) || IsKeyHeld(Keys.W))
                OnMoveUp?.Invoke();

            if (IsKeyHeld(Keys.Down) || IsKeyHeld(Keys.S))
                OnMoveDown?.Invoke();


        }

        // -- Keyboard Utilities --
        public static bool IsKeyPressed(Keys key) =>
            _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

        public static bool IsKeyHeld(Keys key) =>
            _currentKeyboard.IsKeyDown(key);

        public static bool IsKeyReleased(Keys key) =>
            _currentKeyboard.IsKeyUp(key) && _previousKeyboard.IsKeyDown(key);

        // -- Mouse Utilities --
        public static bool IsLeftClick() =>
            _currentMouse.LeftButton == ButtonState.Pressed &&
            _previousMouse.LeftButton == ButtonState.Released;

        public static bool IsRightClick() =>
            _currentMouse.RightButton == ButtonState.Pressed &&
            _previousMouse.RightButton == ButtonState.Released;

        public static int MouseX => _currentMouse.X;
        public static int MouseY => _currentMouse.Y;

        public static MouseState Mouse => _currentMouse;


    }
}
