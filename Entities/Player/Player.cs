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
                    AllowedToMove = true,
                    MovementPattern = MovementPatternType.Straight
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
                    Resistances = ResistanceManager.GetResistances(ElementType.Normal)
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
        public void Update(GameTime gameTime, float delta)
        {
            GetHitbox();
            PopulateMovementPath(gameTime);
            UpdateAnimation(gameTime);
            AnimationController.Update(gameTime);
            UpdateMonsterTakingDamage(gameTime);
            DrawSpecifics.VEManager.Update(delta);
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play( CurrentAnimationState,Animation[CurrentAnimationState]);
        }
        public void UpdateMovement(GameTime gameTime)
        {

            Vector2 nextPoint = CurrentStats.MovePath[0];
            float speed = DrawSpecifics.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - CurrentPos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                CurrentPos = nextPoint;
                CurrentStats.MovePath.RemoveAt(0);
                if (CurrentStats.MovePath.Count <= 0)
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
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (MoveTarget != null) 
            {
                List<Vector2> fullVectorPath = new List<Vector2>();
                Vector2 move = (Vector2)MoveTarget;
                TileCell startingCell = TileManager.GetCell(CurrentPos);
                List<TileCell> cellPath = CustomPathfinder.GetCellToCellPath(CurrentPos, move);
                foreach (var endPos in cellPath)
                {
                    if (endPos == TileManager.GetCell(CurrentPos)) continue;
                    List<Vector2> vectorRange = NPCMovement.GetMovementPatternVector2List(DrawSpecifics.MovementPattern, startingCell , endPos);
                    fullVectorPath.AddRange(vectorRange);
                    startingCell = endPos;

                }
                MoveTarget = null;
                CurrentStats.MovePath = fullVectorPath;
            }
        }
        public void ClearMovementPath()
        {
            CurrentStats.MovePath.Clear();
            SetCurrentAnimationStateToIdle();
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
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawSpecifics.VEManager.Draw(spriteBatch);
            if (!DrawSpecifics.AllowedToBeDrawn) return;
            DrawTexture(spriteBatch);
            DrawCellHighlight(spriteBatch);

        }
        public void DrawTexture(SpriteBatch spriteBatch)
        {
            Vector2 drawOffset = TileManager.OffSetFromCenterOfDiamond(CurrentPos, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle destination = new Rectangle
             (
                                  (int)drawOffset.X,
                                  (int)drawOffset.Y - (DrawSpecifics.Width / 2),
                                       DrawSpecifics.Width,
                                       DrawSpecifics.Height
            );
            Rectangle source = AnimationController.GetCurrentFrame();
            spriteBatch.Draw(SpriteSheet, destination, source, DrawSpecifics.IsFlashingRed ? Color.Red : Color.White);
          
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
            CurrentStats.ChooseWhichAttack.Clear();
            CurrentStats.ActionOrder.Clear();
        
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
        public void DrawCellHighlight(SpriteBatch spriteBatch)
        {
            if (DrawSpecifics.DrawCellHightlight)
            {
                DrawSpecifics.DrawCellHightlight = false;
                int shrink = DrawSpecifics.shrink;
                DrawSpecifics.shrink = 0;
                Color col = DrawSpecifics.HighlightCol;
                DrawSpecifics.HighlightCol = ColorPalette.DarkColor;
                Vector2 coords = TileManager.OffSetFromCenterOfDiamond(CurrentStats.Pos);
                Rectangle rect = new Rectangle(
                    (int)coords.X + shrink - MapTile.TileWidth / 2,
                    (int)coords.Y + shrink,
                    128 - shrink * 2,
                    64 - shrink * 2
                );
                Texture2D text = AssetManager.GetTexture("CellDiamond");
                spriteBatch.Draw(text, rect, col);
            }

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
    }
}

