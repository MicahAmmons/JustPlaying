using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using PlayingAround.ActFolder;
using PlayingAround.AnimationFolder;
using PlayingAround.ButtonsFolder;
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
using PlayingAround.Movement;
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
        public DrawSpecificStats DrawSpecifics {  get; set; }
        public Texture2D Icon {  get; set; }
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
        public int PositionInOrder { get ; set; }
        public MovementController MovementController {  get; set; }
        public bool ExecutingMove { get; set; } = false;
        public bool StartOfTurnEffectsResolved { get; set; } = false;
        public bool EndOfTurnEffectsResolved { get; set; } = false;
        public bool ExecutingSummon { get; set; } = false;
        public bool ExecutingAttack { get; set ; }
        public ActController ActController { get; set; }

        public static Player LoadFromSave(PlayerSaveData data)
        {
            var player = new Player()
            {
                MovementController = new MovementController(data.AnimationData, data.MovementQuickness, CombatMonsterType.Player)
                {
                    CurrentPos = new Vector2(data.CurrentPosX, data.CurrentPosY),
                    DrawPoint = new Vector2(data.CurrentPosX, data.CurrentPosY)
                },
                DrawSpecifics = new DrawSpecificStats()
                {
                    MovementQuickness = (int)data.MovementQuickness,
                    Width = data.Width,
                    Height = data.Height,
                    MovementPattern = MovementPatternType.Straight
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
                Icon = AssetManager.GetTexture($"{data.TextureKey}"),

        };
            player.MovementController.FinishedTileMove += player.FinishedMovingOneTile;
            player.MovementController.FinishedAllMovement += player.FinishedAllMovement;
            player.MovementController.CurrentlyMoving += player.IsCurrentlyMoving;
            return player;
        }
        public Player()
        {
            
        }
        public void Update(GameTime gameTime, float delta)
        {
            GetHitbox();
            UpdateMonsterTakingDamage(gameTime);
            DrawSpecifics.VEManager.Update(delta);
            MovementController.Update(gameTime);

        }
        public void UpdatePlayerDestinationPoint(Vector2 vec)
        {
            if (SceneManager.IsState(SceneState.Play))
            {
                MovementController.SetDestinationPoint(vec);
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
                (int)(MovementController.CurrentPos.X - (DrawSpecifics.Width / 2f) ),
                (int)(MovementController.CurrentPos.Y - DrawSpecifics.Height /4),
                hitboxWidth,
                hitboxHeight
            );
            RectHitBox = hit;
            return hit;
        }
        public PlayerSaveData Save(PlayerSaveData data)
        {
            data.MovementQuickness = (int)this.DrawSpecifics.MovementQuickness;
            data.CurrentPosX = MovementController.CurrentPos.X;
            data.CurrentPosY = MovementController.CurrentPos.Y;
            return data;
        }
        public Vector2? GetDebugClickTarget() => debugClickTarget;
        public void NewMapTilePosition(Vector2 dir)
        {
            MovementController.ClearMovementPath();

            float newX = MovementController.CurrentPos.X;
            float newY = MovementController.CurrentPos.Y;

            if (dir.X < 0) // Moving left
                newX = ViewportManager.ScreenWidth - MovementController.CurrentPos.X;
            else if (dir.X > 0) // Moving right
                newX = ViewportManager.ScreenWidth - MovementController.CurrentPos.X;

            if (dir.Y < 0) // Moving up
                newY = ViewportManager.ScreenHeight - MovementController.CurrentPos.Y;
            else if (dir.Y > 0) // Moving down
                newY = ViewportManager.ScreenHeight - MovementController.CurrentPos.Y;

            MovementController.CurrentPos = new Vector2(newX, newY);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawSpecifics.VEManager.Draw(spriteBatch);

        }
        public void DrawTexture(SpriteBatch spriteBatch, Effect fx = null)
        {
            if (!MovementController.AllowedToBeDrawn) {
                return; }
            if (MovementController.AnimationManager.CurrentControllers == null ) return;    
            foreach (var contr in MovementController.AnimationManager.CurrentControllers)
            {
                if (contr.Animation == null) continue;
                if (fx != null && contr.Animation.SmokeEffect == false) continue;
                if (fx == null && contr.Animation.SmokeEffect == true) continue;

                Animation animation = contr.Animation;
                bool flipHorizontal = MovementController.FlipHorizontally(animation.DefaultDirection);
                Vector2 drawPoint = MovementController.DrawPoint;
                int width = 128;
                int height = 128;
                var pos = TileManager.OffSetFromCenterOfDiamond(drawPoint, width, height);
                Rectangle dest = new Rectangle(
                    (int)pos.X,
                    (int)pos.Y,
                    width,
                    height
                );
                Rectangle source = contr.GetCurrentFrame();
                Texture2D texture = animation.SpriteSheet;

                float frameFade = 1;
                if (animation.FadeEffect)
                    frameFade = 1 - contr.FadeMultiplier;
                SpriteEffects flip = flipHorizontal
                     ? SpriteEffects.FlipHorizontally
                     : SpriteEffects.None;

                spriteBatch.Draw(
                    texture,
                    dest,
                    source,
                    DrawSpecifics.IsFlashingRed ? Color.Red * frameFade : Color.White * frameFade,
                    0f,                  // rotation
                    Vector2.Zero,        // origin
                    flip,                // 👈 flip goes here
                    0f                   // layerDepth
                );
                if (animation.FadeEffect)
                {
                    Rectangle source2 = contr.GetNextFrame();
                    spriteBatch.Draw(
                         texture,
                         dest,
                          source2,
                          DrawSpecifics.IsFlashingRed ? Color.Red * (1 - frameFade) : Color.White * (1 - frameFade),
                          0f,
                          Vector2.Zero,
                          flip,
                         0f
);
                }
            }
        }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell)
        {
            Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle rect = new Rectangle((int)drawPoint.X, (int)drawPoint.Y - DrawSpecifics.Height / 2, DrawSpecifics.Width, DrawSpecifics.Height);
            spriteBatch.Draw(Icon, rect, Color.White);
        }
        public void UpdateTopOfActionStats()
        {
            CurrentStats.MP = BaseStats.MP;        
        }
        public void SpendActionPoint()
        {
            CurrentStats.AP -= 1;
        }

        public void ResolveEffects(TickedTiming ticked)
        {

            if (Aspects.Count == 0)
            {
                if (ticked == TickedTiming.EndOfTurn) { EndOfTurnEffectsResolved = true; }
                if (ticked == TickedTiming.StartOfTurn) { StartOfTurnEffectsResolved = true; }
                return;
            }
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
            if (ticked == TickedTiming.EndOfTurn) { EndOfTurnEffectsResolved = true; }
            if (ticked == TickedTiming.StartOfTurn) { StartOfTurnEffectsResolved = true; }


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

        private float _movetimer = 0;
        public void FinishedMovingOneTile()
        {
            if (SceneManager.IsState(SceneState.Play))
            {
                MovementController.ApproveNextTileStep();
                return;
            }

 

            //Will make more complicated logic for when mechanics are implemented, such as traps or movement damage or terrarin, etc.
            _movetimer += 0.1667f;
            if (_movetimer >= 3)
            {
                CurrentStats.MP -= 1;
                MovementController.ApproveNextTileStep();
                _movetimer = 0;
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
            DrawSpecifics.VEManager.AddEffect(new Visuals.VisualEffect(MovementController.CurrentPos, new Vector2(0, -1), 1)
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
        public void FinishedAllMovement()
        {
            ExecutingMove = false;
            MovementController.ClearMovementPath();
        }
        public void IsCurrentlyMoving()
        {
            ExecutingMove = true;
        }

        public void BeginAct(Act act)
        {
            throw new NotImplementedException();
        }

        public void CreateNewActController()
        {
            ActController = new ActController();
        }
    }
}

