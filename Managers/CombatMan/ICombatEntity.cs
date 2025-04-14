using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

public interface ICombatEntity
{
    int Speed { get; }
    void StartTurn(CombatManager manager);
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}
