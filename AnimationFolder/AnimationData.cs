using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public class AnimationData
    {
        public int Row { get; set; }                 // 1-based
        public int FrameCount { get; set; }
        public int FrameDurationMs { get; set; }     // from JSON
        public bool IsLooping { get; set; }
        public string SpriteSheetName { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public float EndOfCyclePause { get; set; }

    }
}
