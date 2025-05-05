    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using PlayingAround.Entities.Monster.CombatMonsters;
    using PlayingAround.Game.Map;
    using PlayingAround.Manager;
    using PlayingAround.Managers.Assets;
    using PlayingAround.Managers.CombatMan.CombatAttacks;
    using PlayingAround.Managers.CombatMan.CombatBehaviors;
    using System;

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
            public Vector2 EndingPosition;
            public float MovementSpeed;
            public VisualTiming WhenToStart { get; set; }
            public bool ShouldFade = false;
        public int Width;
            public int Height; 



        public VisualEffect(Vector2 startPos, Vector2 velocity, float duration, Color color = default, string text = null, Texture2D texture = null)
            {
                Position = startPos;
                Velocity = velocity;
                Duration = duration;
                Text = text;
                Texture = texture;
                Color = color == default ? Color.White : color;
            ShouldFade = true;
            }
            public VisualEffect(TileCell cell, SingleAttack att, TileCell centerCell,  Color color = default)
            {
                WhenToStart = att.VisualTiming;
            Vector2 vec = TileManager.GetCellCords(cell);
                Position =  new Vector2(vec.X + MapTile.TileWidth/4, vec.Y + MapTile.TileHeight/4);
            Vector2 vecEnd = TileManager.GetCellCords(centerCell);
            EndingPosition = new Vector2(vecEnd.X + MapTile.TileWidth / 4, vecEnd.Y + MapTile.TileHeight/4);
                MovementSpeed = att.VisualVelocity;
                Texture = AssetManager.GetTexture($"{att.Name.Replace(" ", "")}Icon");

                Color = color == default ? Color.White : color;
                Vector2 direction = EndingPosition - Position;
                if (direction != Vector2.Zero)
                    direction.Normalize();
                Velocity = direction * MovementSpeed;
                float distance = Vector2.Distance(Position, EndingPosition);
                Duration = distance / MovementSpeed;
            if (att.Animated)
            {
                Width = 32;
                Height = 32;
            }

        }


        public bool IsExpired => Elapsed >= Duration;

            public bool IsFinished => Elapsed >= Duration;

            public void Update(float deltaTime)
            {
                Elapsed += deltaTime;
                Position += Velocity * deltaTime;
                float alpha = 1f - (Elapsed / Duration);
            if (ShouldFade)
            {
                Color = new Color(Color.R, Color.G, Color.B) * alpha;
            }
            }

            public void Draw(SpriteBatch spriteBatch, SpriteFont font)
            {
            if (Texture != null)
            {
                Rectangle destination = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
                spriteBatch.Draw(Texture, destination, Color);
            }

            else if (!string.IsNullOrEmpty(Text))
                {
                    spriteBatch.DrawString(font, Text, Position, Color);
                }
            }
        }
        public enum VisualTiming
        {
            BeforeAttack,
            DuringAttack,
            AfterAttack,
            IsRunning,
            Complete
        }

    }
