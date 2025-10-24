using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
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
            UpdateAnimations(gameTime, state);
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
        public void UpdateAnimations(GameTime gameTime, AnimationState state)
        {
            var finalState = state;

            ChangeToWalkIfNonSpecificWalking(ref finalState);

            HandleStartWalkTransitionAnimation(ref finalState);

            HandleEndWalkTransitionAnimation(ref finalState);

            if (state == AnimationState.Idle && state != _currentAnimationState)
            {
                ResetStates();
            }

            _currentAnimationState = finalState;

            foreach (var control in ControllerList[_currentAnimationState])
            {
                control.Update(gameTime);
            }

        }
        private void HandleEndWalkTransitionAnimation(ref AnimationState finalState)
        {
            if (_currentAnimationState == AnimationState.Idle && ControllerList.ContainsKey(AnimationState.EndWalkTrans))
            {
                foreach (var contr in ControllerList[AnimationState.EndWalkTrans])
                {
                    if (!contr.IsFinished) finalState = AnimationState.EndWalkTrans;break;
                }
            }
        }
        private void HandleStartWalkTransitionAnimation(ref AnimationState finalState)
        {
            if (IsCurrentlyAWalkingAnimation(finalState) && ControllerList.ContainsKey(AnimationState.StartWalkTrans))
            {
                foreach (var contr in ControllerList[AnimationState.StartWalkTrans])
                {
                    if (!contr.IsFinished) finalState = AnimationState.StartWalkTrans; break;
                }
            }
        }
        private void ChangeToWalkIfNonSpecificWalking(ref AnimationState finalState)
        {
            if (!ControllerList.ContainsKey(finalState) && IsCurrentlyAWalkingAnimation(finalState))
            {
                finalState = AnimationState.Walk;
            }
        }
        public bool IsCurrentlyAWalkingAnimation(AnimationState state) => state is AnimationState.Walk or AnimationState.WalkUp or AnimationState.WalkDown;
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
        internal void QuicknessOverride(float movementQuicknessOverride)
        {
            foreach (var contr in ControllerList)
            {
                AnimationState state = contr.Key;
                List<AnimationController> contrs = contr.Value;
                foreach (var ani in contrs)
                {
                    ani.FrameDurationOverride(movementQuicknessOverride);
                }
            }
        }

        public bool IsTransitioningAnimations()
        {
            if (ControllerList.ContainsKey(AnimationState.StartWalkTrans))
            {
                foreach (var contr in ControllerList[AnimationState.StartWalkTrans])
                {
                    if (!contr.IsFinished) return true;
                }
            }
            return false;
        }
    }
    public class AnimationController
    {
        private Animation _animation;
        public Animation Animation => _animation;
       
        private int _currentFrameIndex = 0;
        private float _frameTimer = 0f;
        private int _direction = 1;
        private float _frameDurationOverride = 0;

        public bool IsStartingPause = true;
        public bool IsFinished = false;
        public float FadeMultiplier = 0f;
        public AnimationController(Animation ani)
        {
            _animation = ani;
            Reset();
        }
        public void Reset()
        {
            _currentFrameIndex = 0;
            _frameTimer = 0f;
            _direction = 1;
            IsStartingPause = true;
            _animation.ResetOverridePath();

            IsFinished = _animation.IsIndefinite;
        }

        public void Update(GameTime gameTime)
        {

            if (_animation == null || _animation.FrameCount < 1)
                return;
            var frameDur = _animation.FrameDuration;
            if (_frameDurationOverride > 0f)
            {
                frameDur = _frameDurationOverride;
            }
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameTimer += delta;

            if (_frameTimer < frameDur) return;

            _frameTimer -= frameDur;

            HandleTravelMovementFrameStep();

            if (_animation.IsLooping)
            {
                StepLooping(delta);
            }
            else
            {
                StepNormalSingle(delta);
            }
        }

        private void HandleTravelMovementFrameStep()
        {
            var path = _animation.OverrideTravelPath;
            if (path == null || path.Count == 0) return;

            _animation.DrawPointOverride = path[0];

            path.RemoveAt(0);

            if (path.Count == 0)
                IsFinished = true;
        }
        private void StepLooping(float delta)
        { 
            if (IsFinished && !_animation.IsIndefinite) return;

            _currentFrameIndex++;
            if (_currentFrameIndex >= _animation.FrameCount)
            {
                 if (InPause()) return;

                _currentFrameIndex = 0;
                if (_animation.HasStartingPause & _animation.IsIndefinite) { Reset(); }
            }

        }
        private void StepNormalSingle(float delta)
        {
            if (IsFinished) return;

            _currentFrameIndex = Math.Min(_currentFrameIndex + 1, _animation.FrameCount - 1);
            if (_currentFrameIndex >= _animation.FrameCount - 1)
            {
                if (InPause()) return;
                IsFinished = true;
            }
        }
        private bool InPause()
        {
            if (_animation.EndCyclePause <= 0) return false;

            if (_currentFrameIndex < _animation.FrameCount + _animation.EndCyclePause)
            {
                return true;
            }

            return false;
        }
        public Rectangle GetCurrentFrame()
        {
            if (_animation == null)
                return Rectangle.Empty;

            return _animation.GetFrame(_currentFrameIndex);
        }

        public Rectangle GetNextFrame()
        {
            int idx = GetNextFrameIndex();
            if (idx < 0) return Rectangle.Empty;
            return _animation.GetFrame(idx);
        }
        public int GetNextFrameIndex()
        {
            if (_animation == null || _animation.FrameCount <= 0)
                return -1; 
            int count = _animation.FrameCount;

            if (_animation.IsLooping)
            {
                int next = _currentFrameIndex + 1;
                if (next >= count) next = 0;
                return next;
            }
            if (_animation.PingPong)
            {
                int next = _currentFrameIndex + _direction;

                // bounce at ends (handles 1 or 2 frames too)
                if (next >= count) next = Math.Max(0, count - 2);
                if (next < 0) next = Math.Min(1, count - 1);

                return Math.Clamp(next, 0, count - 1);
            }
            return Math.Min(count - 1, _currentFrameIndex + 1);
        }
        public int GetCurrentFrameIndex()
        {

            return _currentFrameIndex;
        }
        internal void FrameDurationOverride(float movementQuicknessOverride)
        {
            _frameDurationOverride = movementQuicknessOverride;
        }

        public float GetRemainingTime()
        {
            float remaining = _animation.FrameDuration - _frameTimer;
            return Math.Max(0f, remaining);
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
    StartWalkTrans,
    EndWalkTrans,
    Idle,
    AttackUp,
    AttackDown,
    FX,
    Walk,
    Attack,
}