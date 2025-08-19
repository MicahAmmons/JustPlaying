using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public class AnimationContainer
    {
        public Dictionary<AnimationState, List<Animation>> AnimationBucket {  get; set; } = new Dictionary<AnimationState, List<Animation>>();

        public AnimationContainer(AnimationData data)
        {
            foreach (var item in data.Animations)
            {
                AnimationState animationState = item.Key;
                List<SpecificAnimationData> animations = item.Value;
                List<Animation> ani = new List<Animation>();
                foreach (var anim in animations)
                {
                    ani.Add(new Animation(anim));
                }
                AnimationBucket[animationState] = ani;
            }
        }
        //safe clone constructor
        public AnimationContainer(AnimationContainer data)
        {
            foreach (var kvp in data.AnimationBucket)
            {
                AnimationState animationState = kvp.Key;
                List<Animation> animations = kvp.Value;

                List<Animation> newAnimList = new List<Animation>();
                foreach (var ani in animations)
                {
                    Animation anim = new Animation(ani);
                    newAnimList.Add(anim);
                }
                AnimationBucket[animationState] = newAnimList;
            }

        }
    }
}
