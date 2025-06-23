using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Tiles;
using System.Collections.Generic;
using PlayingAround.Interfaces;

namespace PlayingAround.Managers.Movement
{
    public static class MovementManager
    {
       
        public static void Update(GameTime gameTime)
        {
            switch (SceneManager.CurrentState)
            {
                case SceneState.Play:
                    PlayMonsterManager.UpdateAllMovement(gameTime);
                    PlayerManager.UpdateAllMovement(gameTime);
                    break;
                case SceneState.Combat:
                    CombatMonsterManager.UpdateAllMovement(gameTime);
                    PlayerManager.UpdateAllMovement(gameTime);
                    break;
                case SceneState.Dialogue:
                    PlayMonsterManager.UpdateAllMovement(gameTime);
                    break;
            }
  
        }
      
    }
}

public enum AnimationState
{
    //WalkUp,
    //WalkDown,
    WalkLeft,
    WalkRight,
    IdleLeft,
    IdleRight,
    Idle,
    BouncingUp,
    BouncingDown,
}