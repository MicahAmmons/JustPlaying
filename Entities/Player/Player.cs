using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using System.Collections.Generic;

namespace PlayingAround.Entities.Player
{
    public class Player
    {
        public float Speed { get; set; }
        public Texture2D Texture { get; private set; }

        public int PlayerWidth { get; set; } = 64;
        public int PlayerHeight { get; set; } = 64;

        private Vector2? moveTarget = null;
        private Queue<Vector2> movementPath = new();
        public Vector2 FeetCenter;
        private Vector2? debugClickTarget = null;
        public Vector2? GetDebugClickTarget()
        {
            return debugClickTarget;
        }




        public Player(Texture2D idleTexture, Vector2 startPosition, float speed = 200f)
        {
            Texture = idleTexture;
            FeetCenter = GenerateSpawnPoint(startPosition);
            Speed = speed;
        }

        public void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 movement = Vector2.Zero;

            // Movement input
            if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) { movement.X -= 1; movementPath.Clear(); }
            if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) { movement.X += 1; movementPath.Clear(); }
            if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) { movement.Y -= 1; movementPath.Clear(); }
            if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S)) { movement.Y += 1; movementPath.Clear(); }

            // Normalize diagonal movement
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                Vector2 nextPos = FeetCenter + movement * Speed * delta;

                if (CanMoveTo(nextPos))
                {
                    FeetCenter = nextPos;
                    moveTarget = null; // Cancel click-move if arrow keys are used
                }
            }

            // Follow pixel path (no walkable re-check)
            if (movementPath.Count > 0)
            {
                Vector2 target = movementPath.Peek();
                Vector2 direction = target - FeetCenter;
                float distance = direction.Length();

                if (distance <= Speed * delta)
                {
                    FeetCenter = target;
                    movementPath.Dequeue();
                }
                else
                {
                    direction.Normalize();
                    FeetCenter += direction * Speed * delta;
                }
            }
        }
        


        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destination = new Rectangle(
                (int)(FeetCenter.X),
                (int)FeetCenter.Y,
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
                (int)(FeetCenter.X + (PlayerWidth / 2f) - (hitboxWidth / 2f)),
                (int)(FeetCenter.Y + PlayerHeight - hitboxHeight),
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


        public void SetPath(List<Vector2> path)
        {
            movementPath.Clear();
            debugClickTarget = path.Count > 0 ? path[^1] : null;

            //Vector2 offset = GetFeetOffsetWithinTile();

            foreach (var point in path)
            {
               
                movementPath.Enqueue(point); // Already pixel-perfect!
            }
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
        public void DrawDebugPath(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            foreach (var point in movementPath)
            {
                Rectangle cellRect = new Rectangle(
                    (int)(point.X ),
                    (int)(point.Y ),
                    MapTile.TileWidth,
                    MapTile.TileHeight
                );

                // Draw a semi-transparent yellow box over the path
                spriteBatch.Draw(debugPixel, cellRect, Color.Yellow * 0.4f);
            }
        }
        public Vector2 GetFeetCenter()
        {
            return new Vector2(FeetCenter.X + PlayerWidth / 2f, FeetCenter.Y + PlayerHeight);
        }






    }
}
