using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Triggers
{
    public class TriggerLibrary
    {
        public static Dictionary<string, Trigger> _triggerData = new Dictionary<string, Trigger>();

        public static void LoadContent()
        {
            _triggerData = JsonLoader.LoadTriggerData();
        }

    }
}
