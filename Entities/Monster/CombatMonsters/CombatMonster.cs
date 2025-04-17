using Microsoft.Xna.Framework;
using PlayingAround.Stats;

namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster
    {
        public float Difficulty { get; set; }
        public string IconPath { get; set; }
        public string Name { get; set; }
        public Vector2 currentPos;
        public Vector2 startingPos;

        public float Speed { get; set; }
        public float Health { get; set; }
        public float Mana {  get; set; }
        public bool isPlayerControled { get; set; }
        public bool Draw { get; set; } = true;


        public CombatMonster() 
        {


        }
        public CombatMonster(Player.Player player)
        {
            PlayerStats stats = player.stats;
            Speed = stats.speed;
            Health = stats.health;
            Name = player.Name;
            Mana = stats.mana;
            isPlayerControled = true;
            Draw = false;
        }
    }
}
