using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Pathfinding
{
    public static class MovementInstructions
    {
        public struct MovementInstruction
        {
            public Vector2 Direction;
            public float Distance;

            public MovementInstruction(Vector2 direction, float distance)
            {
                Direction = direction;
                Distance = distance;
            }
        }

    }
}
