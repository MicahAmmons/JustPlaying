using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Summons;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Proximity;
using PlayingAround.Stats;
using PlayingAround.Utils;
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
        public int PlayerWidth { get; set; } = 64;
        public int PlayerHeight { get; set; } = 64;
        public Vector2? MoveTarget = null;
        public List<Vector2> MovementPath = new();
        public Vector2 PlayerCord;
        private Vector2? debugClickTarget = null;

        private TileCell PlayerCurrentTileCell;
        public bool AllowedToMove = true;
        public Dictionary<string, float> PlayerResistances;
        public Rectangle HitBox;
        public Vector2 HitBoxCenter;
        

        public static Player LoadFromSave(PlayerSaveData data)
        {
            var texture = AssetManager.GetTexture(data.TextureKey);
            float offsetX = data.FeetCenterX - (data.Width / 2);
            float offsetY = data.FeetCenterY - data.Height;
            var player = new Player(texture, new Vector2(offsetX, offsetY), data.PlayerSummons, data.Speed)
            {
                PlayerWidth = data.Width,
                PlayerHeight = data.Height
            };
            return player;
        }

        public Player(Texture2D idleTexture, Vector2 startPosition, List<SummonsSaveData> summs, float speed = 200f)
        {
            Texture = idleTexture;
            PlayerCord = startPosition;
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
                if (mon.TotalXP > 0)
                    stats.UnlockedSummons.Add(mon);
                else
                    stats.LockedSummons.Add(mon);
            }
        }


        public void Update(GameTime gameTime)
        {
            GetHitbox();
            GetFeetCenter();
            CheckCurrentPlayerCell();
            PopulateMovementPath();
        }

        public void PopulateMovementPath()
        {
            if (MoveTarget != null) 
            {
                Vector2? endDes = MoveTarget;
                MoveTarget = null;
                List<Vector2> list = CustomPathfinder.BuildPixelPath(HitBox, endDes);
                if (list.Count > 0 && list != null)
                {
                    MovementPath = list;
                }
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
                case SceneManager.SceneState.Play:
                    DrawPlayer(spriteBatch);
                    break;

            }

        }
        public void DrawPlayer(SpriteBatch spriteBatch)
        {
            if (Texture == null) return;
            {
                Rectangle destination = new Rectangle(
                    (int)PlayerCord.X,
                    (int)PlayerCord.Y,
                    PlayerWidth,
                    PlayerHeight
                );
                spriteBatch.Draw(Texture, destination, Color.White);
            }
        }

        public void DrawDebugPath(SpriteBatch spriteBatch, Texture2D debugPixel)
        {
            foreach (var point in MovementPath)
            {
                Rectangle cellRect = new Rectangle(
                    (int)point.X,
                    (int)point.Y,
                    MapTile.TileWidth,
                    MapTile.TileHeight
                );
                spriteBatch.Draw(debugPixel, cellRect, Color.Yellow * 0.4f);
            }
        }


        public void UpdatePlayerEndPoint(Vector2 vec)
        {
            MoveTarget = vec;

        }

        private void CheckCurrentPlayerCell()
        {
            Vector2 feet = HitBoxCenter;
            var currentCell = TileManager.GetCell(feet);
            if (currentCell != PlayerCurrentTileCell)
            {
                PlayerCurrentTileCell = currentCell;
                TileCellManager.OnEnterNewCell(currentCell);
                TileManager.OnEnterNewCell(currentCell);
                ProximityManager.OnEnterNewCell(currentCell, feet);
            }
        }

        public void GetHitbox()
        {
            int hitboxWidth = PlayerWidth;
            int hitboxHeight = PlayerHeight / 3;

            Rectangle hit = new Rectangle(
                (int)(PlayerCord.X + (PlayerWidth / 2f) - (hitboxWidth / 2f)),
                (int)(PlayerCord.Y + PlayerHeight - hitboxHeight),
                hitboxWidth,
                hitboxHeight
            );
            HitBox = hit;
        }

        public void GetFeetCenter()
        {
            HitBoxCenter = new Vector2(PlayerCord.X + PlayerWidth / 2f, PlayerCord.Y + PlayerHeight);
        }
        public PlayerSaveData Save()
        {
            var feetCenter = HitBoxCenter;
            return new PlayerSaveData
            {
                Speed = this.Speed,
                Width = this.PlayerWidth,
                Height = this.PlayerHeight,
                TextureKey = "Hero_Blonde",
                FeetCenterX = feetCenter.X,
                FeetCenterY = feetCenter.Y,
            };
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
    }
}
