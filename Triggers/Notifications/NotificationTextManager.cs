using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers;
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
            if (!SceneManager.IsState(SceneState.Play))
            {
                if (_activeNotificationBoxes.Count > 0)
                {
                    _activeNotificationBoxes.Clear();
                }
                return;
            }
            ToggleActive();
            UpdateActiveProgression(delta);
            UpdateLifeTimeTimers(delta);
        }
        private static void UpdateLifeTimeTimers(float delta)
        {
            if (_activeNotificationBoxes.Count > 0) 
            foreach (var box in _activeNotificationBoxes)
            {
                box.UpdateLifeTimeTimers(delta);
            }
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
                        box.ClearLifeTimeTimer();
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
            if (!SceneManager.IsState(SceneState.Play)) return;
            if (_activeNotificationBoxes.Count == 0) return;

            foreach (var box in _activeNotificationBoxes)
            {
                box.GetTypeSpecificDrawPoints();

                SpriteFont font = box.Font;
                float timer = box.BoxLifeTimer;
                float fadeCurrent = box.FadeTimerCurrent; // remaining fade-out time
                float fadeTimeMax = box.FadeTimerMax;     // total fade-out duration
                float fadeInTimer = box.FadeInTimer;      // per-line fade-in duration

                List<TextData> data = box.TextData;
                for (int i = 0; i < data.Count; i++)
                {
                    var text = data[i];
                    if (timer < text.FadeDelay)
                        continue;

                    float timeSinceActive = timer - text.FadeDelay;
                    float fade = 1f;

                    if (fadeInTimer > 0f && timeSinceActive < fadeInTimer)
                    {
                        fade = MathHelper.Clamp(timeSinceActive / fadeInTimer, 0f, 1f);
                    }
                    else
                    {
                        if (fadeTimeMax > 0f)
                        {
                            float outAlpha = MathHelper.Clamp(fadeCurrent / fadeTimeMax, 0f, 1f);
                            fade = outAlpha;
                        }
                        else
                        {
                            fade = 1f;
                        }
                    }

                    if (fade <= 0f)
                        continue;
                    var size = font.MeasureString(text.Text); 
                    var origin = size * 0.5f; 

                    sb.DrawString(
                        font,
                        text.Text,
                        text.DrawPoint,
                        ColorPalette.DarkColor * fade,
                        text.Rotation,
                        origin,
                        1f,  
                        SpriteEffects.None,
                        0f
                    );
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
