using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data;
using PlayingAround.Data.Summons;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Proximity;
using PlayingAround.Stats;
using PlayingAround.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace PlayingAround.Entities.Player
{
    public class Player
    {
        public float Speed { get; set; }
        public Texture2D Texture { get; private set; }
        public string Name { get; set; }    
        public PlayerStats stats { get; set; }
        public int PlayerWidth { get; set; } = 64;
        public int PlayerHeight { get; set; } = 64;
        private Vector2? moveTarget = null;
        private Queue<Vector2> movementPath = new();
        public Vector2 PlayerCord;
        private Vector2? debugClickTarget = null;

        public Vector2? GetDebugClickTarget()
        {
            return debugClickTarget;
        }

        private Vector2 movement = Vector2.Zero;
        private float deltaTime; // We’ll set this each frame
        private TileCell PlayerCurrentTileCell;
        private bool allowedToMove = true;




        public static Player LoadFromSave(PlayerSaveData data)
        {
            var texture = AssetManager.GetTexture(data.TextureKey);
            float offsetX = data.FeetCenterX - (data.Width/2);
            float offsetY = data.FeetCenterY - data.Height;
            var player = new Player(texture, new Vector2(offsetX, offsetY),  data.PlayerSummons, data.Speed)
            {
                PlayerWidth = data.Width,
                PlayerHeight = data.Height,

            };

            return player;
        }
        public Player(Texture2D idleTexture, Vector2 startPosition, List<SummonsSaveData> summs, float speed = 200f)
        {
            Texture = idleTexture;
            PlayerCord = startPosition; // Readd logic to move it from a nmon walkable spot
            Speed = speed;
            var summonLoader = JsonLoader.LoadSummonProgressions();
            stats = new PlayerStats()
            {
                LockedSummons = new List<SummonedMonster>(),
                UnlockedSummons = new List<SummonedMonster>(),
            };

            foreach (var summon in summs)
            {
                SummonedMonster mon = new SummonedMonster(summon, summonLoader[summon.Name]);
                if (mon.TotalXP <= 0)
                {
                    stats.LockedSummons.Add(mon);
                }
                if (mon.TotalXP > 0)
                {
                    stats.UnlockedSummons.Add(mon);
                }
            }
            


            InputManager.OnMoveLeft += () => movement.X -= 1;
            InputManager.OnMoveRight += () => movement.X += 1;
            InputManager.OnMoveUp += () => movement.Y -= 1;
            InputManager.OnMoveDown += () => movement.Y += 1;
            SceneManager.OnStateChanged += HandleSceneStateChange;
            ScreenTransitionManager.OnFadeToBlackComplete += SetNewTilePosition;
        }
        public SummonedMonster FindSummon(string name)
        {
            return stats.UnlockedSummons.FirstOrDefault(s => s.Name == name)
                ?? stats.LockedSummons.FirstOrDefault(s => s.Name == name);
        }

        public void Update(GameTime gameTime)
        {

 
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (InputManager.IsRightClick())
            {
                Rectangle start = GetHitbox();
                Vector2 target = new Vector2(InputManager.MouseX, InputManager.MouseY);
                var path = CustomPathfinder.BuildPixelPath(start, target);
                SetPath(path);
            }
            MovePlayer();
            movement = Vector2.Zero;
            CheckCurrentPlayerCell();

        }
        private void HandleSceneStateChange(SceneManager.SceneState newState)
        {
            movementPath.Clear();
            allowedToMove = newState == SceneManager.SceneState.Play;
        }
        private void CheckCurrentPlayerCell()
        {
            Vector2 feet = GetFeetCenter();
            var currentCell = TileManager.GetCell(feet);
            if (currentCell != PlayerCurrentTileCell)
            {
                PlayerCurrentTileCell = currentCell;
                TileCellManager.OnEnterNewCell(currentCell); 
                TileManager.OnEnterNewCell(currentCell);
                ProximityManager.OnEnterNewCell(currentCell, feet);
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                Rectangle destination = new Rectangle(
                    (int)(PlayerCord.X),
                    (int)PlayerCord.Y,
                    PlayerWidth,
                    PlayerHeight
                );

                spriteBatch.Draw(Texture, destination, Color.White);
            }
        }

        private void MovePlayer()   
        {
            if (!allowedToMove)
            {
                return;
            }
            // 🔄 Handle direct input movement (keyboard)
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                Vector2 nextPos = PlayerCord + movement * Speed * deltaTime;

                if (CanMoveToCell(nextPos) )
                {
                    nextPos.X = MathHelper.Clamp(nextPos.X, 0, ViewportManager.ScreenWidth - PlayerWidth);
                    nextPos.Y = MathHelper.Clamp(nextPos.Y, -5, ViewportManager.ScreenHeight - PlayerHeight);

                    PlayerCord = nextPos;
                    moveTarget = null; // Cancel click-move if arrow keys are used
                }

                movementPath.Clear(); // Cancel pixel path movement
                return; // Skip path following if manually moving
            }

                // 🧭 Handle path-based movement (click)
                if (movementPath.Count > 0)
                {
                    Vector2 target = movementPath.Peek();
                    Vector2 direction = target - PlayerCord;
                    float distance = direction.Length();

                    if (distance <= Speed * deltaTime)
                    {
                        PlayerCord = target;
                        movementPath.Dequeue();
                    }
                    else
                    {
                        direction.Normalize();
                        PlayerCord += direction * Speed * deltaTime;
                    }
                }
            }
        public Rectangle GetHitbox()    
        {
            int hitboxWidth = PlayerWidth;
            int hitboxHeight = PlayerHeight / 3;

            return new Rectangle(
                (int)(PlayerCord.X + (PlayerWidth / 2f) - (hitboxWidth / 2f)),
                (int)(PlayerCord.Y + PlayerHeight - hitboxHeight),
                hitboxWidth,
                hitboxHeight
            );
        }
        private bool CanMoveToCell(Vector2 nextPos)
        {
            // Expand hitbox dimensions with buffer
            int hitboxWidth = PlayerWidth;
            int hitboxHeight = (PlayerHeight / 3);

            // Predict the new feet hitbox based on where the player would move
            Rectangle futureFeetBox = new Rectangle(
                (int)(nextPos.X + (PlayerWidth / 2f) - (hitboxWidth / 2f)), // center X
                (int)(nextPos.Y + PlayerHeight - (PlayerHeight / 3) ), // move hitbox up by buffer
                hitboxWidth,
                hitboxHeight
            );
            return TileManager.IsCellWalkable( futureFeetBox );
        }
        public void SetPath(List<Vector2> path)
        {
            movementPath.Clear();
            if (path == null || path.Count == 0)
                return;
            debugClickTarget = path.Count > 0 ? path[^1] : null;
            foreach (var point in path)
            {
               
                movementPath.Enqueue(point);
            }
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
            return new Vector2(PlayerCord.X + PlayerWidth / 2f, PlayerCord.Y + PlayerHeight);
        }
        public void SetNewTilePosition()
        {
          
            string dir = PlayerCurrentTileCell.NextTile.NextDirection.ToString();
            float newX = PlayerCord.X;
            float newY = PlayerCord.Y;
            switch (dir)
            {
                case "Right":
                    newX = 0; // Appear at far left
                    break;

                case "Left":
                    newX = ViewportManager.ScreenWidth - PlayerWidth; // Appear at far right
                    break;

                case "Up":
                    newY = ViewportManager.ScreenHeight - PlayerHeight; // Appear at bottom
                    break;

                case "Down":
                    newY = 0; // Appear at top
                    break;
            }
            PlayerCord = new Vector2(newX, newY);
        }
        public PlayerSaveData Save()
{
            var feetCenter = GetFeetCenter();
            return new PlayerSaveData
            {
                Speed = this.Speed,
                Width = this.PlayerWidth,
                Height = this.PlayerHeight,
                TextureKey = "Hero_Blonde", // or store this in a field and use that
                FeetCenterX = feetCenter.X,
                FeetCenterY = feetCenter.Y
    };
}








    }
}
