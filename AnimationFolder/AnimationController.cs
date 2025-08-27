using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.AnimationFolder
{
    public class AnimationManager
    {
        public Dictionary<AnimationState, List<AnimationController>> ControllerList = new Dictionary<AnimationState, List<AnimationController>>();
       
        private AnimationState _currentAnimationState;
        public List<AnimationController> CurrentControllers => ControllerList[_currentAnimationState];
        public bool IsFinished = false;
        public AnimationManager(AnimationData data)
        {
            _currentAnimationState = AnimationState.Idle;
            foreach (var kvp in data.Animations)
            {
                AnimationState state = kvp.Key;
                List<SpecificAnimationData> anis = kvp.Value;
                List<Animation> animations = new List<Animation>();
                foreach (var ani in anis)
                {
                    animations.Add(new Animation(ani));
                }
                List<AnimationController> controllers = new List<AnimationController>();    
                foreach (var ani in animations)
                {
                    controllers.Add(new AnimationController(ani));
                }
                ControllerList[state] = controllers;
            }
        }
        public AnimationManager(AnimationManager man)
        {
            foreach (var kvp in man.ControllerList)
            {
                AnimationState state = kvp.Key;
                List<AnimationController> animConts = new List<AnimationController> ();
                foreach (var cont in man.ControllerList[state])
                {

                   animConts.Add(new AnimationController(cont.Animation));
                }
                ControllerList[state] = animConts;
            }
        }
        public void Update(GameTime gameTime, AnimationState state)
        {
            UpdateState(state);
            UpdateAnimations(gameTime);
            IsFinished = UpdateIsFinished();
        }
        public bool UpdateIsFinished()
        {
            foreach (var contr in ControllerList[_currentAnimationState])
            { 
             if (!contr.IsFinished) return false;
            }
            return true;
        }
        public void UpdateAnimations(GameTime gameTime)
        {
            foreach (var control in ControllerList[_currentAnimationState])
            {
                control.Update(gameTime);
            }
        }
        public void UpdateState(AnimationState state)
        {
            if (_currentAnimationState != state)
            {
                ResetStates();
                _currentAnimationState = state;
            }
        }

        public void ResetStates()
        {
            foreach (var kvp in ControllerList)
            {
                AnimationState state = kvp.Key;
                List<AnimationController> controllerList = kvp.Value;
                foreach (var contr in controllerList)
                {
                    contr.Reset();
                }
            }
        }
    }
    public class AnimationController
    {
        private Animation _animation;
        public Animation Animation => _animation;
       
        private int _currentFrameIndex = 0;
        private float _frameTimer = 0f;
        public float FadeMultiplier = 0f;
        public bool IsFinished = false;
        public AnimationController(Animation ani)
        {
            _animation = ani;
        }
        public void Reset()
        {
            _currentFrameIndex = 0;
            _frameTimer = 0f;
            IsFinished = false;
            if (_animation.IsLooping) IsFinished = true;
        }

        public void Update(GameTime gameTime)
        {

            if (_animation == null || _animation.FrameCount < 1)
                return;
            if (_animation.IsLooping) IsFinished = true;

            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            FadeMultiplier = _frameTimer / _animation.FrameDuration;
            //if its time to go to next frame
            if (_frameTimer >= _animation.FrameDuration)
            {
                _frameTimer -= _animation.FrameDuration;
                _currentFrameIndex++;

                if (_currentFrameIndex >= _animation.FrameCount)
                {
                    if (_animation.IsLooping)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                        _currentFrameIndex--;
                        IsFinished = true;
                }
            }
            FadeMultiplier = MathHelper.Clamp(
       _animation.FrameDuration <= 0f ? 1f : _frameTimer / _animation.FrameDuration,
       0f, 1f
   );
        }

        public Rectangle GetCurrentFrame()
        {
            if (_animation == null)
                return Rectangle.Empty;

            return _animation.GetFrame(_currentFrameIndex);
        }

        public Rectangle GetNextFrame()
        {
            if (_animation == null || _animation.FrameCount <= 0)
                return Rectangle.Empty;

            int next = _currentFrameIndex + 1;
            if (next >= _animation.FrameCount)
                next = 0;
            return _animation.GetFrame(next);
        }
        public int GetCurrentFrameIndex()
        {
            return _currentFrameIndex;
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
    WalkUp,
    WalkDown,
    Idle,
    AttackUp,
    AttackDown,
}