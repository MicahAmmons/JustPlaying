using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using System.Collections.Generic;

public class NewGameIntroCin : ICinematic
{
    private float _timer = 0f;
    private bool _isFinished = false;
    private SpriteFont _font;
    private Song _backgroundMusic;

    private readonly float _fadeDuration = 1.5f; // Seconds to fade in/out
    private readonly float _textDuration = 2.5f; // Seconds at full opacity

    private List<string> _messages = new()
    {
        "Welcome to the Chasm.",
        "This is the story.",
        "This is more information for the story.",
        "Ancient ruins lie in wait...",
        "Echoes whisper of battles past.",
        "Your journey begins here."
    };

    private int _currentMessageIndex = 0;
    private float _messageStartTime = 0f;

    public bool IsFinished => _isFinished;
    public SceneManager.SceneState OnFinishState => SceneManager.SceneState.Play;

    public SpriteFont font => throw new System.NotImplementedException();

    public void Start()
    {
        _font = AssetManager.GetFont("mainFont");
        _backgroundMusic = AssetManager.GetSong("newGameIntroCinBackground");
        MediaPlayer.Play(_backgroundMusic);
        MediaPlayer.IsRepeating = false;

        _timer = 0f;
        _currentMessageIndex = 0;
        _messageStartTime = 0f;
        _isFinished = false;
    }

    public void Update(GameTime gameTime)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_currentMessageIndex >= _messages.Count - 1)
            return; // Keep last message showing — do not increment anymore

        float localTime = _timer - _messageStartTime;
        float totalPhaseDuration = _fadeDuration * 2 + _textDuration;

        if (localTime >= totalPhaseDuration)
        {
            _currentMessageIndex++;
            _messageStartTime = _timer;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Fill background black
        spriteBatch.GraphicsDevice.Clear(Color.Black);

        if (_currentMessageIndex >= _messages.Count)
            return;

        string text = _messages[_currentMessageIndex];
        float localTime = _timer - _messageStartTime;
        float alpha = 0f;

        // Fade logic
        if (localTime < _fadeDuration) // Fade in
        {
            alpha = MathHelper.Clamp(localTime / _fadeDuration, 0f, 1f);
        }
        else if (localTime < _fadeDuration + _textDuration) // Hold full
        {
            alpha = 1f;
        }
        else if (_currentMessageIndex < _messages.Count - 1) // Fade out
        {
            float fadeOutTime = localTime - _fadeDuration - _textDuration;
            alpha = 1f - MathHelper.Clamp(fadeOutTime / _fadeDuration, 0f, 1f);
        }
        else
        {
            alpha = 1f; // Final message: stay fully visible
        }

        Color textColor = Color.White * alpha;

        Vector2 textSize = _font.MeasureString(text);
        Vector2 screenCenter = new Vector2(
            ViewportManager.ScreenWidth / 2f,
            ViewportManager.ScreenHeight / 2f
        );
        Vector2 textPos = screenCenter - textSize / 2f;

        spriteBatch.DrawString(_font, text, textPos, textColor);
    }
}
