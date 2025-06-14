using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Proximity;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using PlayingAround.Stats;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PlayingAround.Entities.Player
{
    public class Player
    {
        public float Speed { get; set; }
        public Texture2D Texture { get; private set; }
        public string Name { get; set; }
        public PlayerStats stats { get; set; }
        public int PlayerWidth { get; set; }
        public int PlayerHeight { get; set; }
        public Vector2? MoveTarget = null;
        public List<Vector2> MovementPath = new();

        private Vector2? debugClickTarget = null;
        public Vector2 CurrentPos;

        private TileCell PlayerCurrentTileCell;
        public bool AllowedToMove = true;
        public Dictionary<ElementType, float> Resistances;
        public Vector2[] DiamondHitBox ;
        public Vector2 HitBoxCenter;
        public Rectangle RectHitBox;
        

        public static Player LoadFromSave(PlayerSaveData data)
        {
            var texture = AssetManager.GetTexture(data.TextureKey);

            var player = new Player(texture,data.PlayerSummons, data.Speed)
            {
                CurrentPos = new Vector2(data.CurrentPosX, data.CurrentPosY),
                PlayerHeight = data.Height,
                PlayerWidth = data.Width,
            };
            return player;

        }

        public Player(Texture2D idleTexture, List<SummonsSaveData> summs, float speed = 200f)
        {
            Texture = idleTexture;
            Speed = speed;

            var summonLoader = JsonLoader.LoadSummonProgressions();
            stats = new PlayerStats()
            {
                LockedSummons = new List<SummonedMonster>(),
                UnlockedSummons = new List<SummonedMonster>(),
            };

            foreach (var summon in summs)
            {
                var mon = new SummonedMonster(summon, summonLoader[summon.Name]);
                if (mon.NumberOfKills > 0)
                    stats.UnlockedSummons.Add(mon);
                else
                    stats.LockedSummons.Add(mon);
            }
        }


        public void Update(GameTime gameTime)
        {
            GetHitbox();
            CheckCurrentPlayerCell();
            PopulateMovementPath();
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
        public void Draw(SpriteBatch spriteBatch)
        {
            switch (SceneManager.CurrentState)
            {
                case SceneState.Play:
                    DrawPlayer(spriteBatch);
                    break;
                case SceneState.Dialogue:
                    DrawPlayer(spriteBatch);
                    break;

            }

        }

        public void DrawPlayer(SpriteBatch spriteBatch)
        {
            
                Vector2 current = CurrentPos;
                Vector2 drawOffSet = TileManager.OffSetFromCenterOfDiamond(current,PlayerWidth, PlayerHeight);   
                Rectangle destination = new Rectangle((int)drawOffSet.X, (int)drawOffSet.Y - (PlayerWidth/2), PlayerWidth, PlayerHeight);
                spriteBatch.Draw(Texture, destination, Color.White);
            
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
            int hitboxWidth = PlayerWidth;
            int hitboxHeight = PlayerHeight/3;

            Rectangle hit = new Rectangle(
                (int)(CurrentPos.X - (PlayerWidth / 2f) ),
                (int)(CurrentPos.Y - PlayerHeight /4),
                hitboxWidth,
                hitboxHeight
            );
            RectHitBox = hit;
            return hit;
        }

        public PlayerSaveData Save(PlayerSaveData data)
        {
            data.Speed = this.Speed;
            data.CurrentPosX = CurrentPos.X;
            data.CurrentPosY = CurrentPos.Y;
            return data;
        }

        public List<SummonsSaveData> SavePlayerSummons()
        {
            List<SummonsSaveData> summs = new List<SummonsSaveData>();
            foreach (var sum in this.stats.UnlockedSummons)
            {
                //Dictionary <string, int> abilityPoints = new Dictionary<string, int>();
                //abilityPoints.Add("Defense", sum.Defense);
                //abilityPoints.Add("MaxHealth", sum.MaxHealth);
                //abilityPoints.Add("")

                SummonsSaveData data = new SummonsSaveData()
                {
                    Name = sum.Name,
                    NumberOfKills = sum.NumberOfKills
                };
                summs.Add(data);
            }
            return summs;
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
