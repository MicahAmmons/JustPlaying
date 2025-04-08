using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsters
    {
        // A list of combat monsters that this play monster is associated with
        public List<CombatMonster> Monsters { get; set; }

        // Properties populated from PlayMonsterData
        public float MovementSpeed { get; set; }
        public Rectangle PacingBoundary { get; set; }
        public string MovementPattern { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public float Difficulty { get; set; }
        public string Name { get; set; }

        // Constructor that takes a PlayMonsterData object for easy deserialization
        public PlayMonsters(PlayMonsterData data)
        {
            Name = data.Name;
            Difficulty = data.Difficulty;
            MovementSpeed = data.MovementSpeed;
            PacingBoundary = data.PacingBoundary;
            MovementPattern = data.MovementPattern;

            // Initialize the Monsters list (for example, if we have a way to populate this with CombatMonsters)
            Monsters = new List<CombatMonster>();

            // You might add logic here to populate the Monsters list based on the PlayMonsterData
            // E.g., you could check the MonsterType and Difficulty to add specific types of CombatMonsters.
        }
    }
}
