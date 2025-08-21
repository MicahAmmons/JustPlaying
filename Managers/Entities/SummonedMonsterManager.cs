using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Tiles;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Entities
{
    public static class SummonedMonsterManager
    {
        private static Dictionary<string, SummonedSavedStats> _summonedSaveData;
        public static Dictionary<string, SummonedSavedStats> UnlockedSummons { get; set; } = new Dictionary<string, SummonedSavedStats>();
        public static Dictionary<string, SummonedSavedStats> LockedSummons { get; set; } = new Dictionary<string, SummonedSavedStats> ();

        public static void LoadContent(Dictionary<string,SummonedSavedStats> data)
        {
            _summonedSaveData = data;
            foreach (var kvp in _summonedSaveData)
            {
                string monName = kvp.Key;
                SummonedSavedStats stats = kvp.Value;
                stats.Icon = AssetManager.GetTexture($"{monName}Icon");
                if (stats.TotalNumberOfKills > 0)
                {
                    UnlockedSummons[monName] = stats;
                }
                else if (stats.TotalNumberOfKills <= 0)
                {
                    LockedSummons[monName] = stats;
                }
            }
        }
        public static Dictionary<string, SummonedSavedStats> Save()
        {
            return _summonedSaveData;
        }
    }
}
