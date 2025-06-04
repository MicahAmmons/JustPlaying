using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Managers.Tiles;
using System.Collections.Generic;


namespace PlayingAround.Game.Map
{
    public class TileCell
    {
        public int X { get; }
        public int Y { get; }
        public string TexturePath { get; }
        public bool IsWalkable { get; }
        public int Z { get; }
        public string? BehindOverlay { get; }
        public string? FrontOverlay { get; }
        public string? Npc { get; }
        public string? Trigger { get; }
        public NextTileData? NextTile {  get; }
        public bool PlayerSpawnable { get; }
        public bool MonsterSpawnable { get; }
        public bool PlayMonsterSpawnable { get; }
        public CombatMonster CombatMonster { get; set; }
        public bool BlockedByMonster { get; set; } = false;
        public Vector2 CenterPoint { get; set; }
        public string NPCName { get; set; }


        public TileCell(int x, int y)
        {
            X = x;
            Y = y;
        }
        public TileCell(TileCellData data)        
        {
            X = data.X;
            Y = data.Y;
            TexturePath = "default" ;
            IsWalkable = data.Walkable;
            Z = Z;
            PlayerSpawnable = data.PlayerSpawnable;
            MonsterSpawnable = data.MonsterSpawnable;
            PlayMonsterSpawnable = data.PlayMonsterSpawnable;
            BehindOverlay = data.BehindOverlay;
            FrontOverlay = data.FrontOverlay;
            NPCName = data.NPCName;
            Trigger = data.Trigger;
            NextTile = data.NextTile;
            CenterPoint = new Vector2(data.X * MapTile.TileWidth, data.Y * MapTile.TileHeight );
        }


    }
}
