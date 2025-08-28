using PlayingAround.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.ParticleFolder
{
    public class IEmitter
    {
        public Vector2 EmitPositon {  get; set; }
    }
    public class MouseEmitter : IEmitter
    {
        public Vector2 EmitPosition => new Vector2(InputManager.MouseX, InputManager.MouseY);
    }
}