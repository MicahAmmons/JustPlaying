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
        public bool CanSpawn { get; }
        public bool PlayerSpawnable { get; }
        public bool MonsterSpawnable { get; }
        public bool PlayMonsterSpawnable { get; }
        public CombatMonster CombatMonster { get; set; }
        public bool BlockedByMonster { get; set; } = false;
        public Vector2 CenterPoint { get; set; }


        public TileCell(int x, int y)
        {
            X = x;
            Y = y;
        }
        public TileCell(
            int x,
            int y,
            string texturePath,
            bool walkable = true,
            int z = 0,
            bool playerSpawnable = false,
            bool monsterSpawnable = false,
            bool playMonsterSpawnable = false,
            string? behindOverlay = null,
            string? frontOverlay = null,
            string? npc = null,
            string? trigger = null,
            NextTileData? nextTile = null)



           
        {
            X = x;
            Y = y;
            TexturePath = texturePath;
            IsWalkable = walkable;
            Z = z;
            PlayerSpawnable = playerSpawnable;
            MonsterSpawnable = monsterSpawnable;
            PlayMonsterSpawnable = playMonsterSpawnable;
            BehindOverlay = behindOverlay;
            FrontOverlay = frontOverlay;
            Npc = npc;
            Trigger = trigger;
            NextTile = nextTile;
            CanSpawn = IsWalkable && NextTile == null;
            CenterPoint = new Vector2(x * MapTile.TileWidth, y * MapTile.TileHeight );
        }


    }
}
