using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Triggers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
         public float PauseDurationMax { get; set; } = 0;
         public float PauseDurationMin { get; set; }
         public float? MovementQuicknessOverride { get; set; }
        public bool HasCombatTrigger { get; set; } = true;
         public List<Trigger> Triggers { get; set; } = new List<Trigger> { };
    }


}
