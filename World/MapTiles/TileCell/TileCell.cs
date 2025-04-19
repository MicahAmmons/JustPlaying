using PlayingAround.Entities.Monster;
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
        public bool HeroSpawnable { get; }
        public bool MonsterSpawnable { get; }



        public TileCell(
            int x,
            int y,
            string texturePath,
            bool walkable = true,
            int z = 0,
            bool heroSpawnable = false,
            bool monsterSpawnable = false,
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
            HeroSpawnable = heroSpawnable;
            MonsterSpawnable = monsterSpawnable;
            BehindOverlay = behindOverlay;
            FrontOverlay = frontOverlay;
            Npc = npc;
            Trigger = trigger;
            NextTile = nextTile;
            CanSpawn = IsWalkable && NextTile == null;
        }


    }
}
