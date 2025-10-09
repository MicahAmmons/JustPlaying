using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.Notifications
{
    public static class NotificationTextManager
    {
        private static List<NotificationTextBox> _activeNotificationBoxes = new List<NotificationTextBox>();
        private static readonly HashSet<NotificationTextBox> _toggledThisFrameBoxes = new();

        public static void LoadContent()
        {
        }
        public static void Update(float delta)
        {
            ToggleActive();
            UpdateActiveProgression(delta);
        }
        private static void UpdateActiveProgression(float delta)
        {
            for (int i = _activeNotificationBoxes.Count - 1; i >= 0; i--)
            {
                var box = _activeNotificationBoxes[i];
                if (box == null) {_activeNotificationBoxes.RemoveAt(i); continue;}
                if (box.Active)
                {
                    box.ResetCurrentFadeTimer();
                }
                else
                    {
                    box.FadeTimerCurrent -= delta;
                    if (box.FadeTimerCurrent <= 0f)
                    {
                        _activeNotificationBoxes.RemoveAt(i);
                        continue;
                    }
                }
            }
        }
        private static void ToggleActive()
        {
            foreach (var box in _activeNotificationBoxes)
            {
                box.MarkInactive();
            }
            foreach (var box in _toggledThisFrameBoxes)
            {
                if (box == null) continue;
                if (_activeNotificationBoxes.Contains(box)) { box.MarkActive(); continue; }
                _activeNotificationBoxes.Add(box);
                box.SetCacheAnchorPoint();
                box.MarkActive();
            }
            _toggledThisFrameBoxes.Clear();
        }
        public static void Draw(SpriteBatch sb)
        {
            if (_activeNotificationBoxes.Count == 0) return;
            foreach (var box in _activeNotificationBoxes)
            {
                float t = (box.FadeTimerMax > 0f)
            ? MathHelper.Clamp(box.FadeTimerCurrent / box.FadeTimerMax, 0f, 1f)
            : 1f;
                if (t <= 0f) continue;
                Vector2 position = box.GetTypeSpecificDrawPoints();

                const float MaxLineWidth = 200f;
                const int Padding = 5;

                Rectangle rect = box.Rect;

                float rotation = box.GetRotation();
                float scale = 1f;
                SpriteEffects fx = SpriteEffects.None;

                Texture2D texture = box.GetTexture();
                Color bgColor = Color.White * t;
                Color textColor = ColorPalette.LightColor * t;
                Color keyColor = Color.White * t;
                SpriteFont font = box.Font;

                string beforeText = box.BeforeKeyText ?? string.Empty;
                string keyText = $"{box.Key}";
                string afterText = box.AfterKeyText ?? string.Empty;

                string middle = string.IsNullOrEmpty(keyText) ? string.Empty : keyText;
                string fullText = $"{beforeText} {middle} {afterText}".Trim();
                Vector2 origin = Vector2.Zero;
                var wrapped = WrapText(font, fullText, MaxLineWidth, out Vector2 textBlockSize);

                var finalRect = new Rectangle(
                        (int)MathF.Round(position.X - (textBlockSize.X * 0.5f) - Padding),
                        (int)MathF.Round(position.Y - (textBlockSize.Y * 0.5f) - Padding),
                        (int)MathF.Ceiling(textBlockSize.X) + Padding * 2,
                        (int)MathF.Ceiling(textBlockSize.Y) + Padding * 2
                );
                origin = new Vector2(finalRect.Width / 2f, finalRect.Height / 2f);
                //Drawing the Rectangle texture
                // 1) Background: scale to match finalRect and rotate around center
                if (texture != null)
                {
                    var bgOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                    var bgScale = new Vector2(
                        finalRect.Width / (float)texture.Width,
                        finalRect.Height / (float)texture.Height
                    );

                    sb.Draw(
                        texture,
                        position,                 // center pivot
                        null,
                        bgColor,
                        rotation,
                        bgOrigin,                 // rotate around center of the texture
                        bgScale,                  // scale to our computed rect size
                        fx,
                        0f
                    );
                }

                // 2) Text: compute local offsets relative to the same center pivot
                float localY = -textBlockSize.Y * 0.5f + Padding;
                foreach (var (line, lineWidth) in wrapped)
                {
                    float localX = -lineWidth * 0.5f; // center line horizontally
                    sb.DrawString(
                        font,
                        line,
                        position + new Vector2(localX, localY),
                        textColor,
                        rotation,
                        Vector2.Zero,            // IMPORTANT: origin = zero for text
                        1f,
                        fx,
                        0f
                    );
                    localY += font.LineSpacing;
                }
            }
        }
        public static void AddNotificationBox(NotificationTextBox box)
        {
            _toggledThisFrameBoxes.Add(box);
        }
        private static List<(string line, float width)> WrapText(SpriteFont font, string text, float maxWidth, out Vector2 blockSize)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<(string, float)>();
            var sb = new StringBuilder();
            float spaceWidth = font.MeasureString(" ").X;
            float lineWidth = 0f;
            float maxLineWidth = 0f;

            foreach (var word in words)
            {
                float wordWidth = font.MeasureString(word).X;
                bool needsSpace = sb.Length > 0;
                float projected = lineWidth + (needsSpace ? spaceWidth : 0f) + wordWidth;

                if (projected > maxWidth && sb.Length > 0)
                {
                    lines.Add((sb.ToString(), lineWidth));
                    maxLineWidth = MathF.Max(maxLineWidth, lineWidth);
                    sb.Clear();
                    lineWidth = 0f;
                    needsSpace = false;
                    projected = wordWidth;
                }

                if (needsSpace) { sb.Append(' '); lineWidth += spaceWidth; }
                sb.Append(word);
                lineWidth += wordWidth;
            }

            if (sb.Length > 0)
            {
                lines.Add((sb.ToString(), lineWidth));
                maxLineWidth = MathF.Max(maxLineWidth, lineWidth);
            }

            float height = lines.Count > 0 ? lines.Count * font.LineSpacing : 0f;
            blockSize = new Vector2(maxLineWidth, height);
            return lines;
        }

    }
}
