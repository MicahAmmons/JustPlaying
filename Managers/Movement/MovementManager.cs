using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Tiles;
using PlayingAround.Managers.NPCHouse;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Movement
{
    public static class MovementManager
    {

        public static Queue<CombatMonster> _combatMonsters => CombatGuard.CurrentCombat._turnOrder;
        public static Player _player => PlayerManager.CurrentPlayer;
        public static List<PlayMonsters> _playerMonsters => TileManager.CurrentMapTile.PlayMonstersManager.CurrentPlayMonsters;
        //public static List<NPC> _currentNPCs => NPCManager.CurrentNPCs;

        private const int PlayMonsterIconWidth = 64;
        private const int PlayMonsterIconHeight = 64;



        public static void Update(GameTime gameTime)
        {
            switch (SceneManager.CurrentState)
            {
                case SceneState.Play:
                    UpdatePlayMonstersPosition(gameTime);
                    UpdatePlayerPosition(gameTime);
                //    UpdateNPCPosition(gameTime);
                    break;

                case SceneState.Combat:
                    UpdateCombatMonsterPosition(gameTime);
                    break;
                case SceneState.Dialogue:
                    UpdatePlayMonstersPosition(gameTime);
                    break;
            }
           
        }

        public static void UpdateCombatMonsterPosition(GameTime gameTime)
        {
            foreach (var mon in  _combatMonsters)
            {
                if (mon.MovePath == null || mon.MovePath.Count <= 0 || !mon.AllowedToMove) continue;
                Vector2 nextPoint = mon.MovePath[0];
                float speed = mon.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

                Vector2 direction = nextPoint - mon.currentPos;
                float distance = direction.Length();

                if (distance <= speed)
                {
                    mon.currentPos = nextPoint;
                    mon.MovePath.RemoveAt(0);
                }
                else
                {
                    direction.Normalize();
                    mon.currentPos += direction * speed;
                }
            }
        }

        public static void UpdatePlayerPosition(GameTime gameTime)
        {
            if (!_player.AllowedToMove || _player == null || _player.MovementPath.Count <= 0 || _player.MovementPath == null) return;


            Vector2 nextPoint = _player.MovementPath[0];
            float speed = _player.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - _player.CurrentPos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                _player.CurrentPos = nextPoint;
                _player.MovementPath.RemoveAt(0);
            }
            else
            {
                direction.Normalize();
                _player.CurrentPos += direction * speed;
            }
        }
     
        public static void UpdatePlayMonstersPosition(GameTime gameTime)
        {
            if (_playerMonsters == null || _playerMonsters.Count == 0) return;
            
            foreach (var mon in _playerMonsters)
            {
                if (mon.MovePath == null || mon.MovePath.Count == 0)
                    return;

                Vector2 nextPoint = mon.MovePath[0];
                float speed = mon.MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                Vector2 direction = nextPoint - mon.CurrentPos;
                float distance = direction.Length();

                if (distance <= speed)
                {
                    mon.CurrentPos = nextPoint;
                    mon.MovePath.RemoveAt(0);
                }
                else
                {
                    direction.Normalize();
                    mon.CurrentPos += direction * speed;
                }
            }
        }
  

        }


}

