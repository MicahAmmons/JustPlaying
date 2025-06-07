using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Quests
{
    public class QuestData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<QuestObjective> objectives { get; set; }
        public QuestRewards rewards { get; set; }
        public bool autoStart { get; set; }
        public bool autoComplete { get; set; }
    }

    public class QuestObjective
    {
        public string id { get; set; }
        public string description { get; set; }
        public string type { get; set; } // e.g., "kill", "collect", "talk"
        public string targetId { get; set; }
        public int count { get; set; }
    }

    public class QuestRewards
    {
        public int experience { get; set; }
        public List<string> items { get; set; } = new();
        public List<string> customTriggers { get; set; } = new();
    }

}
