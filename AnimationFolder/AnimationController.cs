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
        private float _endTimer = 0f;
        private AnimationState _currentAnimationState;
        public bool IsFinished = false;

        public Animation CurrentAnimation => _currentAnimation;

        public void Play(AnimationState state, Animation newAnimation)
        {
            if (_currentAnimationState == state && _currentAnimation != null)
                return;

            _currentAnimation = newAnimation;
            _currentFrameIndex = 0;
            _frameTimer = 0f;
            _currentAnimationState = state;
            _endTimer = 0f;
            IsFinished = false;
            if (_currentAnimation.IsLooping) IsFinished = true;
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

                if (_currentFrameIndex >= _currentAnimation.FrameCount)
                {
                    if (_currentAnimation.IsLooping)
                        _currentFrameIndex = 0;
                    else if (_endTimer < _currentAnimation.EndOfCyclePause)
                    {
                        _endTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        _currentFrameIndex = _currentAnimation.FrameCount - 1;
                        _frameTimer = _currentAnimation.FrameDuration;
                    }
                    else
                        IsFinished = true;
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
    UpLeft,
    UpRight,
    DownLeft,
    DownRight,
}
public enum AnimationState
{
    WalkUpRight,
    WalkUpLeft,
    WalkDownRight,
    WalkDownLeft,
    IdleLeft,
    IdleRight,
    Idle,
    SlamTopLeft,
    SlamTopRight,
    SlamBottomLeft,
    SlamBottomRight,
}