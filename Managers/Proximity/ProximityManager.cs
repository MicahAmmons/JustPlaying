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

        private const int _distanceForInteract = 64;
        private const int _distanceForNextTileInteract = 48;

        private static readonly HashSet<PlayMonsters> _playMonstersCurrentlyInRange = new();


        public static event Action<PlayMonsters> OnPlayerNearPlayMonster;
        public static event Action<PlayMonsters> OnPlayerLeavePlayMonster;

        public static event Action<NPC> OnPlayerNearNPC;
        public static event Action<NPC> OnPlayerLeaveNPC;

        public static event Action<NextTileData> OnPlayerNearNextTile;
        public static event Action<NextTileData> OnPlayerLeaveNextTile;


        public static void Update(GameTime gameTime)
        {

            if (!SceneManager.IsState(SceneState.Play)) return;
                UpdatePlayerCords();
                IsPlayerInPlayMonsterRange();
                IsPlayerInNPCRange();
                IsPlayerInNextTileRange();
        }
        private static readonly HashSet<NPC> _npcsCurrentlyInRange = new();
        public static void IsPlayerInNPCRange()
        {
            float thresholdSq = _distanceForInteract * _distanceForInteract;

            foreach (var npc in _currentNPCs)
            {
                Vector2 delta = _playerCurrentCords - npc.currentPos;
                bool inRange = delta.LengthSquared() <= thresholdSq;

                bool tracked = _npcsCurrentlyInRange.Contains(npc);

                if (inRange && !tracked)
                {
                    _npcsCurrentlyInRange.Add(npc);
                    OnPlayerNearNPC?.Invoke(npc);
                }
                else if (!inRange && tracked)
                {
                    _npcsCurrentlyInRange.Remove(npc);
                    OnPlayerLeaveNPC?.Invoke(npc);
                }
            }
        }
        public static void IsPlayerInPlayMonsterRange()
        {
            float thresholdSq = _distanceForInteract * _distanceForInteract;

            foreach (var mon in _currentPlayMonsters)
            {
                Vector2 delta = _playerCurrentCords - mon.MovementController.CurrentPos;
                bool inRange = delta.LengthSquared() <= thresholdSq;
                bool tracked = _playMonstersCurrentlyInRange.Contains(mon);

                if (inRange && !tracked)
                {
                    _playMonstersCurrentlyInRange.Add(mon);   
                    OnPlayerNearPlayMonster?.Invoke(mon);  
                }
                else if (!inRange && tracked)
                {
                    _playMonstersCurrentlyInRange.Remove(mon);
                    OnPlayerLeavePlayMonster?.Invoke(mon);   
                }
            }
        }

        private static readonly HashSet<NextTileData> _nextTilesCurrentlyInRange = new();

        public static void IsPlayerInNextTileRange()
        {
            float thresholdSq = _distanceForNextTileInteract * _distanceForNextTileInteract;

            foreach (var (center, nextTileData) in _currentNextTiles)
            {
                Vector2 delta = _playerCurrentCords - center;
                bool inRange = delta.LengthSquared() <= thresholdSq;

                bool tracked = _nextTilesCurrentlyInRange.Contains(nextTileData);

                if (inRange && !tracked)
                {
                    _nextTilesCurrentlyInRange.Add(nextTileData);
                    OnPlayerNearNextTile?.Invoke(nextTileData);
                }
                else if (!inRange && tracked)
                {
                    _nextTilesCurrentlyInRange.Remove(nextTileData);
                    OnPlayerLeaveNextTile?.Invoke(nextTileData);
                }
            }
        }
        public static void ClearCurrentRange()
        {
            _nextTilesCurrentlyInRange?.Clear();
            _npcsCurrentlyInRange?.Clear();
            _playMonstersCurrentlyInRange?.Clear();
        }

        private static void UpdatePlayerCords()
        {
            _playerCurrentCords = _currentPlayer.MovementController.CurrentPos;
        }










    }
}
