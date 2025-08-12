using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using System.Net.WebSockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Tiles;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsters : IAnimatable, IOutOfCombatAnimated
    {
        // A list of combat monsters that this play monster is associated with
        public List<CombatMonster> Monsters { get; set; }
        public TileCell CurrentCell { get; set; }
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
        public Vector2? MoveTarget { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector2? AnimationDrawPoint { get ; set; }

        public PlayMonsters(PlayMonsterData data, CombatMonster mon)
        {
            Name = mon.Name;
            UniqueId = $"{Name}PM";

            OOCombatStats = new OutOfCombatAnimatedStats()
            {
                IsPaused = true,
                PauseTimer = 0f,
                CurrentPauseDuration = 0f, // Will move immedaitely if at 0
                PauseDurationMax = data.PauseDurationMax,
                PauseDurationMin = data.PauseDurationMin,
                MovementQuickness = data.MovementQuickness,
            };
            DrawSpecifics = new DrawSpecificStats()
            {
                Width = mon.DrawSpecifics.Width,
                Height = mon.DrawSpecifics.Height,
                MovementPattern = data.MovementPattern,
                IsFlashingRed = false,
                DamageFlashTimer = 0f,
                AllowedToMove = true
            };
            Animation = mon.Animation;
            AnimationController = new AnimationController();
            CurrentAnimationState = AnimationState.IdleRight;
            FacingDirection = Direction.Right;
            
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
        public void Update(GameTime gameTime)
        {
            PopulateMovementPath(gameTime);
            UpdateAnimation(gameTime);
            AnimationController.Update(gameTime);
            UpdateMovement(gameTime);   
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play(CurrentAnimationState, Animation[CurrentAnimationState]);
        }
        public void UpdateMovement(GameTime gameTime)
        {
            if (OOCombatStats.DestinationPoint == null) return;
            if (!AnimationController.IsFinished) return;

            Vector2 direction = (Vector2)OOCombatStats.DestinationPoint - OOCombatStats.CurrentPos;

            OOCombatStats.CurrentPos = (Vector2)OOCombatStats.DestinationPoint;
            AnimationDrawPoint = null;
            SetFacingDirection(direction);
            SetCurrentAnimationStateToIdle();

        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneState.Play || SceneManager.CurrentState == SceneState.Dialogue)
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
            else drawPoint = OOCombatStats.CurrentPos;
            int width = AnimationController.CurrentAnimation.Width;
            int height = AnimationController.CurrentAnimation.Height;
            var pos = TileManager.OffSetFromCenterOfDiamond(drawPoint,width, height);
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
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (StayPaused(gameTime))
                return;
            if (!AnimationController.IsFinished) 
                return;
            AnimationDrawPoint = OOCombatStats.CurrentPos;
            Vector2 end = FindEndPoint();
            OOCombatStats.DestinationPoint = end;
            Vector2 direction = (Vector2)OOCombatStats.DestinationPoint - OOCombatStats.CurrentPos;
            OOCombatStats.IsPaused = true;
            SetAnimationWalkState(direction);
        }
        private Vector2 FindEndPoint()
        {
            var tiles = TileManager.GetWalkableNeighbors(TileManager.GetCell(OOCombatStats.CurrentPos));
            List<TileCell> cells = new List<TileCell>();
            foreach (var tile in tiles)
            {
                if (!TileManager.DoesCellAlreadyContainPlayerMon(tile))
                {
                    cells.Add(tile);
                }
            }
            int index = RandomHut.rng.Next(cells.Count);
            TileCell cell = cells[index];
            CurrentCell = cell;
            return cell.CenterPoint;
        }
        public bool StayPaused(GameTime gameTime)
        {
            if (!OOCombatStats.IsPaused) return false;

            OOCombatStats.PauseTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (OOCombatStats.PauseTimer <= 0)
            {
                OOCombatStats.IsPaused = false;
                SetCurrentPauseDuration(); // Preload duration for next pause
                return false;
            }

            return true; // Still paused
        }
        public void SetCurrentPauseDuration()
        {
            OOCombatStats.CurrentPauseDuration = MathF.Round(
                (float)(OOCombatStats.PauseDurationMin + RandomHut.rng.NextDouble() *
                (OOCombatStats.PauseDurationMax - OOCombatStats.PauseDurationMin)), 2);

            OOCombatStats.PauseTimer = OOCombatStats.CurrentPauseDuration;
        }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell)
        {
 
        }
        public void UpdateMonsterTakingDamage(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        public void SetPlayMonsterStartingPos(Vector2 centerPoint)
        {
            OOCombatStats.CurrentPos = centerPoint;
        }
    }
}
