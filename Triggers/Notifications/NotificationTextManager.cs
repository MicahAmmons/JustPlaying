using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.Notifications
{
    public static class NotificationTextManager
    {
        private static List<NotificationTextBox> _notificationBoxes = new List<NotificationTextBox>();

        public static void Draw(SpriteBatch sb)
        {
            if (_notificationBoxes.Count == 0) return;
            foreach(var box in _notificationBoxes)
            {
                Vector2 anchorPoint = box.AnchorPoint.ProximityTrackingPoint;
                
            }
        }
        public static void AddNotificationBoxes(NotificationTextBox box)
        {
            _notificationBoxes.Add(box);
        }
    }
}
