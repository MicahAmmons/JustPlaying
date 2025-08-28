using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder.ParticleFolder
{
     public static class ParticleManager
    {
        private static readonly List<Particle> _particles = new();
        private static readonly List<ParticleEmitter> _emitters = new();

        public static void AddParticle(Particle P)
        {
            _particles.Add(P);
        }
        public static void AddParticleEmitter(ParticleEmitter e)
        {
            _emitters.Add(e);
        }
        public static void UpdateParticles(float delta)
        {
            foreach (var p in _particles)
            {
                p.Update(delta);
            }
        }
        public static void UpdatEmitter(float delta)
        {
            foreach (var  e in _emitters)
            {
                e.Update(delta);
            }
        }
        public static void Update(GameTime gameTime)
        {
            if (InputManager.IsLeftClick())
            {
                AddParticle(new Particle(new Vector2(InputManager.MouseX, InputManager.MouseY), new()));
            }
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateParticles(delta);
            UpdatEmitter(delta);
        }

        public static void Draw(SpriteBatch sb)
        {
            foreach (var p in _particles)
            {
                p.Draw(sb);
            }
        }

    }
}
