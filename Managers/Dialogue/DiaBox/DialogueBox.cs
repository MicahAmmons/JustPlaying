using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace PlayingAround.Managers.Dialogue
{
    public static class DialogueBox
    {
        private static SpriteFont _font;
        private static Texture2D _background;
        private static Rectangle _boxRect;

        private static string _speakerName;
        private static string _text;
        private static List<string> _responses;
        private static List<Rectangle> _responseRects = new();

        private static bool _hasActiveDialogue => !string.IsNullOrEmpty(_text);

        public static void LoadContent()
        {
            _font = AssetManager.GetFont("mainFont");
            _background = AssetManager.GetTexture("fightBackground");

            int screenWidth = ViewportManager.ScreenWidth; // Replace with dynamic size if you want
            int screenHeight = ViewportManager.ScreenHeight;

            int boxWidth = screenWidth - 200;
            int boxHeight = 200;
            int boxX = 100;
            int boxY = screenHeight - boxHeight - 50;

            _boxRect = new Rectangle(boxX, boxY, boxWidth, boxHeight);
        }
        public static int? GetClickedResponseIndex(Vector2 mousePos)
        {
            for (int i = 0; i < _responseRects.Count; i++)
            {
                if (_responseRects[i].Contains(mousePos)) return i;
            }
            return null;
        }



        public static void SetDialogue(string speaker, string text, List<string> responses)
        {
            if (speaker != null) { _speakerName = speaker; }
            _text = text;
            _responses = responses;
        }

        public static void ClearDialogue()
        {
            _speakerName = null;
            _text = null;
            _responses = null;

        }


        public static void Draw(SpriteBatch spriteBatch)
        {
            if (!_hasActiveDialogue)
                return;
            _responseRects.Clear();
            if (_responses != null)
            {
                for (int i = 0; i < _responses.Count; i++)
                {
                    Vector2 responsePos = new Vector2(_boxRect.X + 40, _boxRect.Y + 100 + i * 30);
                    string text = $"{i + 1}. {SanitizeText(_responses[i])}";
                    Vector2 size = _font.MeasureString(text);
                    Rectangle rect = new Rectangle((int)responsePos.X, (int)responsePos.Y, (int)size.X, (int)size.Y);

                    _responseRects.Add(rect);
                    spriteBatch.DrawString(_font, text, responsePos, Color.Yellow);
                }
            }

            spriteBatch.Draw(_background, _boxRect, Color.DarkSlateBlue);

            // Draw speaker name
            Vector2 namePos = new Vector2(_boxRect.X + 20, _boxRect.Y + 10);
            spriteBatch.DrawString(_font, _speakerName, namePos, Color.White);

            // Draw main text
            Vector2 textPos = new Vector2(_boxRect.X + 20, _boxRect.Y + 50);
            spriteBatch.DrawString(_font, SanitizeText(_text), textPos, Color.LightGray);

            spriteBatch.DrawString(_font, SanitizeText(_speakerName), namePos, Color.White);
            spriteBatch.DrawString(_font, SanitizeText(_text), textPos, Color.LightGray);

            if (_responses != null)
            {
                for (int i = 0; i < _responses.Count; i++)
                {
                    Vector2 responsePos = new Vector2(_boxRect.X + 40, _boxRect.Y + 100 + i * 30);
                    spriteBatch.DrawString(_font, $"{i + 1}. {SanitizeText(_responses[i])}", responsePos, Color.Yellow);
                }
            }
        }
        private static string SanitizeText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Replace curly quotes and em dashes with safe versions
            return input
                .Replace("“", "\"").Replace("”", "\"")
                .Replace("‘", "'").Replace("’", "'")
                .Replace("—", "-").Replace("–", "-");
        }

    }
}
