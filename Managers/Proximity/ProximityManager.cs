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
        private static Dictionary<Vector2, NextTileData> _currentNextTiles => TileManager.CurrentMapTile.NextTileMap;

        private static Vector2 _playerCurrentCords;

        private const int _distanceForInteract = 50;
        private const int _distanceForNextTileInteract = 48;


        public static event Action<PlayMonsters> OnPlayerNearPlayMonster;
        public static event Action OnPlayerLeavePlayMonster;

        public static event Action<NPC> OnPlayerNearNPC;
        public static event Action OnPlayerLeaveNPC;

        public static event Action<Vector2,  NextTileData> OnPlayerNearNextTile;
        public static event Action OnPlayerLeaveNextTile;






        public static void Update(GameTime gameTime)
        {

            if (SceneManager.IsState(SceneState.Play))
            {
                UpdatePlayerCords();
                IsPlayerInPlayMonsterRange();
                IsPlayerInNPCRange();
                IsPlayerInNextTileRange();
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
        public static void IsPlayerInNextTileRange()
        {
            bool nextTileWasNear = false;
            foreach (var (center, nextTileData) in _currentNextTiles)
            {
                if (Vector2.Distance(_playerCurrentCords, center) <= _distanceForNextTileInteract)
                {
                    OnPlayerNearNextTile?.Invoke(center, nextTileData);
                    nextTileWasNear = true;
                    break;
                }
            }
            if (!nextTileWasNear)
            {
                OnPlayerLeaveNextTile?.Invoke();
            }
        }
        private static void UpdatePlayerCords()
        {
            _playerCurrentCords = _currentPlayer.CurrentPos;
        }










    }
}
