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

    }
    public class CombatNotificationTextBox : NotificationTextBox
    {
        public CombatNotificationTextBox(IProximityTracked proxy)
        {
            AnchorPoint = proxy;
            BeforeKeyText = "Press";
            AfterKeyText = "to begin Combat";
            Key = Keys.E;
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
    }
}
