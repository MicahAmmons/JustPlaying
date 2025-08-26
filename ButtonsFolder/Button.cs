using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CombatStateMachine;

using Microsoft.Xna.Framework.Input;

namespace PlayingAround.ButtonsFolder
{
    public class ButtonManager
    {
        private List<Button> _buttons = new List<Button>() { };
        public event Action<Button> ButtonSelected;
        public event Action ButtonDeselected;
        public void SetCurrentButtons(Button button) => _buttons.Add(button);
        public void UpdateInput()
        {
            if (_buttons.Count == 0) return;

            var mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);
            bool leftPressedThisFrame = InputManager.IsLeftClick();
            bool rightPressedThisFrame = InputManager.IsRightClick();

            if (rightPressedThisFrame) 
            {
                ResetButtons();
                ButtonDeselected?.Invoke();
                return; 
            }
            foreach (var b in _buttons)
            {
                if (b.UpdateInput(mousePoint, leftPressedThisFrame))
                {
                    bool alreadySelected = b.CurrentlySelected;
                    ResetButtons();

                    if (alreadySelected) 
                    {
                        ButtonDeselected?.Invoke();
                        return; 
                    }

                    b.CurrentlySelected = true;
                    ButtonSelected?.Invoke(b);

                    return;
                };
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_buttons.Count == 0) return;
            foreach (var b in _buttons)
            {
                b.Draw(spriteBatch);
            }
        }

        public void ResetButtons()
        {
            foreach (var but in _buttons) but.ResetInputState();
        }
    }
}
    public class Button
    {
        public Texture2D Texture = AssetManager.GetTexture("fightBackground");
        public Rectangle DrawRectangle;
        public bool MouseHovered = false;
        public bool CurrentlySelected = false;
        public Button(Rectangle rect)
        {
            DrawRectangle = rect;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Color col = Color.White;
        if (MouseHovered) col = Color.Blue;
        if (CurrentlySelected) col = Color.Red;

       
            spriteBatch.Draw(Texture, DrawRectangle, col);
        }
        public bool UpdateInput(Point mousePoint, bool leftPressedThisFrame)
        {
            MouseHovered = DrawRectangle.Contains(mousePoint);
            if (MouseHovered && leftPressedThisFrame)
            {
            return true;
            }
        return false;
        }
        public void ResetInputState()
        {
            CurrentlySelected= false;
            MouseHovered= false;
        }
    }
