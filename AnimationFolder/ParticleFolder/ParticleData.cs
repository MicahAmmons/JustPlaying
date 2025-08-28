using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.ParticleFolder
{
    public class ParticleData
    {
        public Texture2D texture;
        public float lifeSpan;
        public Color colorStart;
        public Color colorEnd;
        public float opacityStart = 1f;
        public float opacityEnd = 0f;
        public float sizeStart = 32f;
        public float sizeEnd = 4f;
        public float speed = 100f;
        public float angle = 0f;
        
        public ParticleData()
        {
            texture = AssetManager.GetTexture("ParticleDefault");
            lifeSpan = 3f;
            colorStart = ColorPalette.DarkColor;
            colorEnd = ColorPalette.LightColor; 

        
        }
    }
}
