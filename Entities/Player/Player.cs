using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using PlayingAround.AnimationFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Interfaces;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;


namespace PlayingAround.Entities.Player
{
    public class Player : ICombatant, IOutOfCombatAnimated, ICollidable
    {
        public BaseCombatStats BaseStats { get; set; }
        public CurrentCombatStats CurrentStats { get; set; } 
        public AnimationController AnimationController { get; set; } = new AnimationController();
        public Dictionary<AnimationState, Animation> Animation {  get; set; } = new Dictionary<AnimationState, Animation>();
        public Direction FacingDirection { get; set; } = Direction.Right;
        public Texture2D SpriteSheet { get; set; }
        public AnimationState CurrentAnimationState { get; set; } = AnimationState.IdleRight;
        public DrawSpecificStats DrawSpecifics {  get; set; }
        public Texture2D Icon {  get; set; }
        public Vector2? MoveTarget { get; set; }
        public List<Aspect> Aspects { get; set; } = new List<Aspect>();
        public List<SingleAttack> Attacks { get; set; } = new List<SingleAttack>();
        public bool isDead { get; set; } = false;
        public List<TileCell> MoveableCells { get; set; } = new List<TileCell>();

        public List<Vector2> MovementPath = new();
        public bool AllowedToMove { get; set; } = true; 
        public Vector2[] DiamondHitBox {  get; set; }
        public Rectangle RectHitBox { get; set; }
        private Vector2? debugClickTarget {  get; set; }
        public CombatMonsterType Is { get; set; } = CombatMonsterType.Player;
        public string UniqueId { get; set; } = "Player";
        public Vector2 HitBoxCenter {  get; set; }
        public OutOfCombatAnimatedStats OOCombatStats {  get; set; }
        public Vector2 CurrentPos
        {
            get
            {
                return SceneManager.CurrentState switch
                {
                    SceneState.Combat => CurrentStats.Pos,
                    _ => OOCombatStats.CurrentPos
                };
            }
            set
            {
                if (SceneManager.CurrentState == SceneState.Combat)
                    CurrentStats.Pos = value;
                else
                    OOCombatStats.CurrentPos = value;
            }
        }


        public static Player LoadFromSave(PlayerSaveData data)
        {
            var player = new Player()
            {
                
                DrawSpecifics = new DrawSpecificStats()
                {
                    MovementQuickness = (int)data.MovementQuickness,
                    Width = data.Width,
                    Height = data.Height,
                    AllowedToMove = true
                },
                OOCombatStats = new OutOfCombatAnimatedStats()
                {
                    CurrentPos = new Vector2(data.CurrentPosX, data.CurrentPosY),
                },
                BaseStats = new BaseCombatStats()
                {
                    MP = 4,
                    AP = 3,
                    Health = 10,
                    Initiative = 3
                },
                CurrentStats = new CurrentCombatStats()
                {
                    MP = 4,
                    AP = 3,
                    Health = data.CurrentCombatStats.Health,
                },
                SpriteSheet = AssetManager.GetTexture("PlayerSS"),
                Icon = AssetManager.GetTexture("Hero_Blonde"),
            };
            foreach (var kvp in data.Animations)
            {
                AnimationState state = kvp.Key;
                int row = kvp.Value[0];
                int frames = kvp.Value[1];
                int duration = kvp.Value[2];
                player.Animation[state] = new Animation(player.SpriteSheet, row, frames, duration);
            }
            return player;

        }

        public Player()
        {

        }


        public void Update(GameTime gameTime)
        {
            GetHitbox();
            PopulateMovementPath();
            UpdateAnimation(gameTime);
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play( CurrentAnimationState,Animation[CurrentAnimationState]);
        }
        public void PopulateMovementPath()
        {
            if (MoveTarget != null) 
            {
                Vector2 move = (Vector2)MoveTarget;

                MoveTarget = null;
                MovementPath = CustomPathfinder.GetCellToCellPath(CurrentPos, move);

            }

        }

        public void ClearMovementPath()
        {
            MovementPath.Clear();
        }

       


        public void UpdatePlayerEndPoint(Vector2 vec)
        {
            MoveTarget = vec;
         
        }


        public void GetHitbox()
        {
            Rectangle rect = GetRectangleHitBox();
            Vector2 top = new Vector2(rect.Center.X, rect.Top);
            Vector2 bottom = new Vector2(rect.Center.X, rect.Bottom);
            Vector2 left = new Vector2(rect.Left, rect.Center.Y);
            Vector2 right = new Vector2(rect.Right, rect.Center.Y);

            DiamondHitBox = new Vector2[] { top, right, bottom, left };
        }

        public Rectangle GetRectangleHitBox()
        {
            int hitboxWidth = DrawSpecifics.Width;
            int hitboxHeight = DrawSpecifics.Height/3;

            Rectangle hit = new Rectangle(
                (int)(CurrentPos.X - (DrawSpecifics.Width / 2f) ),
                (int)(CurrentPos.Y - DrawSpecifics.Height /4),
                hitboxWidth,
                hitboxHeight
            );
            RectHitBox = hit;
            return hit;
        }

        public PlayerSaveData Save(PlayerSaveData data)
        {
            data.MovementQuickness = this.DrawSpecifics.MovementQuickness;
            data.CurrentPosX = CurrentPos.X;
            data.CurrentPosY = CurrentPos.Y;
            return data;
        }

        public Vector2? GetDebugClickTarget() => debugClickTarget;

        public void NewMapTilePosition(Vector2 dir)
        {
            ClearMovementPath();

            float newX = CurrentPos.X;
            float newY = CurrentPos.Y;

            if (dir.X < 0) // Moving left
                newX = ViewportManager.ScreenWidth - CurrentPos.X;
            else if (dir.X > 0) // Moving right
                newX = ViewportManager.ScreenWidth - CurrentPos.X;

            if (dir.Y < 0) // Moving up
                newY = ViewportManager.ScreenHeight - CurrentPos.Y;
            else if (dir.Y > 0) // Moving down
                newY = ViewportManager.ScreenHeight - CurrentPos.Y;

            CurrentPos = new Vector2(newX, newY);
        }

        public void SetFacingDirection(Vector2 vec)
        {
            FacingDirection = vec.X <= 0 ? Direction.Right : Direction.Left;
        }

        public void SetCurrentAnimationState()
        {
            CurrentAnimationState = FacingDirection == Direction.Right
              ? AnimationState.WalkRight
              : AnimationState.WalkLeft;
        }
        public void SetCurrentAnimationStateToIdle()
        {
            CurrentAnimationState = FacingDirection == Direction.Right
             ? AnimationState.IdleRight
             : AnimationState.IdleLeft;
        }

        public void UpdateMovement(GameTime gameTime)
        {

            Vector2 nextPoint = MovementPath[0];
            float speed = DrawSpecifics.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - CurrentPos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                CurrentPos = nextPoint;
                MovementPath.RemoveAt(0);
                if (MovementPath.Count <= 0)
                {
                    SetCurrentAnimationStateToIdle();
                }
            }
            else
            {
                direction.Normalize();
                CurrentPos += direction * speed;
                SetFacingDirection(direction);
                SetCurrentAnimationState();
            }
        }
    }
    }

