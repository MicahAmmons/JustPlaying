using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlayingAround.Visuals
{
    public class VisualEffect
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Duration;
        public float Elapsed;
        public string Text;
        public Texture2D Texture;
        public Color Color;

        public VisualEffect(Vector2 startPos, Vector2 velocity, float duration, Color color = default, string text = null, Texture2D texture = null)
        {
            Position = startPos;
            Velocity = velocity;
            Duration = duration;
            Text = text;
            Texture = texture;
            Color = color == default ? Color.White : color;
        }

        public bool IsExpired => Elapsed >= Duration;

        public bool IsFinished => Elapsed >= Duration;

        public void Update(float deltaTime)
        {
            Elapsed += deltaTime;
            Position += Velocity * deltaTime;
            float alpha = 1f - (Elapsed / Duration);
            Color = new Color(Color.R, Color.G, Color.B) * alpha;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Position, Color);
            }
            else if (!string.IsNullOrEmpty(Text))
            {
                spriteBatch.DrawString(font, Text, Position, Color);
            }
        }
    }
}
