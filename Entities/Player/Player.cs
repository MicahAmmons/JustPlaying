using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using PlayingAround.AnimationFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Proximity;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PlayingAround.Entities.Player
{
    public class Player
    {
        public BaseCombatStats BaseCombatStats { get; set; }
        public CurrentCombatStats CurrentCombatStats { get; set; } 
        public AnimationController AnimationController { get; set; } = new AnimationController();
        public Dictionary<AnimationState, Animation> Animation {  get; set; } = new Dictionary<AnimationState, Animation>();
        public Direction FacingDirection { get; set; } = Direction.Right;
        public Texture2D PlayerSpriteSheet { get; set; }
        public AnimationState CurrentAnimationState { get; set; } = AnimationState.IdleRight;
        public DrawSpecificStats DrawSpecifics {  get; set; }
        public Vector2 CurrentPos;
        public Texture2D Icon;

        public Vector2? MoveTarget = null;
        public List<Vector2> MovementPath = new();
        public bool AllowedToMove = true;
        private TileCell PlayerCurrentTileCell;
        public Vector2[] DiamondHitBox ;
        public Vector2 HitBoxCenter;
        public Rectangle RectHitBox;
        private Vector2? debugClickTarget;

        public static Player LoadFromSave(PlayerSaveData data)
        {
            var player = new Player()
            {
                CurrentPos = new Vector2(data.CurrentPosX, data.CurrentPosY),
                DrawSpecifics = new DrawSpecificStats()
                {
                    MovementQuickness = (int)data.MovementQuickness,
                    Width = data.Width,
                    Height = data.Height,
                },
                BaseCombatStats = new BaseCombatStats()
                {
                    MP = 4,
                    AP = 3,
                    Health = 10,
                    Initiative = 3
                },
                CurrentCombatStats = new CurrentCombatStats()
                {
                    Health = data.CurrentCombatStats.Health,
                },
                PlayerSpriteSheet = AssetManager.GetTexture("PlayerSS"),
                Icon = AssetManager.GetTexture("Hero_Blonde"),
            };
            foreach (var kvp in data.Animations)
            {
                AnimationState state = kvp.Key;
                int row = kvp.Value[0];
                int frames = kvp.Value[1];
                int duration = kvp.Value[2];
                player.Animation[state] = new Animation(player.PlayerSpriteSheet, row, frames, duration);
            }
            return player;

        }

        public Player()
        {
        }


        public void Update(GameTime gameTime)
        {
            GetHitbox();
            CheckCurrentPlayerCell();
            PopulateMovementPath();
            UpdateAnimation(gameTime);
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play( CurrentAnimationState,Animation[CurrentAnimationState]);
        }
        public void PopulateMovementPath()
        {
            if (MoveTarget != null) 
            {
                Vector2 move = (Vector2)MoveTarget;

                MoveTarget = null;
                MovementPath = CustomPathfinder.GetCellToCellPath(CurrentPos, move);

            }

        }

        public void ClearMovementPath()
        {
            MovementPath.Clear();
        }

       


        public void UpdatePlayerEndPoint(Vector2 vec)
        {
            MoveTarget = vec;
         
        }

        private void CheckCurrentPlayerCell()
        {
            Vector2 feet = CurrentPos;
            var currentCell = TileManager.GetCell(feet);
            if (currentCell != PlayerCurrentTileCell)
            {
                PlayerCurrentTileCell = currentCell;
                TileManager.OnEnterNewCell(currentCell);
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


    }
}
