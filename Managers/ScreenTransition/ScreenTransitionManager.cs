using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PlayingAround.Managers
{
    public static class ScreenTransitionManager
    {
        private static float _fadeAlpha = 0f;
        private static Texture2D _blackTexture;

        private static float _timer = 0f;
        private const float FadeOutDuration = 1f;
        private const float FadeInDuration = 1f;

        private static bool _isTransitioning = false;
        public static event Action OnFadeToBlackComplete;


        private enum TransitionPhase { None, FadeOut, FadeIn }
        private static TransitionPhase _phase = TransitionPhase.None;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _blackTexture = new Texture2D(graphicsDevice, 1, 1);
            _blackTexture.SetData(new[] { Color.Black });

            SceneManager.OnStateChanged += OnSceneStateChanged;
        }

        private static void OnSceneStateChanged(SceneManager.SceneState newState)
        {
            if (newState == SceneManager.SceneState.SceneTransition)
            {
                _isTransitioning = true;
                _phase = TransitionPhase.FadeOut;
                _timer = 0f;
                _fadeAlpha = 0f;
            }
        }

        public static void Update(GameTime gameTime)
        {
            if (!_isTransitioning) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += delta;

            switch (_phase)
            {
                case TransitionPhase.FadeOut:
                    _fadeAlpha = MathHelper.Clamp(_timer / FadeOutDuration, 0f, 1f);

                    if (_timer >= FadeOutDuration)
                    {
                        _timer = 0f;
                        _phase = TransitionPhase.FadeIn;
                            // ✅ This is the trigger moment
                        OnFadeToBlackComplete?.Invoke();
                        

                        // This is where you'd handle the teleport or scene change logic
                        // e.g., TileManager.TeleportPlayer(...);
                    }
                    break;


                case TransitionPhase.FadeIn:
                    _fadeAlpha = 1f - MathHelper.Clamp(_timer / FadeInDuration, 0f, 1f);

                    if (_timer >= FadeInDuration)
                    {
                        _phase = TransitionPhase.None;
                        _isTransitioning = false;
                        _fadeAlpha = 0f;
                        SceneManager.SetState(SceneManager.SceneState.Play); // Resume game
                    }
                    break;
            }
        }

        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_fadeAlpha <= 0f) return;

            spriteBatch.Draw(
                _blackTexture,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.Black * _fadeAlpha
            );
        }
    }
}
