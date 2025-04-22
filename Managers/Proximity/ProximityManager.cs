using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Proximity
{
    public class ProximityManager
    {
        private static Vector2 _currentPlayerCord;
        private static TileCell _currentPlayerCell;

        private static List<PlayMonsters> PlayMonsters;
        private static MapTile _currentMapTile;

        private const int _distanceForMonsterInteract = 50;
        public static event Action<PlayMonsters> OnPlayerNearMonster;
        public static event Action OnPlayerLeaveMonster;

        public static Dictionary<CombatMonster, TileCell> CombatMonsterCells;



        public static void Update(GameTime gameTime)
        {
            if (_currentMapTile == null) return;

            if (SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                IsPlayerInMonsterRange();
                IsNPCInRange();
                IsTrapInRange();
            }
            if (SceneManager.CurrentState == SceneManager.SceneState.Combat)
            {

            }
            
        }
        public static bool IsTrapInRange()
        {
            return true;
        }
        public static bool IsNPCInRange()
        {
            return false;
        }
        public static void IsPlayerInMonsterRange()
        {
            bool monsterWasNear = false;
            foreach (var mon in _currentMapTile.PlayMonstersManager.CurrentPlayMonsters)
            {
                if (Vector2.Distance(_currentPlayerCord, mon.CurrentPos) <= _distanceForMonsterInteract)
                {
                    OnPlayerNearMonster?.Invoke(mon);
                    monsterWasNear = true;
                    break; 
                }
            }
            if (!monsterWasNear)
            {
                OnPlayerLeaveMonster?.Invoke();
            }
        }
        public static void OnEnterNewCell(TileCell cell, Vector2 cord)
        {
            _currentPlayerCord = cord;
            _currentPlayerCell = cell;
        }
        public static void UpdateMapTile(MapTile mapTile)
        {
            _currentMapTile = mapTile;
        }









    }
}
