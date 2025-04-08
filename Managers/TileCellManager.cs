using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using System;

public static class TileCellManager
{
    private static Texture2D arrowTexture;
    private static Vector2? arrowPosition;
    private static float arrowRotation;
    private static Rectangle? arrowHitbox;

    public static TileCell PlayerCurrentCell;

    public static void Initialize()
    {
        arrowTexture = AssetManager.GetTexture("3Arrows");
    }

    public static void Update(GameTime gameTime)
    {
        if (PlayerCurrentCell.NextTile != null)
            DisplayArrow();
        else
            ClearArrow();
    }

    public static void OnEnterNewCell(TileCell cell)
    {
        if (PlayerCurrentCell != cell)
        {
            PlayerCurrentCell = cell;
        }
    }

    private static void DisplayArrow()
    {
        Vector2 cellCenter = new Vector2(
            PlayerCurrentCell.X * MapTile.TileWidth + MapTile.TileWidth / 2f,
            PlayerCurrentCell.Y * MapTile.TileHeight + MapTile.TileHeight / 2f
        );

        Vector2 offset = PlayerCurrentCell.NextTile.NextDirection switch
        {
            "Up" => new Vector2(0, 0),
            "Down" => new Vector2(0, -5),
            "Left" => new Vector2(0, 0),
            "Right" => new Vector2(0, 0),
            _ => Vector2.Zero
        };

        arrowPosition = cellCenter + offset;

        arrowRotation = PlayerCurrentCell.NextTile.NextDirection switch
        {
            "Up" => 0,
            "Down" => MathF.PI,
            "Left" => -MathF.PI / 2,
            "Right" => MathF.PI / 2,
            _ => 0
        };
        int size = (int)(MapTile.TileWidth / (float)arrowTexture.Width * 2); // or whatever your arrow scale is
        arrowHitbox = new Rectangle(
            (int)(arrowPosition.Value.X - size / 2f),
            (int)(arrowPosition.Value.Y - size / 2f),
            size,
            size
        );

    }





    private static void ClearArrow()
    {
        arrowHitbox = null;
        arrowPosition = null;
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        if (arrowPosition.HasValue)
        {
            float scale =  (MapTile.TileWidth / (float)arrowTexture.Width *2);

            spriteBatch.Draw(
                arrowTexture,
                arrowPosition.Value,
                null,
                Color.White,
                arrowRotation,
                new Vector2(arrowTexture.Width / 2f, arrowTexture.Height / 2f),
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }

}
