using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public class QuestSaveData
    {
        public QuestStage stage { get; set; } = QuestStage.NotStarted;
        public Dictionary<string, QuestObjectives> objectives { get; set; } = new();
    }

    public class QuestObjectives
    {
        public int progress { get; set; } = 0;
        public bool completed { get; set; } = false;
        
    }



}

