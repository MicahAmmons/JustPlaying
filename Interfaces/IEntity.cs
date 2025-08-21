using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public bool ExecutingMove {  get; }
        public void FinishedAllMovement();
        public void IsCurrentlyMoving();
    }
    public interface ICombatant : IAnimatable
    {
        public BaseCombatStats BaseStats { get; }
        public CurrentCombatStats CurrentStats { get; }
        public List<Aspect> Aspects { get; }
        public List<SingleAttack> Attacks { get; }
        public bool isDead { get; set; }
        public List<TileCell> MoveableCells { get; set; }
        public CombatMonsterType Is {  get; set; }
        public int PositionInOrder {  get; set; }
        public bool StartOfTurnEffectsResolved { get; set; }
        public bool EndOfTurnEffectsResolved { get; set; }
        public bool ExecutingSummon { get; set; }
        void Update(GameTime gameTime, float delta);
        public void UpdateTopOfActionStats();
        public CombatState? DecideAction();
        public void SpendActionPoint();
        bool IsAttackComplete();
        public void SetCurrentEffected(ICombatant combatant, TileCell cell);
        void PerformAttack();
        void ApplyAspect(string aspect, ElementType elementDamage);
        void ApplyDamage(float damage, ElementType elementDamage);
        public void CreateNewAttackVisual();
        public void ClearAttackCycle();
        public void SetCombatantAttackPathingInformation();
        public void ResolveEffects(TickedTiming endOfTurn);
        public void ClearAllAspects();
        public void UpdateCombatPosition(int pos);
        public void FinishedMovingOneTile();

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
