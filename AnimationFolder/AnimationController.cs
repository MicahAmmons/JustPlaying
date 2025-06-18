using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public class AnimationController
    {
        private Animation _currentAnimation;
        private int _currentFrameIndex = 0;
        private float _frameTimer = 0f;
        private AnimationState _currentAnimationState;

        public Animation CurrentAnimation => _currentAnimation;

        public void Play(AnimationState state, Animation newAnimation)
        {
            if (_currentAnimationState == state)
                return;

            _currentAnimation = newAnimation;
            _currentFrameIndex = 0;
            _frameTimer = 0f;
            _currentAnimationState = state;
        }

        public void Update(GameTime gameTime)
        {
            if (_currentAnimation == null || _currentAnimation.FrameCount <= 1)
                return;

            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_frameTimer >= _currentAnimation.FrameDuration)
            {
                _frameTimer -= _currentAnimation.FrameDuration;
                _currentFrameIndex++;
                System.Diagnostics.Debug.WriteLine("Your message here");

                if (_currentFrameIndex >= _currentAnimation.FrameCount)
                {
                    if (_currentAnimation.IsLooping)
                        _currentFrameIndex = 0;
                    else
                        _currentFrameIndex = _currentAnimation.FrameCount - 1; // Stay on last frame
                }
            }
        }

        public Rectangle GetCurrentFrame()
        {
            if (_currentAnimation == null)
                return Rectangle.Empty;

            return _currentAnimation.GetFrame(_currentFrameIndex);
        }
    }

}
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}