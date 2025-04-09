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
        public float TimeSinceLastJump { get; set; } // Time since the last jump
        public float JumpCooldown { get; set; } // Time to wait before the next jump (seconds)
        public string Name { get; set; }
        public Vector2 LandingPoint { get; set; } // The target landing position
        public Vector2 JumpStartPosition { get; set; }

        // Constructor that takes a PlayMonsterData object for easy deserialization
        public PlayMonsters()
        {
            TimeSinceLastJump = 0f;
            JumpCooldown = 2f;
        }
    }
}
