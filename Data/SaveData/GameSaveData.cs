using PlayingAround.Data.MapTile;
using PlayingAround.Game.Map;
using System.Collections.Generic;

namespace PlayingAround.Data.SaveData
{
    public class GameSaveData
    {
        public MapTileSaveData MapTile { get; set; }
        public PlayerSaveData Player { get; set; }
        public DayCycleSaveData DayCycle { get; set; }
        public Dictionary<string, SummonedSavedStats> SummonedData { get; set; }
        public Dictionary<string, QuestSaveData> Quests { get; set; } = new();
        public Dictionary<string, SettingSaveData> Settings { get; set; } = new();




    }
}