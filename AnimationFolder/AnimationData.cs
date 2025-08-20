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
        public Dictionary<AnimationState, List<SpecificAnimationData>> Animations { get; set; }

    }
    public class SpecificAnimationData
    {
        public int Row { get; set; }                 // 1-based
        public int FrameCount { get; set; }
        public int FrameDurationMs { get; set; }     // from JSON
        public bool IsLooping { get; set; }
        public string SpriteSheetName { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public bool FadeEffect { get; set; } = false;
        public bool SmokeEffect {  get; set; } = false;
        public Direction DefaultDirection { get; set; }
        public AttackName? AttackName { get; set; } = null;
    }
}
