using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Smoke
{
    public class SmokeTexture
    {
        // Info for the smoke special effect - will probably make a list of new obj for 
        //the ability to add multiple at a later date
        public float[] Frequency { get; set; }   // JSON array maps here
        public float[] Speed { get; set; }       // JSON array maps here
        public float DistortAmount { get; set; }
        public float Opacity { get; set; }
        public string SmokeFXName { get; set; }
        [JsonIgnore] public Texture2D SmokeFXTexture { get; set; }



        public List<FadingTextures> FadingBaseTextures { get; set; }

        public List<string> StaticBaseTexturesString { get; set; } 
        [JsonIgnore] public List<Texture2D> StaticBaseTextures = new List<Texture2D>();





        // Convenience to get a Vector2
        [JsonIgnore]
        public Vector2 FrequencyVec => new Vector2(Frequency[0], Frequency[1]);

        [JsonIgnore]
        public Vector2 SpeedVec => new Vector2(Speed[0], Speed[1]);
    }

    public class FadingTextures
    {
        public string TextureName { get; set; }
        public Texture2D Texture;
        public float FadeDuration {  get; set; }
        public float CurrentTimer { get; set; }
        public int FadeDirection { get; set; } = 1;
    }
}

