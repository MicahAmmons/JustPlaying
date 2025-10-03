using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
         public float PauseDurationMax { get; set; } = 0;
         public float PauseDurationMin { get; set; }
        public float? MovementQuicknessOverride { get; set; }

    }


}
