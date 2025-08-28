using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System; 


namespace PlayingAround.AnimationFolder.ParticleFolder
{
    public class Particle
    {
        private readonly ParticleData _data;

        private Vector2 _position;
        private float _lifeSpanLeft;
        private float _lifeSpanAmount;
        private Color _color;
        private float _opacity;
        public bool isFinished = false;
        private float _scale;
        private Vector2 _origin;
        private Vector2 _direction;

        public Particle(Vector2 pos, ParticleData data)
        {
            _data = data;
            _lifeSpanLeft = data.lifeSpan;
            _lifeSpanAmount = data.lifeSpan;
            _position = pos;
            _color = data.colorStart;
            _opacity = data.opacityStart;
            _origin = new(_data.texture.Width/2, _data.texture.Height/2);

            if (data.speed != 0)
            {
                float rad = MathHelper.ToRadians(_data.angle);
                _direction = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
            }
            else
            {
                {
                    _direction = Vector2.Zero;
                }
            }
        }
        public void Update(float delta)
        {
            _lifeSpanLeft -= delta;
            if (_lifeSpanAmount <= 0f )
            {
                isFinished = true;
                return;
            }
            _lifeSpanAmount = MathHelper.Clamp(_lifeSpanLeft / _data.lifeSpan, 0, 1f);
            _color = Color.Lerp(_data.colorEnd, _data.colorStart, _lifeSpanAmount);
            _opacity = MathHelper.Clamp(MathHelper.Lerp(_data.opacityEnd, _data.opacityStart, _lifeSpanAmount), 0, 1);
            _scale = MathHelper.Lerp(_data.sizeEnd, _data.sizeStart, +_lifeSpanAmount) / _data.texture.Width;
            _position -= _direction * _data.speed * delta;
        }
        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_data.texture, _position, null, _color * _opacity, 0f, _origin, _scale, SpriteEffects.None, 1f);
        }
    }
}
