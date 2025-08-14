using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;


namespace PlayingAround.World.MapTiles.CellHighlights
{
    public class TileCellHighlights
    {
        public Color InvalidTarget { get; }
        public Color ValidTarget { get; }
        public Color Walkable { get; }
        public Color PlayerStartable { get; }
        public Color MonsterStartable { get; }

        public TileCellHighlights(TileCellHighlightData data)
        {
            InvalidTarget = BuildColor(data.invalidTarget);
            ValidTarget = BuildColor(data.validTarget);
            Walkable = BuildColor(data.walkable);
            PlayerStartable = BuildColor(data.playerStartable);
            MonsterStartable = BuildColor(data.monsterStartable);
        }

        private static Color BuildColor(HighlightStyleData style)
        {
            var baseC = ColorPalette.GetColor(style.color);
            float f = MathHelper.Clamp(style.fade, 0f, 1f);

            // Keep RGB as-is; scale alpha by fade (0..1)
            byte a = (byte)(baseC.A * f);
            return new Color(baseC.R, baseC.G, baseC.B, a);
        }
    }
}