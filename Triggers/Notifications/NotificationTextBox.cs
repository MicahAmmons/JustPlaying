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
            SpriteFont font = AssetManager.GetFont("mainFont");
            Vector2 size = font.MeasureString(fullText);

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
}
