using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.CombatMan;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Movement
{
    public static class MovementManager
    {

        public static Queue<CombatMonster> _combatMonsters => CombatManager._turnOrder;
        public static Player _player => PlayerManager.CurrentPlayer;
        public static List<PlayMonsters> _playerMonsters => TileManager.CurrentMapTile.PlayMonstersManager.CurrentPlayMonsters;


        private const int PlayMonsterIconWidth = 64;
        private const int PlayMonsterIconHeight = 64;



        public static void Update(GameTime gameTime)
        {
            switch (SceneManager.CurrentState)
            {
                case SceneManager.SceneState.Play:
                    UpdatePlayMonstersPosition(gameTime);
                    UpdatePlayerPosition(gameTime);
                    break;

                case SceneManager.SceneState.Combat:
                    UpdateCombatMonsterPosition(gameTime);
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

            Vector2 direction = nextPoint - _player.PlayerCord;
            float distance = direction.Length();

            if (distance <= speed)
            {
                _player.PlayerCord = nextPoint;
                _player.MovementPath.RemoveAt(0);
            }
            else
            {
                direction.Normalize();
                _player.PlayerCord += direction * speed;
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

