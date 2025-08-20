using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.SmokeText
{
    public class SmokeTexture
    {
        public float[] Frequency { get; set; }   // JSON array maps here
        public float[] Speed { get; set; }       // JSON array maps here
        public float DistortAmount { get; set; }
        public float Opacity { get; set; }

        // Convenience to get a Vector2
        [JsonIgnore]
        public Vector2 FrequencyVec => new Vector2(Frequency[0], Frequency[1]);

        [JsonIgnore]
        public Vector2 SpeedVec => new Vector2(Speed[0], Speed[1]);
    }


}

