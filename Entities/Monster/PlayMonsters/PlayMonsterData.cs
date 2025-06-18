using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
         public float MovementQuickness { get; set; }
         public MovementPatternType MovementPattern { get; set; }
         public float PauseDurationMax { get; set; } = 0;
         public float PauseDurationMin { get; set; }

    }


}
