using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlayingAround.Managers;

public static class CinematicRuler
{
    private static ICinematic _current;
    private static CurrentCinematic _type = CurrentCinematic.None;

    public static CurrentCinematic Current => _type;
    public static bool IsActive => _type != CurrentCinematic.None;

    public static void Play(CurrentCinematic cinematic)
    {
        _type = cinematic;
        _current = cinematic switch
        {
            CurrentCinematic.NewGameIntro => new NewGameIntroCin(),
            // Add others here
            _ => null
        };

        _current?.Start();
    }

    public static void Update(GameTime gameTime)
    {
        if (_current == null) return;

        _current.Update(gameTime);

        if (_current.IsFinished)
        {
            SceneState state = _current.OnFinishState;
            _current = null;
            _type = CurrentCinematic.None;
            SceneManager.SetState(state);
        }
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        _current?.Draw(spriteBatch);
    }
}
public enum CurrentCinematic
{
    None,
    NewGameIntro,
}