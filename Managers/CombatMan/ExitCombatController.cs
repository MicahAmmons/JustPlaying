using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ButtonsFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingAround.Managers.CombatMan
{
    public class ExitCombatController
    {
        public Button Button;
        public Texture2D BehindButtonTexture = AssetManager.GetTexture("fightBackground");
        public Rectangle BehindButtonRectangle;
        public SpriteFont Font = AssetManager.GetFont("mainFont");
        public bool PlayerWon;

        private readonly CombatMonsterType _winnerType;
        public readonly Dictionary<string, int> DefeatedMonsterList;
        private readonly string _titleText;
        private readonly List<string> _linesToShow = new List<string>();

        public bool LeaveCombat => Button.CurrentlySelected;

        // Layout knobs
        private readonly int _panelPadding = 24;
        private readonly int _lineSpacing = 6;       // extra pixels between lines
        private readonly float _titleScale = 1.15f;  // make the title a touch larger

        public ExitCombatController(CombatMonsterType winnerType, Dictionary<string, int> info)
        {
            _winnerType = winnerType;
            DefeatedMonsterList = info;
            PlayerWon = winnerType == CombatMonsterType.Player ? true : false;
            // Panel + button placement (kept from your original)
            BehindButtonRectangle = new Rectangle(710, 440, 500, 200);
            Button = new Button(new Rectangle(885, 580, 150, 50));

            // Title text
            _titleText = (_winnerType == CombatMonsterType.AI) ? "Defeated" : "Victory";

            // If player/summons won, prepare the kill list lines
            if (_winnerType != CombatMonsterType.AI && DefeatedMonsterList.Count > 0)
            {
                // header
                _linesToShow.Add("Defeated monsters:");
                // body
                foreach (var kvp in DefeatedMonsterList.OrderByDescending(k => k.Value))
                {
                    // `kvp.Key` is the monster UniqueId you tracked. If you have a display name map,
                    // swap it in here instead of showing UniqueId.
                    _linesToShow.Add($"{kvp.Key}: {kvp.Value}");
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            // Panel
            sb.Draw(BehindButtonTexture, BehindButtonRectangle, Color.White);

            // Title (centered at top of panel with some padding)
            var titleSize = Font.MeasureString(_titleText) * _titleScale;
            var titlePos = CenterX(BehindButtonRectangle, titleSize.X)
                           + new Vector2(0, _panelPadding);
            sb.DrawString(Font, _titleText, titlePos, Color.White, 0f, Vector2.Zero, _titleScale, SpriteEffects.None, 0f);

            // Kill list (only on victory)
            if (_winnerType != CombatMonsterType.AI && _linesToShow.Count > 0)
            {
                // Start just under the title
                float y = titlePos.Y + titleSize.Y + _panelPadding;

                foreach (var line in _linesToShow)
                {
                    var lineSize = Font.MeasureString(line);
                    var linePos = new Vector2(BehindButtonRectangle.X + _panelPadding, y);
                    sb.DrawString(Font, line, linePos, Color.White);
                    y += lineSize.Y + _lineSpacing;
                }
            }

            // Exit button
            Button?.Draw(sb);
        }

        public void Update()
        {
            var mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);
            bool leftPressedThisFrame = InputManager.IsLeftClick();

            if (Button.UpdateInput(mousePoint, leftPressedThisFrame))
            {
                Button.CurrentlySelected = true;
            }
        }


        private Vector2 CenterX(Rectangle area, float width)
        {
            float x = area.X + (area.Width - width) * 0.5f;
            return new Vector2(x, area.Y);
        }
    }
}
