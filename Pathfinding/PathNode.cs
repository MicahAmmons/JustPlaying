namespace PlayingAround.Game.Pathfinding
{
    public class PathNode
    {
        public int X { get; }
        public int Y { get; }

        public int GCost { get; set; } // Distance from start
        public int HCost { get; set; } // Heuristic to target
        public int FCost => GCost + HCost; // Total cost

        public bool Walkable { get; }
        public PathNode Parent { get; set; }

        public PathNode(int x, int y, bool walkable)
        {
            X = x;
            Y = y;
            Walkable = walkable;
        }
    }
}
