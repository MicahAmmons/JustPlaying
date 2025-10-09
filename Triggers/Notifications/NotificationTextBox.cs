using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;

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
        public bool Active { get; set; } = false;
        public float FadeTimerMax { get; set; }
        public float FadeTimerCurrent {  get; set; }
        public Texture2D BackgroundTexture { get; set; }
        public void MarkInactive()
        {
            Active = false;
            FadeTimerCurrent = 0;
        }
        public void MarkActive()
        {
          
            FadeTimerCurrent = FadeTimerMax;
            Active = true;
        }
        public abstract Vector2 GetTypeSpecificDrawPoints();
        public virtual Texture2D GetTexture()
        {
            return BackgroundTexture;
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
            FadeTimerMax = 3f;
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
            Vector2 anchorPoint = AnchorPoint.ProximityTrackingPoint;
            return anchorPoint + new Vector2(0, 32f);
        }

    }
}
