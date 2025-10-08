using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ActFolder;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Movement;
using System;
using System.Collections.Generic;
using static CombatStateMachine;

namespace PlayingAround.Interfaces
{
    public interface IEntity
    {
        public Texture2D Icon { get; }
        public string UniqueId { get; }
    }
    public interface IDrawn : IEntity
    {
        public void Draw (SpriteBatch spriteBatch);
        public DrawSpecificStats DrawSpecifics { get; }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell);
        public void UpdateMonsterTakingDamage(GameTime gameTime);
        
    }
    public interface IAnimatable : IDrawn
    {
        public MovementController MovementController {  get; }
        public void FinishedAllMovement();

    }
    public interface ICombatant : IAnimatable
    {
        public BaseCombatStats BaseStats { get; }
        public CurrentCombatStats CurrentStats { get; }
        public List<Aspect> Aspects { get; }
        public bool isDead { get; set; }
        public List<TileCell> MoveableCells { get; set; }
        public CombatMonsterType Is {  get; set; }
        public int PositionInOrder {  get; set; }
        public bool StartOfTurnEffectsResolved { get; set; }
        public bool EndOfTurnEffectsResolved { get; set; }
        public bool ExecutingSummon { get; set; }
        public bool ExecutingAttack { get; set; }
        public bool ExecutingMove { get; set; }
        public SummonAct SummonAct { get; set; }
        public ActController ActController { get; set; }
        void Update(GameTime gameTime, float delta);
        public void UpdateTopOfActionStats();
        public void SpendActionPoint();
        void PerformAttack();
        void ApplyAspect(string aspect, ElementType elementDamage);
        void ApplyDamage(float damage, ElementType elementDamage);
        public void ResolveEffects(TickedTiming endOfTurn);
        public void ClearAllAspects();
        public void UpdateCombatPosition(int pos);
        public void FinishedMovingOneTile();
        public void BeginAct(Act act);
        public void UpdateAct(float delta);
        public void UpdateTopOfRoundStats();


    }


    public interface IProximityTracked
    {
        public Vector2 ProximityTrackingPoint { get; set; }
    }


    public interface IOutOfCombatAnimated
    {
        public OutOfCombatAnimatedStats OOCombatStats {  get; }

    }
    public interface ICollidable
    {
        public Vector2[] DiamondHitBox { get; }
        public Vector2 HitBoxCenter {  get; }
    }
}
