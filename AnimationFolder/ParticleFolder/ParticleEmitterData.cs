using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.ParticleFolder
{
    public class ParticleEmitterData
    {
        public ParticleData ParticleData = new();

        public float angle = 0f;
        public float angleVariance = 45f;
        public float lifeSpanMin = 0.1f;
        public float lifeSpawnMax = 2f;
        public float speedMin = 10f;
        public float speedMax = 100f;
        public float interval = 1;
        public int emitCount = 1;

        public ParticleEmitterData()
        {

        }


    }
}
