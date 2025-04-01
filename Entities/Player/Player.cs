using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Game.Map;
using PlayingAround.Manager;

namespace PlayingAround.Entities.Player
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        public float Speed { get; set; }
        public Texture2D Texture { get; private set; }

        public int PlayerWidth { get; set; } = 64;
        public int PlayerHeight { get; set; } = 64;

        private Vector2? moveTarget = null;

        public Player(Texture2D idleTexture, Vector2 startPosition, float speed = 200f)
        {
            Texture = idleTexture;
            Position = GenerateSpawnPoint(startPosition);
            Speed = speed;
        }

        public void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 movement = Vector2.Zero;

            // Movement input
            if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
                movement.X -= 1;
            if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
                movement.X += 1;
            if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W))
                movement.Y -= 1;
            if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S))
                movement.Y += 1;

            // Normalize diagonal movement
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                Vector2 nextPos = Position + movement * Speed * delta;

                if (CanMoveTo(nextPos))
                {
                    Position = nextPos;
                    moveTarget = null; // Cancel click-move if arrow keys are used
                }
            }

            // Mouse right-click movement
            if (mouse.RightButton == ButtonState.Pressed)
            {
                moveTarget = new Vector2(mouse.X, mouse.Y);
            }

            // Continue moving toward click target
            if (moveTarget.HasValue)
            {
                Vector2 direction = moveTarget.Value - Position;

                if (direction.Length() > 1f)
                {
                    direction.Normalize();
                    Vector2 nextPos = Position + direction * Speed * delta;

                    if (CanMoveTo(nextPos))
                        Position = nextPos;
                    else
                        moveTarget = null; // stop if movement blocked
                }
                else
                {
                    moveTarget = null; // close enough
                }
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destination = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                PlayerWidth,
                PlayerHeight
            );

            spriteBatch.Draw(Texture, destination, Color.White);
        }

        public Rectangle GetFeetHitbox()
        {
            int hitboxWidth = PlayerWidth;
            int hitboxHeight = PlayerHeight / 3;

            return new Rectangle(
                (int)(Position.X + (PlayerWidth / 2f) - (hitboxWidth / 2f)),
                (int)(Position.Y + PlayerHeight - hitboxHeight),
                hitboxWidth,
                hitboxHeight
            );
        }


        public Vector2 GenerateSpawnPoint(Vector2 starting)
        {
            var tile = TileManager.CurrentMapTile;
            if (tile == null)
                return starting; // fallback

            int cellX = (int)(starting.X / MapTile.TileWidth);
            int cellY = (int)(starting.Y / MapTile.TileHeight);

            if (IsCellWalkable(cellX, cellY, tile))
                return CenterOfCell(cellX, cellY);

            // Search outward for walkable cell
            const int maxSearchRadius = 10;

            for (int radius = 1; radius <= maxSearchRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int x = cellX + dx;
                        int y = cellY + dy;

                        if (IsCellWalkable(x, y, tile))
                            return CenterOfCell(x, y);
                    }
                }
            }

            // If nothing found, fallback
            return starting;
        }
        private bool IsCellWalkable(int x, int y, MapTile tile)
        {
            if (x < 0 || y < 0 || x >= MapTile.GridWidth || y >= MapTile.GridHeight)
                return false;

            return tile.TileGrid[x, y].IsWalkable;
        }
        private bool CanMoveTo(Vector2 nextPos)
        {
            var tile = TileManager.CurrentMapTile;
            if (tile == null)
                return false;

            // Predict the new feet hitbox based on where the player would move
            int hitboxWidth = PlayerWidth;
            int hitboxHeight = PlayerHeight / 3;

            Rectangle futureFeetBox = new Rectangle(
                (int)(nextPos.X + (PlayerWidth / 2f) - (hitboxWidth / 2f)),
                (int)(nextPos.Y + PlayerHeight - hitboxHeight),
                hitboxWidth,
                hitboxHeight
            );

            // Check the 4 corners of the feet box
            return IsCornerWalkable(futureFeetBox.Left, futureFeetBox.Top, tile) &&
                   IsCornerWalkable(futureFeetBox.Right - 1, futureFeetBox.Top, tile) &&
                   IsCornerWalkable(futureFeetBox.Left, futureFeetBox.Bottom - 1, tile) &&
                   IsCornerWalkable(futureFeetBox.Right - 1, futureFeetBox.Bottom - 1, tile);
        }



        private Vector2 CenterOfCell(int x, int y)
        {
            return new Vector2(
                x * MapTile.TileWidth + MapTile.TileWidth / 2,
                y * MapTile.TileHeight + MapTile.TileHeight / 2
            );
        }
        private bool IsCornerWalkable(int pixelX, int pixelY, MapTile tile)
        {
            int cellX = pixelX / MapTile.TileWidth;
            int cellY = pixelY / MapTile.TileHeight;

            return IsCellWalkable(cellX, cellY, tile);
        }

    }
}
