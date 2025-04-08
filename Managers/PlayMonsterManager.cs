using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers
{
    public class PlayMonsterManager
    {

        public static List<PlayMonsters> GeneratePlayMonsters(MapTileData data)
        {
 
            // Try loading from disk
            string path = $"C:/Users/micah/OneDrive/Desktop/Repos/PlayingAround/Entities/Monster/PlayMonsters/PlayMonsterJson/PlayMonsters.json";
            Dictionary<string, List<PlayMonsterData>> jsonData = JsonLoader.LoadPlayMonsterData(path);

            //deserialize json
            // Find the mosntesr that match the MonsterString (could be 1, could be 20)

            List<PlayMonsters> finalList = new List<PlayMonsters>();
            

            return finalList;
        }
    }
}
