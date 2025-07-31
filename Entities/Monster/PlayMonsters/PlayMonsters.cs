using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

        public PlayMonsters(PlayMonsterData data, CombatMonster mon)
        {
            Name = mon.Name;
            UniqueId = $"{Name}PM";
            Icon = AssetManager.GetTexture($"{Name}Icon");

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
            //SpriteSheet = mon.SpriteSheet;\
            SpriteSheet = AssetManager.GetTexture("PlayerSS");
            Animation = mon.Animation;
            AnimationController = new AnimationController();
            CurrentAnimationState = AnimationState.IdleRight;
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
                        ? AnimationState.WalkRight
                        : AnimationState.WalkLeft;
                    break;
            }
        }
        public void SetCurrentAnimationStateToIdle()
        {
            CurrentAnimationState = FacingDirection == Direction.Right
             ? AnimationState.IdleRight
             : AnimationState.IdleLeft;
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
            if (MovePath == null || MovePath.Count <= 0 || !DrawSpecifics.AllowedToMove) return;
            Vector2 nextPoint = MovePath[0];
            float speed = OOCombatStats.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

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
        public void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneState.Play || SceneManager.CurrentState == SceneState.Dialogue)
            DrawTexture(spriteBatch);
        }
        public void DrawTexture(SpriteBatch spriteBatch)
        {
            var pos = TileManager.OffSetFromCenterOfDiamond(OOCombatStats.CurrentPos, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle dest = new Rectangle(
                (int)pos.X,
                (int)pos.Y,
                DrawSpecifics.Width,
                DrawSpecifics.Height
            );
            Rectangle source = AnimationController.GetCurrentFrame();
            spriteBatch.Draw(SpriteSheet, dest, source, DrawSpecifics.IsFlashingRed ? Color.Red : Color.White);

        }
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (MovePath != null && MovePath.Count > 0)
                return;
            if (StayPaused(gameTime))
                return;
            Vector2 end = FindEndPoint();

            MovePath = NPCMovement.GetMovementPatternVector2List(
                                                                DrawSpecifics.MovementPattern,
                                                                TileManager.GetCell(OOCombatStats.CurrentPos),
                                                                TileManager.GetCell(end)
            );
            OOCombatStats.IsPaused = true;
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
