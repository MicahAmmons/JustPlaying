using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Quests
{
    public static  class QuestLibrary
    {

        private static Dictionary<string, QuestData> _questsData;

        public static void LoadContent()
        {
            _questsData = JsonLoader.LoadQuests();
        }



    }
}
