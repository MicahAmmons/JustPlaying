using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlayingAround.Managers
{
    public static class ViewportManager
    {
        public static int ScreenWidth { get; private set; }
        public static int ScreenHeight { get; private set; }
        public static Rectangle Bounds => new Rectangle(0, 0, ScreenWidth, ScreenHeight);

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            ScreenWidth = graphicsDevice.Viewport.Width;
            ScreenHeight = graphicsDevice.Viewport.Height;
        }

        public static bool IsPointWithinScreen(Vector2 point, int buffer)
        {
            return point.X >= -buffer &&
                   point.X <= ScreenWidth + buffer &&
                   point.Y >= -buffer &&
                   point.Y <= ScreenHeight + buffer;
        }
    }
}
