using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
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
        public void DrawTexture(SpriteBatch spriteBatch);
        public DrawSpecificStats DrawSpecifics { get; }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell);
        public void UpdateMonsterTakingDamage(GameTime gameTime);
        public void UpdateMovement(GameTime gameTime);
        
    }
    public interface IAnimatable : IDrawn
    {
        public AnimationController AnimationController { get; }
        public Dictionary<AnimationState, Animation> Animation {  get; }
        public Texture2D SpriteSheet { get; }
        public Direction FacingDirection { get; set; }
        public AnimationState CurrentAnimationState { get; set; }
        public Vector2? MoveTarget { get; set; }
        public Vector2? AnimationDrawPoint { get; set; }
        public void SetFacingDirection(Vector2 vec);
        public void SetCurrentAnimationState();
        public void SetCurrentAnimationStateToIdle();
        public void UpdateAnimation(GameTime gameTime);
        public void PopulateMovementPath(GameTime gameTime);
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

        void Update(GameTime gameTime, float delta);
        public void UpdateTopOfActionStats();
        public AITurnState? DecideAction();
        public void SpendActionPoint();
        bool IsAttackComplete();
        public void SetCurrentEffected(ICombatant combatant, TileCell cell);
        void PerformAttack();
        void ApplyAspect(string aspect, ElementType elementDamage);
        void ApplyDamage(float damage, ElementType elementDamage);
        public void CreateNewAttackVisual();
        public void ClearAttackCycle();
        public void SetCombatantAttackPathingInformation();
        public void ResolveAspects(TickedTiming endOfTurn);
        public void ClearAllAspects();
        public void UpdateCombatPosition(int pos);
        public void MovedOneCell();
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
