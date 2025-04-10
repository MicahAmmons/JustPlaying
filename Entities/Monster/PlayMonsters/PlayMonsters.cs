using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsters
    {
        // A list of combat monsters that this play monster is associated with
        public List<CombatMonster> Monsters { get; set; }
        public Vector2 SpawnPosition { get; set; }

        // Properties populated from PlayMonsterData
        public float MovementSpeed { get; set; }
        public Rectangle PacingBoundary { get; set; }
        public string MovementPattern { get; set; }
        public string IconPath { get; set; }
        public Texture2D Icon { get; set; }
        public string Name { get; set; }
        public Vector2 CurrentPos { get; set; }
        public List<Vector2> MovePath { get; set; }
        public bool IsPaused { get; set; } = false;
        public float PauseTimer { get; set; } = 0f; // in seconds
        public float PauseDuration { get; set; } = 1.5f; // how long to pause after a path ends



        // Constructor that takes a PlayMonsterData object for easy deserialization
        public PlayMonsters()
        {

        }
    }
}
