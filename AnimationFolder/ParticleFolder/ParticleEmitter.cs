using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.ParticleFolder
{
    public class ParticleEmitter
    {
        private readonly ParticleEmitterData _data;
        private float _intervalLeft;
        private readonly IEmitter _emitter;

        public ParticleEmitter(IEmitter emitter, ParticleEmitterData data)
        {
            _emitter = emitter;
            _data = data;
            _intervalLeft = data.interval;
        }
        private void Emit (Vector2 pos)
        {
            ParticleData d = _data.ParticleData;
            Random rng = RandomHut.rng;
            d.lifeSpan = rng.Next((int)_data.lifeSpawnMax, (int)_data.lifeSpanMin);
            d.speed = rng.Next((int)_data.speedMin, (int)_data.speedMax);
            float r = (float)(rng.NextDouble()*2)-1;
            d.angle += _data.angleVariance * r;

            Particle p = new Particle(pos,d);
            ParticleManager.AddParticle(p);
        }
        public void Update(float delta)
        {
            _intervalLeft -= delta;
            while (_intervalLeft <= 0f)
            {
                _intervalLeft += _data.interval;
                var pos = _emitter.EmitPositon;
                for (int i = 0; i < _data.emitCount; i++)
                {
                    Emit(pos);
                }
            }


        }
    }
}
