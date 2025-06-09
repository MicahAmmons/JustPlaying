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
        public Dictionary<string, ObjectiveProgress> objectives { get; set; } = new();
    }

    public class ObjectiveProgress
    {
        public int progress { get; set; } = 0;
        public bool completed { get; set; } = false;

        // Optional if you want runtime checks:
        public int requiredAmount { get; set; } = 1; // Not needed for saving, but useful during gameplay
    }



}
