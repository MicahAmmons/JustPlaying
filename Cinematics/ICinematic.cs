using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;

public interface ICinematic
{
    void Start();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
    bool IsFinished { get; }
    SceneManager.SceneState OnFinishState { get; }
    SpriteFont font { get; }
}
