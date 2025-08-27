using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ActFolder;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Interfaces;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using PlayingAround.Movement;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using static CombatStateMachine;


namespace PlayingAround.Entities.Monster.CombatMonsters
{
    public class CombatMonster : ICombatant
    {

        public string UniqueId { get; set; }
        public ElementType ElementType { get; set; }
        public string Name { get; set; }
        public string NamePlusLevel { get; set; }




        public BaseCombatStats BaseStats { get; set; }
        public CurrentCombatStats CurrentStats { get; set; }
        public DrawSpecificStats DrawSpecifics { get; set; }
        public List<Aspect> Aspects { get; set; } = new List<Aspect>();
        public CombatMonsterType CombatantIs { get; set; }
        public bool isDead { get; set; } = false;
        public Texture2D Icon { get; set; }
        public CombatMonsterType Is { get; set; }
        public ActController ActController { get; set; }
        public List<TileCell> MoveableCells { get; set; } = new List<TileCell> { };
        public int PositionInOrder { get; set; }
        public MovementController MovementController { get; set; }
        public AttackAct AttackAct { get; set; } = null;
        public MoveAct MoveAct { get; set; } = null;
        public bool StartOfTurnEffectsResolved { get; set; } = false;
        public bool EndOfTurnEffectsResolved { get; set; } = false;
        public bool ExecutingSummon { get; set; } = false;
        public bool ExecutingAttack { get; set; } = false;
        public bool ExecutingMove {  get; set; } = false;
        public SummonAct SummonAct { get ; set ; }



        public CombatMonster(CombatMonsterData data, ElementType element = ElementType.None)
        {
            ElementType = element == ElementType.None ? data.DefaultElementType : element;
            BaseStats = new BaseCombatStats()
            {
                MP = data.BaseStats.MP,
                AP = data.BaseStats.AP,
                Health = data.BaseStats.Health,
                Initiative = data.BaseStats.Initiative,
                Resistances = ResistanceManager.GetResistances(ElementType),
            };
            CurrentStats = new CurrentCombatStats()
            {
                Health = BaseStats.Health,
                AP = BaseStats.AP,
                MP = BaseStats.MP,
                Resistances = ResistanceManager.GetResistances(ElementType),
            };

            ActController = new ActController(data.ActionOrder);
            UniqueId = data.UniqueId;
            BaseStats.Resistances = ResistanceManager.GetResistances(ElementType);
            DrawSpecifics = data.DrawSpecifics;
            Name = UniqueId;

            CombatantIs = CombatMonsterType.AI;
            Icon = AssetManager.GetTexture($"{UniqueId}Icon");
            Is = CombatMonsterType.AI;
            MovementController = new MovementController(data.AnimationData, 0, Is);
            MovementController.FinishedTileMove += FinishedMovingOneTile;
            MovementController.FinishedAllMovement += FinishedAllMovement;
            MovementController.CurrentlyMoving += IsCurrentlyMoving;
        }

        public CombatMonster()
        {

        }



        public void FinishedMovingOneTile()
        {
            if (SceneManager.IsState(SceneState.Play))
            {
                MovementController.ApproveNextTileStep();
                return;
            }
            
            CurrentStats.MP -= 1;

            //Will make more complicated logic for when mechanics are implemented, such as traps or movement damage or terrarin, etc.
            MovementController.ApproveNextTileStep();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawSpecifics.VEManager.Draw(spriteBatch);
            DrawTexture(spriteBatch);
        }
        public void DrawTexture(SpriteBatch spriteBatch)
        {
            if (MovementController.AnimationManager.CurrentControllers == null) return;
            foreach (var contr in MovementController.AnimationManager.CurrentControllers)
            {
                if (contr.Animation == null) continue;


                Animation animation = contr.Animation;
                bool flipHorizontal = MovementController.FlipHorizontally(animation.DefaultDirection);
                Vector2 drawPoint = MovementController.DrawPoint;
                int width = animation.Width;
                int height = animation.Height;
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
        public void Update(GameTime gameTime, float delta)
        {
            UpdateMonsterTakingDamage(gameTime);
            DrawSpecifics.VEManager.Update(delta);
            UpdateAct(delta);
            MovementController.Update(gameTime);
        }
        public void UpdateAct(float delta)
        {
            if (AttackAct != null)
            {
                SingleAttack attack = AttackAct.Attack;
                Vector2 currentPos = MovementController.CurrentPos;
                Vector2 targetPos = AttackAct.EffectedTargets.First().Key.MovementController.CurrentPos;
                Vector2 direction = targetPos - currentPos;
                direction.Normalize();
                MovementController.SetAttackAnimation(direction);
                int frame = MovementController.AnimationManager.CurrentControllers.First().GetCurrentFrameIndex();

                if (attack.AttackPerformedFrame <= frame)
                {
                    if (!AttackAct.Attack.IsFinished)
                    PerformAttack();
                }
                if (attack.IsFinished && MovementController.AnimationManager.IsFinished)
                {
                    SpendActionPoint();
                    MovementController.SetCurrentAnimationStateToIdle();
                    AttackAct.ClearActParams();
                    AttackAct = null;
                    ExecutingAttack = false;
                }
            }
            if (MoveAct != null)
            {
                MovementController.SetMovePath(MoveAct.ActMovementCellPath);
                MoveAct = null;
            }
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
        public void PerformAttack()
        {
            AttackAct.Attack.IsFinished = true;
            foreach (var comb in AttackAct.EffectedTargets)
            {
                ICombatant combatant = comb.Key;
                TileCell cell = comb.Value;
                AttackManager.PerformAttack(AttackAct.Attack, combatant, cell);
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
        public void UpdateTopOfActionStats()
        {

        }
        public void UpdateTopOfRoundStats()
        {
            StartOfTurnEffectsResolved = false;
            EndOfTurnEffectsResolved = false;
            CurrentStats.AP = BaseStats.AP;
            CurrentStats.MP = BaseStats.MP;
        }
        public void BeginAct(Act act)
        {
            switch (act.ActType)
            {
                case ActType.Attack:
                    ExecutingAttack = true;
                    AttackAct = (AttackAct)act;
                    MovementController.AnimationManager.ResetStates();
                    break;
                case ActType.Move:
                    ExecutingMove = true;
                    MoveAct = (MoveAct)act;
                    break;
                case ActType.EndTurn:
                    break;
            }
        }
        public void SpendActionPoint()
        {
            CurrentStats.AP -= 1;
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
            CreateNumericalDamageVE(finalDamage, elementDamage);
           
            DrawSpecifics.IsFlashingRed = true;
            DrawSpecifics.DamageFlashTimer = 0.5f;
        }
        public void CreateNumericalDamageVE(int damage, ElementType elementDamage)
        {
            Vector2 perimeterPos = GetRandomPointOnPerimeter(MovementController.CurrentPos, DrawSpecifics.Width, DrawSpecifics.Height);
            DrawSpecifics.VEManager.AddEffect(new Visuals.VisualEffect(perimeterPos, new Vector2(0, -1), 1)
            {
                Color = ColorPalette.GetElementColor(elementDamage),
                Text = $"{damage}",
            });
        }
        private static Vector2 GetRandomPointOnPerimeter(Vector2 bottomCenter, int width, int height)
        {
            float halfWidth = width / 2f;
            float topY = bottomCenter.Y - height;
            float leftX = bottomCenter.X - halfWidth;
            float rightX = bottomCenter.X + halfWidth;

            int side = RandomHut.rng.Next(4); // 0=top, 1=right, 2=bottom, 3=left
            switch (side)
            {
                case 0: // top
                    return new Vector2(leftX + RandomHut.rng.Next(width), topY);
                case 1: // right
                    return new Vector2(rightX, topY + RandomHut.rng.Next(height));
                case 2: // bottom
                    return new Vector2(leftX + RandomHut.rng.Next(width), bottomCenter.Y);
                case 3: // left
                default:
                    return new Vector2(leftX, topY + RandomHut.rng.Next(height));
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
    }
    public enum CombatMonsterType
    {
        Summoned,
        AI,
        Player,
        Self,
        PlayMonster
    }
}
