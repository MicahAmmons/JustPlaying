using System.Collections.Generic;

namespace PlayingAround.Data
{
    public class GameSaveData
    {
        public string CurrentTileId { get; set; }
        public PlayerSaveData Player { get; set; }
        //public Dictionary<string, NPCSaveData> NPCs { get; set; } = new();
        //public Dictionary<string, QuestSaveData> Quests { get; set; } = new();
        //public InventorySaveData Inventory { get; set; } = new();
        //public PlayerStats Stats { get; set; } = new();


    }
}