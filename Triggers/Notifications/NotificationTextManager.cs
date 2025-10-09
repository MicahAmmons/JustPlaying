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
        private static SpriteFont Font;

        public static void LoadContent()
        {
            Font = AssetManager.GetFont("mainFont");
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
            foreach(var box in _activeNotificationBoxes)
            {
                float t = (box.FadeTimerMax > 0f)
            ? MathHelper.Clamp(box.FadeTimerCurrent / box.FadeTimerMax, 0f, 1f)
            : 1f;
                if (t <= 0f) continue;
                Vector2 position = box.GetTypeSpecificDrawPoints();
                Rectangle rect = box.Rect;
                Vector2 origin = new Vector2(rect.Width / 2f, rect.Height / 2f);
   
                float rotation = 0f;
                float scale = 1f;
                SpriteEffects fx = SpriteEffects.None;
                Texture2D texture = box.GetTexture();
                string beforeText = box.BeforeKeyText ?? string.Empty;
                string keyText = $"{box.Key}";
                string afterText = box.AfterKeyText ?? string.Empty;
                Color bgColor = Color.White * t;
                Color textColor = ColorPalette.DarkColor * t;
                Color keyColor = ColorPalette.LightColor * t;
                SpriteFont font = Font;
                if (texture != null)
                {
                    sb.Draw(
                        texture,
                        position,
                        sourceRectangle: null,
                        color: bgColor,
                        rotation: rotation,
                        origin: origin,
                        scale: scale,
                        effects: fx,
                        layerDepth: 0f
                    );
                }

                // 2) Draw text: "beforeText  KEY  afterText"
                if (font != null)
                {
                    Vector2 sizeBefore = string.IsNullOrEmpty(beforeText) ? Vector2.Zero : font.MeasureString(beforeText);
                    Vector2 sizeKey = font.MeasureString(keyText);
                    Vector2 sizeAfter = string.IsNullOrEmpty(afterText) ? Vector2.Zero : font.MeasureString(afterText);

                    // total width to center the line at 'position'
                    float totalWidth = sizeBefore.X + sizeKey.X + sizeAfter.X;

                    // vertical alignment: center baseline within the rect
                    float lineHeight = font.LineSpacing;
                    Vector2 cursor = new Vector2(position.X - totalWidth / 2f,
                                                   position.Y - lineHeight / 2f);

                    // Before
                    if (!string.IsNullOrEmpty(beforeText))
                    {
                        sb.DrawString(font, beforeText, cursor, textColor);
                        cursor.X += sizeBefore.X;
                    }

                    // KEY (distinct color)
                    sb.DrawString(font, keyText, cursor, keyColor);
                    cursor.X += sizeKey.X;

                    // After
                    if (!string.IsNullOrEmpty(afterText))
                    {
                        sb.DrawString(font, afterText, cursor, textColor);
                    }
                }
            }
        }
        public static void AddNotificationBox(NotificationTextBox box)
        {
            _toggledThisFrameBoxes.Add(box);
        }
    }
}
