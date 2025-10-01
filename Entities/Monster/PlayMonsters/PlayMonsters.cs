using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using System.Net.WebSockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Tiles;
using PlayingAround.Movement;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsters : IAnimatable, IOutOfCombatAnimated
    {
        // A list of combat monsters that this play monster is associated with
        public List<CombatMonster> Monsters { get; set; }
        public TileCell CurrentCell { get; set; }
        public Texture2D Icon { get; set; }
        public string Name { get; set; }
        public DrawSpecificStats DrawSpecifics {  get; set; }
        public string UniqueId {  get; set; }
        public OutOfCombatAnimatedStats OOCombatStats {  get; set; }
        public MovementController MovementController { get; set; }
        public bool ExecutingMove { get; set; } = false;

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
            };
            DrawSpecifics = new DrawSpecificStats()
            {
                IsFlashingRed = false,
                DamageFlashTimer = 0f,
            };

            MovementController = new MovementController(mon.MovementController.AnimationManager);
            MovementController.FinishedTileMove += FinishedMovingOneTile;
            MovementController.FinishedAllMovement += FinishedAllMovement;
            MovementController.CurrentlyMoving += IsCurrentlyMoving;
        }
     
        public void Update(GameTime gameTime)
        {
            PopulateMovementPath(gameTime);
            MovementController.Update(gameTime); 
        }
      
        public void Draw(SpriteBatch spriteBatch)
        {
            var state = SceneManager.CurrentState;
            if (state is SceneState.Dialogue or SceneState.Play or SceneState.MapTileTransition)
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
                int yOffset = animation.YOffset;
                int width = animation.Width;
                int height = animation.Height;
                var pos = TileManager.OffSetFromCenterOfDiamond(drawPoint, width, height);
                Rectangle dest = new Rectangle(
                    (int)pos.X,
                    (int)pos.Y - yOffset,
                    width,
                    height
                );
                Rectangle source = contr.GetCurrentFrame();
                Texture2D texture = animation.SpriteSheet;

                float frameFade = 1;
                if (animation.FadeEffect)
                    frameFade = 1;
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
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (StayPaused(gameTime))
                return;
            if (MovementController.TileMovePath.Count > 0) return;

            
        }
        private void FinishedMovingOneTile()
        {
            OOCombatStats.IsPaused = true;
            MovementController.ApproveNextTileStep();
        }
        private void FindEndPoint()
        {
            var tiles = TileManager.GetWalkableNeighbors(TileManager.GetCell(MovementController.CurrentPos));
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
            MovementController.SetDestinationPoint(cell.CenterPoint);
        }
        public bool StayPaused(GameTime gameTime)
        {
            if (!OOCombatStats.IsPaused) return false;

            OOCombatStats.PauseTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (OOCombatStats.PauseTimer <= 0)
            {
                OOCombatStats.IsPaused = false;
                SetCurrentPauseDuration(); // Preload duration for next pause
                FindEndPoint();
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
            MovementController.CurrentPos = centerPoint;
            MovementController.DrawPoint = centerPoint;
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
    }
}
