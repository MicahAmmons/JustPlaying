using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Movement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayingAround.Movement
{
    public class MovementController
    {
        public Vector2 DrawPoint { get; set; }
        public Vector2 CurrentPos { get; set; }
        public List<Vector2> VectorMovePath { get; set; } = new List<Vector2>();
        public List<TileCell> TileMovePath { get; set; } = new List<TileCell> { };
        public Vector2? DestinationPoint { get; private set; }
        public float MovementQuickness { get; set; }
        public Vector2? CachedPosition { get; private set; }
        public AnimationManager AnimationManager { get; set; }
        public AnimationState CurrentAnimationState { get; set; } = AnimationState.Idle;
        public Direction HorizontalFacingDirection { get; set; } = Direction.Right;
        public Direction VerticalFacingDirection { get; set; } = Direction.Up;
        public CombatMonsterType Is {  get; set; }

        public event Action StartedTileMove;
        public event Action FinishedTileMove;
        public event Action FinishedAllMovement;
        public event Action CurrentlyMoving;

        private bool _nextMoveReady = true;
        public bool IsMoving = false;
        public bool AllowedToBeDrawn = true;

        // player and combat monster constructor
        public MovementController(AnimationData data, CombatMonsterType type)
        {
            AnimationManager = new AnimationManager(data);
            Is = type;
        }
        // play monster constructor 
        public MovementController(AnimationManager man)
        {
            AnimationManager = new AnimationManager(man);

            Is = CombatMonsterType.PlayMonster;
        }



        public void Update(GameTime gameTime)
        {
            PopulateMovementPath(gameTime);
            UpdateMovement(gameTime);
            AnimationManager.Update(gameTime, CurrentAnimationState);
        }
        public void SetFacingDirection(Vector2 direction)
        {
            if (direction != Vector2.Zero)
                direction.Normalize();

            if (direction.X >= 0)
                HorizontalFacingDirection = Direction.Right;
            else
                HorizontalFacingDirection = Direction.Left;
            if (direction.Y <= 0)
                VerticalFacingDirection = Direction.Up;
            else 
                VerticalFacingDirection = Direction.Down;

        }

        public void SetAnimationWalkState(Vector2 direction)
        {
            SetFacingDirection(direction);
            CurrentAnimationState = VerticalFacingDirection switch
            {
                Direction.Up => AnimationState.WalkUp,
                Direction.Down => AnimationState.WalkDown,
                _ => CurrentAnimationState
            };
        }
        public void SetCurrentAnimationStateToIdle()
        {
                CurrentAnimationState = AnimationState.Idle;
        }
        public void SetAttackAnimation(Vector2 direction)
        {
            SetFacingDirection(direction);
            CurrentAnimationState = VerticalFacingDirection switch
            {
                Direction.Up => AnimationState.AttackUp,
                Direction.Down => AnimationState.AttackDown,
                _ => CurrentAnimationState
            };
        }


        private void OnStartMoveOneTile()
        {
            //This means movement has begun and it will not start the next one until _nextMOveReady is toggle
            _nextMoveReady = false;
            StartedTileMove?.Invoke();


        }
        public void ApproveNextTileStep()
        {
            _nextMoveReady = true;
            if (TileMovePath.Count <= 0 && VectorMovePath.Count <= 0)
            {
                FinishedAllMovement();
            }
        }
        public void SetMovePath(List<TileCell> movePath)
        {
            TileMovePath = new List<TileCell>(movePath);
        }
        public void SetDestinationPoint (Vector2 destinationPoint)
        {
            DestinationPoint = destinationPoint;
        }
        public void UpdateMovement(GameTime gameTime)
        {
            if (!_nextMoveReady)
            {

                if (AnimationManager.IsFinished)
                {
                    
                    DrawPoint = CurrentPos;
                    SetCurrentAnimationStateToIdle();
                    FinishedTileMove?.Invoke();
                }
                return;
            }
            if (TileMovePath.Count > 0)
            {
                OnStartMoveOneTile();
                // Consume next tile: keep DrawPoint at old tile, jump CurrentPos to new tile, start walk anim
                Vector2 oldPos = CurrentPos;
                CurrentPos = TileMovePath[0].CenterPoint;
                TileMovePath.RemoveAt(0);


                Vector2 direction = CurrentPos - oldPos; 
                SetAnimationWalkState(direction);

                DrawPoint = oldPos;

                return;      
            }
            if (VectorMovePath.Count > 0)
            {

                float speed = 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector2 nextPoint = VectorMovePath[0];
                Vector2 direction = nextPoint - CurrentPos;
                float distance = direction.Length();

                if (distance <= speed)
                {
                    CurrentPos = nextPoint;
                    VectorMovePath.RemoveAt(0);
                    DrawPoint = CurrentPos;

                    if (VectorMovePath.Count == 0)
                        SetCurrentAnimationStateToIdle();
                }
                else
                {
                    direction.Normalize();
                    CurrentPos += direction * speed;
                    DrawPoint = CurrentPos;
                    SetAnimationWalkState(direction);
                }
            }
        }
        public void PopulateMovementPath(GameTime gameTime)
        {
           switch (Is)
            {
                case CombatMonsterType.Player:
                    switch (SceneManager.CurrentState)
                    {
                        case SceneState.Play:
                            PopulatePlayerOutOfCombatMovementPath(); break;
                            case SceneState.Combat:
                            PopulateInCombatTileMovementPath(); break;
                    }
                    break;
                case CombatMonsterType.AI:
                    PopulateInCombatTileMovementPath();
                    break;
                case CombatMonsterType.Summoned:
                    PopulateInCombatTileMovementPath();
                    break;
                case CombatMonsterType.PlayMonster:
                    PopulateInCombatTileMovementPath();
                    break;
            }

        }
        public void PopulateInCombatTileMovementPath()
        {
                if (DestinationPoint != null)
                {
                CurrentlyMoving();
                List<TileCell> cells = GridMovement.GetCellToCellPath(CurrentPos, (Vector2)DestinationPoint);
                    List<TileCell> cellsToRemove = new List<TileCell>();
                    foreach (var cell in cells)
                    {
                        if (cell.CenterPoint == CurrentPos)
                        {
                            cellsToRemove.Add(cell);
                        }
                    }
                    foreach (var cell in cellsToRemove)
                    {
                        cells.Remove(cell);
                    }
                DestinationPoint = null;
                TileMovePath = cells;
            }
        }
        public void PopulatePlayerOutOfCombatMovementPath()
        {
                if (DestinationPoint != null)
                {
                CurrentlyMoving();
                List<Vector2> cellPath = GridMovement.BuildStraightLinePath(CurrentPos, (Vector2)DestinationPoint);
                    DestinationPoint = null;
                    if (cellPath == null || cellPath.Count == 0) // Abort early if path is empty
                        return;
                    if (cellPath[0] == CurrentPos) // Remove CurrentPos if it's the first point in the path
                        cellPath.RemoveAt(0);
                    if (cellPath.Count == 0) // All points were removed, nothing left to move to
                        return;
                    VectorMovePath = cellPath;
                }          
        }
        public void ClearMovementPath()
        {
            _nextMoveReady = true;     // ensure controller doesn't keep waiting
            TileMovePath.Clear();
            VectorMovePath.Clear();
            SetCurrentAnimationStateToIdle();
        }
        public bool FlipHorizontally(Direction dir)
        {
            return dir != HorizontalFacingDirection;
        }
        public void CachPos()
        {
            CachedPosition = CurrentPos;
            ToggleAllowedToBeDrawn(false);
        }
        public void ClearCachPos()
        {
            CachedPosition = null;
        }
        public void SetCurrentPos(Vector2 vec)
        {
            CurrentPos = vec;
            DrawPoint = vec;
        }
        public void ToggleAllowedToBeDrawn(bool allowed)
        {
            AllowedToBeDrawn = allowed;
        }
       
        internal bool FinishedTileMovement()
        {
            return TileMovePath.Count <= 0 && AnimationManager.IsFinished && _nextMoveReady;
        }
    }
}
