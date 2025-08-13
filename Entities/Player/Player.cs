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
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json.Serialization;


namespace PlayingAround.Entities.Player
{
    public class Player : ICombatant, IOutOfCombatAnimated, ICollidable
    {
        public BaseCombatStats BaseStats { get; set; }
        [JsonIgnore] public CurrentCombatStats CurrentStats { get; set; } 
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
        public int PositionInOrder { get ; set; }
        public Vector2? AnimationDrawPoint { get ; set; }
        public List<Vector2> PlayerOOCMovePath { get; set; } = new List<Vector2>();

        public static Player LoadFromSave(PlayerSaveData data)
        {
            var player = new Player()
            {

                DrawSpecifics = new DrawSpecificStats()
                {
                    MovementQuickness = (int)data.MovementQuickness,
                    Width = data.Width,
                    Height = data.Height,
                    AllowedToMove = true,
                    MovementPattern = MovementPatternType.Straight
                },
                OOCombatStats = new OutOfCombatAnimatedStats()
                {
                    CurrentPos = new Vector2(data.CurrentPosX, data.CurrentPosY),
                },
                BaseStats = new BaseCombatStats()
                {
                    MP = 6,
                    AP = 2,
                    Health = 10,
                    Initiative = 3
                },
                CurrentStats = new CurrentCombatStats()
                {
                    MP = 4,
                    AP = 3,
                    Health = data.Health,
                    Resistances = ResistanceManager.GetResistances(ElementType.Normal)
                },
                SpriteSheet = AssetManager.GetTexture("PlayerSS"),
                Icon = AssetManager.GetTexture("Hero_Blonde"),
            };
            foreach (var kvp in data.AnimationData)
            {
                AnimationState state = kvp.Key;
                AnimationData datas = kvp.Value;
                player.Animation[state] = new Animation(datas.FrameCount, datas.FrameWidth, datas.FrameHeight, (int)datas.FrameDurationMs, datas.Row, datas.IsLooping, datas.SpriteSheetName, datas.EndOfCyclePause);
            }

            return player;
        }
        public Player()
        {
        }
        public void Update(GameTime gameTime, float delta)
        {
            GetHitbox();
            PopulateMovementPath(gameTime);
            UpdateAnimation(gameTime);
            AnimationController.Update(gameTime);
            UpdateMonsterTakingDamage(gameTime);
            DrawSpecifics.VEManager.Update(delta);
            UpdateMovement(gameTime);
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play( CurrentAnimationState,Animation[CurrentAnimationState]);
        }
        public void UpdateMovement(GameTime gameTime)
        {
            if (CurrentStats.MovePath.Count > 0)
            {
                if (!AnimationController.IsFinished) return;
                AnimationDrawPoint = CurrentPos;
                CurrentPos = CurrentStats.MovePath[0].CenterPoint;
                CurrentStats.MovePath.RemoveAt(0);
                Vector2 direction = (Vector2)CurrentStats.DestinationPoint - CurrentStats.Pos;
                SetAnimationWalkState(direction);
            }
            if (PlayerOOCMovePath.Count > 0)
            {
                Vector2 nextPoint = PlayerOOCMovePath[0];

                float speed = DrawSpecifics.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

                Vector2 direction = nextPoint - CurrentPos;
                float distance = direction.Length();

                if (distance <= speed)
                {
                    CurrentPos = nextPoint;
                    PlayerOOCMovePath.RemoveAt(0);
                    if (PlayerOOCMovePath.Count == 0)
                    {
                        SetCurrentAnimationStateToIdle();
                    }
                }
                else
                {
                    direction.Normalize();
                    CurrentPos += direction * speed;
                    SetFacingDirection(direction);
                    SetAnimationWalkState(direction);
                }
            }
          
        }
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (OOCombatStats.DestinationPoint != null)
            {
                List<Vector2> cellPath = GridMovement.BuildStraightLinePath(CurrentPos, (Vector2)OOCombatStats.DestinationPoint);
                OOCombatStats.DestinationPoint = null;
                if (cellPath == null || cellPath.Count == 0) // Abort early if path is empty
                    return;
                if (cellPath[0] == CurrentPos) // Remove CurrentPos if it's the first point in the path
                    cellPath.RemoveAt(0);
                if (cellPath.Count == 0) // All points were removed, nothing left to move to
                    return;
                PlayerOOCMovePath = cellPath;
            }
            if (CurrentStats.DestinationPoint != null)
            {
                CurrentStats.MovePath = GridMovement.GetCellToCellPath(CurrentPos, (Vector2)CurrentStats.DestinationPoint);
                CurrentStats.DestinationPoint = null;
            }

        }

        public void ClearMovementPath()
        {
            PlayerOOCMovePath.Clear();
            CurrentStats.MovePath.Clear();
            SetCurrentAnimationStateToIdle();
        }
        public void UpdatePlayerDestinationPoint(Vector2 vec)
        {
            if (SceneManager.IsState(SceneState.Play))
            {
                OOCombatStats.DestinationPoint = vec;
            }
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
        public void SetFacingDirection(Vector2 direction)
        {
            if (direction != Vector2.Zero)
                direction.Normalize();

            if (direction.X > 0 && direction.Y < 0)
                FacingDirection = Direction.UpRight;
            else if (direction.X < 0 && direction.Y < 0)
                FacingDirection = Direction.UpLeft;
            else if (direction.X > 0 && direction.Y > 0)
                FacingDirection = Direction.DownRight;
            else
                FacingDirection = Direction.DownLeft;
        }
        public void SetCurrentAnimationState()
        {
        }
        public void SetAnimationWalkState(Vector2 direction)
        {
            SetFacingDirection(direction);
            CurrentAnimationState = FacingDirection switch
            {
                Direction.UpRight => AnimationState.WalkUpRight,
                Direction.UpLeft => AnimationState.WalkUpLeft,
                Direction.DownRight => AnimationState.WalkDownRight,
                Direction.DownLeft => AnimationState.WalkDownLeft,
                _ => CurrentAnimationState
            };
        }
        public void SetCurrentAnimationStateToIdle()
        {
            if (FacingDirection == Direction.Right ||
                FacingDirection == Direction.UpRight ||
                FacingDirection == Direction.DownRight)
            {
                CurrentAnimationState = AnimationState.IdleRight;
            }
            else if (FacingDirection == Direction.Left ||
                     FacingDirection == Direction.UpLeft ||
                     FacingDirection == Direction.DownLeft)
            {
                CurrentAnimationState = AnimationState.IdleLeft;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawSpecifics.VEManager.Draw(spriteBatch);
            if (!DrawSpecifics.AllowedToBeDrawn) return;
            DrawTexture(spriteBatch);

        }
        public void DrawTexture(SpriteBatch spriteBatch)
        {
            if (AnimationController.CurrentAnimation == null) return;
            Vector2 drawPoint = new Vector2(0, 0);
            if (AnimationDrawPoint != null)
            {
                drawPoint = (Vector2)AnimationDrawPoint;
            }
            else drawPoint = CurrentPos;
            int width = AnimationController.CurrentAnimation.Width;
            int height = AnimationController.CurrentAnimation.Height;
            var pos = TileManager.OffSetFromCenterOfDiamond(drawPoint, width, height);
            Rectangle dest = new Rectangle(
                (int)pos.X,
                (int)pos.Y,
                width,
                height
            );
            Rectangle source = AnimationController.GetCurrentFrame();
            Texture2D texture = AnimationController.CurrentAnimation.SpriteSheet;
            spriteBatch.Draw(texture, dest, source, DrawSpecifics.IsFlashingRed ? Color.Red : Color.White);

        }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell)
        {
            Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle rect = new Rectangle((int)drawPoint.X, (int)drawPoint.Y - DrawSpecifics.Height / 2, DrawSpecifics.Width, DrawSpecifics.Height);
            spriteBatch.Draw(SpriteSheet, rect, AnimationController.GetCurrentFrame(),  Color.White);
        }
        internal void ToggleDrawn()
        {
            DrawSpecifics.AllowedToBeDrawn = !DrawSpecifics.AllowedToBeDrawn;
        }
        public void UpdateTopOfActionStats()
        {
            CurrentStats.MP = BaseStats.MP;
            CurrentStats.Actions.Clear();
        
        }
        public CombatStateMachine.AITurnState? DecideAction()
        {
            throw new NotImplementedException();
        }
        public void SpendActionPoint()
        {
            CurrentStats.AP -= 1;
        }
        public void ResolveAspects(TickedTiming ticked)
        {

            if (Aspects.Count == 0) return;
            foreach (var aspect in Aspects)
            {
                if (aspect.WhenTicked != ticked) continue;
                if (aspect.IsDamage)
                {
                    ApplyDamage(aspect.Damage, aspect.DamageType);
                    aspect.Duration -= 1;
                }
                if (aspect.Duration == 0) Aspects.Remove(aspect);
            }
        }
        public bool IsAttackComplete()
        {
            throw new NotImplementedException();
        }
        public void SetCurrentEffected(ICombatant combatant, TileCell cell)
        {
            throw new NotImplementedException();
        }
        public void PerformAttack()
        {
            throw new NotImplementedException();
        }
        public void MovedOneCell()
        {
            // Logic to check for traps or damages or things that stop movement or damage per movement
            CurrentStats.MP -= 1;
            CurrentStats.MovePath.RemoveAt(0);
            if (CurrentStats.MovePath.Count <= 0)
            {
                SetCurrentAnimationStateToIdle();
            }
        }
        public void ApplyAspect(string aspect, ElementType elementDamage)
        {
            Aspect asp = AspectManager.GetAspect(aspect, elementDamage);
            Aspects.Add(asp);
        }
        public void ApplyDamage(float damage, ElementType elementDamage)
        {
            int finalDamage = (int)MathF.Round(CurrentStats.Resistances[elementDamage] * damage);
            CurrentStats.Health -= finalDamage;
            DrawSpecifics.VEManager.AddEffect(new Visuals.VisualEffect(CurrentPos, new Vector2(0, -1), 1)
            {
                Color = ColorPalette.GetElementColor(elementDamage),
                Text = $"{finalDamage}",
            });
            DrawSpecifics.IsFlashingRed = true;
            DrawSpecifics.DamageFlashTimer = 0.5f;
        }
        public void CreateNewAttackVisual()
        {
            throw new NotImplementedException();
        }
        public void ClearAttackCycle()
        {
            throw new NotImplementedException();
        }
        public void SetCombatantAttackPathingInformation()
        {
            throw new NotImplementedException();
        }
        public void UpdateMonsterTakingDamage(GameTime gameTime)
        {
            if (DrawSpecifics.IsFlashingRed)
            {
                DrawSpecifics.DamageFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds; ;
                if (DrawSpecifics.DamageFlashTimer <= 0f)
                {
                    DrawSpecifics.IsFlashingRed = false;
                }
            }


        }
        public void ClearAllAspects()
        {
            Aspects.Clear();
        }
        public void UpdateCombatPosition(int pos)
        {
            PositionInOrder = pos;
        }
    }
}

