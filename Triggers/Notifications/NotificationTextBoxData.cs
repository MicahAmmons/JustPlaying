using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.Notifications
{
    public class NotificationTextBoxData
    {
        public string BeforeKeyText { get; set; }
        public string AfterKeyText { get; set; }
        public string Key {  get; set; }
        public NotificationTextBoxType Type { get; set; }

    }
}
public enum NotificationTextBoxType
{
    Combat,
    Message,
}