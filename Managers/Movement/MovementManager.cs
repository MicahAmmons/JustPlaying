using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Manager;
using PlayingAround.Managers;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Movement
{
    public static class MovementManager
    {

        public static List<CombatMonster> _combatMonsters = new List<CombatMonster>();
        public static List<Player> _players = new List<Player> ();
        public static List<PlayMonsters> _playerMonsters => TileManager.CurrentMapTile.PlayMonstersManager.CurrentPlayMonsters;


        private const int PlayMonsterIconWidth = 64;
        private const int PlayMonsterIconHeight = 64;



        public static void Update(GameTime gameTime)
        {
            switch (SceneManager.CurrentState)
            {
                case SceneManager.SceneState.Play:
                    UpdatePlayMonstersPosition(gameTime);

                    break;

                case SceneManager.SceneState.Combat:
                    break;
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

