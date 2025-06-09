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
        private static Vector2 _playerCurrentCords;

        private const int _distanceForInteract = 50;


        public static event Action<PlayMonsters> OnPlayerNearPlayMonster;
        public static event Action OnPlayerLeavePlayMonster;

        public static event Action<NPC> OnPlayerNearNPC;
        public static event Action OnPlayerLeaveNPC;






        public static void Update(GameTime gameTime)
        {

            if (SceneManager.IsState(SceneState.Play))
            {
                UpdatePlayerCords();
                IsPlayerInPlayMonsterRange();
                IsPlayerInNPCRange();
            }            
        }
        public static void IsPlayerInNPCRange()
        {
            bool npcWasNear = false;
            foreach (var npc in _currentNPCs)
            {
                if (Vector2.Distance(_playerCurrentCords, npc.currentPos) <= _distanceForInteract)
                {
                    OnPlayerNearNPC?.Invoke(npc);
                    npcWasNear = true;
                    break;
                }
            }
            if (!npcWasNear)
            {
                OnPlayerLeaveNPC?.Invoke();
            }
        }

        public static void IsPlayerInPlayMonsterRange()
        {
            bool monsterWasNear = false;
            foreach (var mon in _currentPlayMonsters)
            {
                if (Vector2.Distance(_playerCurrentCords, mon.CurrentPos) <= _distanceForInteract)
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
        private static void UpdatePlayerCords()
        {
            _playerCurrentCords = _currentPlayer.CurrentPos;
        }










    }
}
