using PlayingAround.Entities.Monster;

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
        public string? Monster { get; }
        public string? Trigger { get; }
        public NextTileData? NextTile {  get; }
        


        public TileCell(
            int x,
            int y,
            string texturePath,
            bool walkable = true,
            int z = 0,
            string? behindOverlay = null,
            string? frontOverlay = null,
            string? npc = null,
            string? monster = null,
            string? trigger = null,
            NextTileData? nextTile = null)
        {
            X = x;
            Y = y;
            TexturePath = texturePath;
            IsWalkable = walkable;
            Z = z;
            BehindOverlay = behindOverlay;
            FrontOverlay = frontOverlay;
            Npc = npc;
            Monster = monster;
            Trigger = trigger;
            NextTile = nextTile;
        }

    }
}
