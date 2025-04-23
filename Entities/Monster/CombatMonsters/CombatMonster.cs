using Microsoft.Xna.Framework;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Stats;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster
    {
        public float Difficulty { get; set; }
        public string IconPath { get; set; }
        public string Name { get; set; }

        public Vector2 currentPos;

        public Vector2 startingPos;
        public int Speed { get; set; }
        public float MovementQuickness { get; set; }
        public bool isPlayerControled { get; set; }
        public bool Draw { get; set; } = true;
        public string TurnBehavior { get; set; }
        public string MovementPattern { get; set; }
        public ListOfAttacks Attacks { get; set; }
        public List<string> Immunities { get; set; }
        public List<string> Resistances { get; set; }
        public List<string> Vulnerabilities { get; set; }
        public List<Vector2> MovePath { get; set; } = new();
        public bool PathGenerated { get; set; } = false;
        public int ID { get; set; }
        public TileCell CurrentCell { get; set; }
        public int AttackPower {  get; set; }
        public int TurnNumber { get; set; } = 0;
        public Queue<string> OrderOfActions { get; set; }
        public string ChooseAttackBehavior { get; set; } // add number of cells moved
        public float MaxHealth { get; set; }
        public float MaxMana { get; set; }
        public float CurrentMana { get; set; }
        public float CurrentHealth { get; set; }    
        public TileCell PlayerMovementEndPoint { get; set; }
        public float Initiation { get; set; } = 5;




        public CombatMonster() 
        {


        }
        public CombatMonster(Player.Player player)
        {
            PlayerStats stats = player.stats;
            Speed = stats.MovementSpeed;
            MaxHealth = stats.MaxHealth;
            CurrentHealth = stats.CurrentHealth;
            Name = player.Name;
            MaxMana = stats.MaxMana;
            CurrentMana = stats.CurrentMana;
            isPlayerControled = true;
            Draw = false;
            AttackPower = 1;
            Initiation = stats.Initiation;
            MovementQuickness = 200f;
            MovementPattern = "straight";
        }
    }
}
