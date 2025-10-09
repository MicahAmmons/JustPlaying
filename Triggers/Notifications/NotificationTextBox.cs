using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.Notifications
{
    public abstract class NotificationTextBox
    {
        public Rectangle Rect {  get; set; }
        public Keys Key { get; set; }
        public string BeforeKeyText {  get; set; }
        public string AfterKeyText { get; set; } 
        public IProximityTracked AnchorPoint {  get; set; }
        public Vector2 CacheAnchorPoint { get; set; }
        public bool Active { get; set; } = false;
        public float FadeTimerMax { get; set; }
        public float FadeTimerCurrent {  get; set; }
        public Texture2D BackgroundTexture { get; set; }
        public SpriteFont Font { get; set; }
        public const int Padding = 10;
        public const float MaxTextWidth = 100f;

        public string WrappedText { get; set; }

        public void MarkInactive()
        {
            Active = false;
        }
        public void MarkActive()
        {
            ResetCurrentFadeTimer();
            Active = true;
        }
        public void ResetCurrentFadeTimer()
        {
            FadeTimerCurrent = FadeTimerMax;
        }
        public abstract Vector2 GetTypeSpecificDrawPoints();
        public virtual Texture2D GetTexture()
        {
            return BackgroundTexture;
        }
        public virtual void SetCacheAnchorPoint()
        {
            CacheAnchorPoint = AnchorPoint.ProximityTrackingPoint;
        }

        public virtual float GetRotation()
        {
            return 0f;
        }
        //If the anchorpoint moves (like a playmonster) it doesn't redraw the prompt in new area
        internal virtual bool AnchorPointMoved(Vector2 anchorPoint)
        {
            return CacheAnchorPoint == anchorPoint;
        }
    }
    public class CombatNotificationTextBox : NotificationTextBox
    {
        public CombatNotificationTextBox(IProximityTracked proxy)
        {
            AnchorPoint = proxy;
            BeforeKeyText = "Press";
            AfterKeyText = "to begin Combat";
            Key = Keys.E;
            FadeTimerMax = 0.5f;
            BackgroundTexture = AssetManager.GetTexture("CombatNotificationBG");
            string fullText = $"{BeforeKeyText} {Key.ToString()} {AfterKeyText}";
            Font = AssetManager.GetFont("mainFont");
            Vector2 size = Font.MeasureString(fullText);

            Rect = new Rectangle(
               0,
               0,
               (int)size.X + 10 * 2,
               (int)size.Y + 10 * 2
           );
        }
        public override Vector2 GetTypeSpecificDrawPoints()
        {
            Vector2 anchorPoint = CacheAnchorPoint;
            return anchorPoint + new Vector2(0, 32f);
        }

    }
    public class MessageNotificationTextBox : NotificationTextBox
        
    {
        public MessageNotificationTextBox(IProximityTracked proxy, NotificationTextBoxData data)
        {
            AnchorPoint = proxy;
            BeforeKeyText = data?.BeforeKeyText ?? string.Empty;
            AfterKeyText = data?.AfterKeyText ?? string.Empty;

            // Parse optional key
            Keys? parsedKey = null;
            if (!string.IsNullOrWhiteSpace(data?.Key) && Enum.TryParse(data.Key, true, out Keys k))
                parsedKey = k;

            FadeTimerMax = 2f;
            BackgroundTexture = AssetManager.GetTexture("DarkColorBG");
            Font = AssetManager.GetFont("NotificationBoxFont");

            // Build display text (skip the middle token if no key)
            string middle = parsedKey.HasValue ? parsedKey.Value.ToString() : string.Empty;
            string fullText = $"{BeforeKeyText} {middle} {AfterKeyText}".Trim();

            // Wrap to max width and compute size
            (WrappedText, Vector2 textSize) = WrapText(Font, fullText, MaxTextWidth);

            // Size the background rect to the text + padding
            Rect = new Rectangle(
                0, 0,
                (int)MathF.Ceiling(textSize.X) + Padding * 2,
                (int)MathF.Ceiling(textSize.Y) + Padding * 2
            );
        }

        private static (string wrapped, Vector2 size) WrapText(SpriteFont font, string text, float maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (string.Empty, Vector2.Zero);

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = font.MeasureString(" ").X;
            float maxLineWidth = 0f;
            int lineCount = 1;

            foreach (var word in words)
            {
                float wordWidth = font.MeasureString(word).X;

                bool needsSpace = lineWidth > 0f;
                float projected = lineWidth + (needsSpace ? spaceWidth : 0f) + wordWidth;

                if (projected > maxWidth && lineWidth > 0f)
                {
                    // new line
                    sb.Append('\n');
                    maxLineWidth = MathF.Max(maxLineWidth, lineWidth);
                    lineWidth = 0f;
                    lineCount++;
                    needsSpace = false;
                    projected = wordWidth;
                }

                if (needsSpace) { sb.Append(' '); lineWidth += spaceWidth; }
                sb.Append(word);
                lineWidth += wordWidth;
            }

            maxLineWidth = MathF.Max(maxLineWidth, lineWidth);
            float height = lineCount * font.LineSpacing;

            return (sb.ToString(), new Vector2(maxLineWidth, height));
        }

        public override Vector2 GetTypeSpecificDrawPoints()
        {
            Vector2 anchorPoint = CacheAnchorPoint;
            return anchorPoint + new Vector2(-128f, -64f);
        }
        public override float GetRotation()
        {
            float degrees = 345f;
            float rotation = MathHelper.ToRadians(degrees);
            return rotation;
        }
    }
}
