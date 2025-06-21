using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsters : IAnimatable, IOutOfCombatAnimated
    {
        // A list of combat monsters that this play monster is associated with
        public List<CombatMonster> Monsters { get; set; }
        public Texture2D Icon { get; set; }
        public string Name { get; set; }
        public List<Vector2> MovePath { get; set; }
        public AnimationController AnimationController {  get; set; }
        public Dictionary<AnimationState, Animation> Animation {  get; set; }
        public Texture2D SpriteSheet {  get; set; }
        public Direction FacingDirection {  get; set; }
        public AnimationState CurrentAnimationState {  get; set; }
        public DrawSpecificStats DrawSpecifics {  get; set; }

        public string UniqueId {  get; set; }

        public OutOfCombatAnimatedStats OOCombatStats {  get; set; }

        public PlayMonsters(PlayMonsterData data, CombatMonster mon)
        {
            Name = mon.Name;
            UniqueId = $"{Name}PM";
            Icon = AssetManager.GetTexture($"{Name}Icon");
            OOCombatStats = new OutOfCombatAnimatedStats()
            {
                IsPaused = false,
                PauseTimer = 0f,
                CurrentPauseDuration = 0f, // Will move immedaitely if at 0
                PauseDurationMax = data.PauseDurationMax,
                PauseDurationMin = data.PauseDurationMin,
            };
            DrawSpecifics = new DrawSpecificStats()
            {
                Width = mon.DrawSpecifics.Width,
                Height = mon.DrawSpecifics.Height,
                MovementQuickness = mon.DrawSpecifics.MovementQuickness,
                MovementPattern = mon.DrawSpecifics.MovementPattern,
                IsFlashingRed = false,
                DamageFlashTimer = 0f,
                AllowedToMove = true
            };
            SpriteSheet = mon.SpriteSheet;
            Animation = mon.Animation;
            AnimationController = new AnimationController();
            CurrentAnimationState = AnimationState.Idle;
            FacingDirection = Direction.Right;
        }
        public void SetFacingDirection(Vector2 vec)
        {
            FacingDirection = vec.X <= 0 ? Direction.Right : Direction.Left;
        }
        public void SetCurrentAnimationState()
        {
            switch (DrawSpecifics.MovementPattern)
            {
                case MovementPatternType.Arc:
                    CurrentAnimationState = FacingDirection == Direction.Right
                      ? AnimationState.BouncingUp
                      : AnimationState.BouncingDown;
                    break;
            }
        }
        public void SetCurrentAnimationStateToIdle()
        {
            CurrentAnimationState = FacingDirection == Direction.Right
             ? AnimationState.IdleRight
             : AnimationState.IdleLeft;
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play(CurrentAnimationState, Animation[CurrentAnimationState]);
        }

        public void UpdateMovement(GameTime gameTime)
        {

                Vector2 nextPoint = MovePath[0];
            float speed = DrawSpecifics.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - OOCombatStats.CurrentPos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                OOCombatStats.CurrentPos = nextPoint;
                MovePath.RemoveAt(0);
                // Set idle animation once path is complete
                if (MovePath.Count <= 0)
                {
                    SetCurrentAnimationStateToIdle();

                }
            }
            else
            {
                direction.Normalize();
                OOCombatStats.CurrentPos += direction * speed;
                SetFacingDirection(direction);
                SetCurrentAnimationState();

            }
        }
    }
}
