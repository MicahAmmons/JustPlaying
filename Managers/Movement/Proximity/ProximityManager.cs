using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Proximity
{
    public class ProximityManager
    {
        private static Player _currentPlayer => PlayerManager.CurrentPlayer;
        private static List<NPC> _currentNPCs => TileManager.CurrentMapTile.NPCs;
        private static List<PlayMonsters> _currentPlayMonsters => TileManager.CurrentMapTile.PlayMonstersList;

        private const int _distanceForMonsterInteract = 50;

        public static event Action<PlayMonsters> OnPlayerNearPlayMonster;
        public static event Action OnPlayerLeavePlayMonster;

        public static event Action<NPC> OnPlayerNearNPC;
        public static event Action OnPlayerLeaveNPC;

        private static Vector2 _playerCurrentCords;




        public static void Update(GameTime gameTime)
        {

            if (SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                UpdatePlayerCords();
                IsPlayerInMonsterRange();
            }
            if (SceneManager.CurrentState == SceneManager.SceneState.Combat)
            {

            }
            
        }
        private static void UpdatePlayerCords()
        {
            _playerCurrentCords = _currentPlayer.CurrentPos;
        }
        public static void IsPlayerInMonsterRange()
        {
            bool monsterWasNear = false;
            foreach (var mon in _currentPlayMonsters)
            {
                if (Vector2.Distance(_playerCurrentCords, mon.CurrentPos) <= _distanceForMonsterInteract)
                {
                    OnPlayerNearPlayMonster?.Invoke(mon);
                    monsterWasNear = true;
                    break; 
                }
            }
            if (!monsterWasNear)
            {
                OnPlayerLeavePlayMonster?.Invoke();
            }
        }









    }
}
