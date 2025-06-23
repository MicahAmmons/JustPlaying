using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using System;
using System.Collections.Generic;


namespace PlayingAround.Entities.Monster.CombatMonsters
{
     public class CombatMonster: ICombatant
    {

        public string UniqueId { get; set; }
        public ElementType ElementType { get; set; }
        public string Name { get; set; }
        public string NamePlusLevel { get; set; }




        public BaseCombatStats BaseStats { get; set; }
        public CurrentCombatStats CurrentStats { get; set; }
        public DrawSpecificStats DrawSpecifics { get; set; }
        public List<Aspect> Aspects { get; set; } = new List<Aspect>();
        public Dictionary<AnimationState, Animation> Animation { get; set; } = new Dictionary<AnimationState, Animation>();
        public List<SingleAttack> Attacks { get; set; } = new List<SingleAttack> { };
        public CombatMonsterType CombatantIs { get; set; }
        public bool isDead { get; set; } = false;
        public Texture2D Icon { get; set; }
        public Texture2D SpriteSheet { get; set; }
        public CombatMonsterType Is {  get; set; }
        public AnimationController AnimationController { get; set; } = new AnimationController();
        public Direction FacingDirection { get; set; } = Direction.Right;
        public AnimationState CurrentAnimationState { get; set; }
        public List<TileCell> MoveableCells {  get; set; } = new List<TileCell> { };
        public Vector2? MoveTarget {  get; set; }

        public CombatMonster (CombatMonsterData data, ElementType element = ElementType.None)
        {
            ElementType = element == ElementType.None ? data.DefaultElementType : element;
            BaseStats = new BaseCombatStats()
            {
                MP = data.BaseStats.MP,
                AP = data.BaseStats.AP,
                Health = data.BaseStats.Health,
                Initiative = data.BaseStats.Initiative,
                Resistances = ResistanceManager.GetResistances(ElementType),
                DecideWhichAttack = new Queue<ChooseWhichMonsterAttack>(),
                ActionOrder = new Queue<MonsterActionOrder>(),
            };
            CurrentStats = new CurrentCombatStats()
            {
                Health = BaseStats.Health,
                AP = BaseStats.AP,
                MP = BaseStats.MP,
                Resistances = ResistanceManager.GetResistances(ElementType)
            };
            foreach (var action in data.ActionOrder)
            {
                CurrentStats.ActionOrder.Enqueue(action);
                BaseStats.ActionOrder.Enqueue(action);
            }
            foreach (var choice in data.DecideWhichAttack)
            {
                CurrentStats.ChooseWhichAttack.Enqueue(choice);
                BaseStats.DecideWhichAttack.Enqueue(choice);
            }
            UniqueId = data.UniqueId;

            BaseStats.Resistances = ResistanceManager.GetResistances(ElementType);
            DrawSpecifics = data.DrawSpecifics;
            DrawSpecifics.AllowedToMove = true;
            Name = UniqueId;

            foreach (var kvp in data.AttackData)
            {
                Attacks.Add(AttackManager.GetAttack(kvp.Key, kvp.Value));
            }
            CombatantIs = CombatMonsterType.AI;
            Icon = AssetManager.GetTexture($"{UniqueId}Icon");
            //SpriteSheet = Icon;
            SpriteSheet = AssetManager.GetTexture("PlayerSS");
            Is = CombatMonsterType.AI;
            foreach (var kvp in data.AnimationData)
            {
                AnimationState state = kvp.Key;
                int row = kvp.Value[0];
                int frames = kvp.Value[1];
                int duration = kvp.Value[2];
                Animation[state] = new Animation(SpriteSheet, row, frames, duration);
            }
        }

        public CombatMonster()
        {

        }
        public void SetFacingDirection(Vector2 vec)
        {
            FacingDirection =vec.X <= 0 ? Direction.Right : Direction.Left;
        }
        public void SetCurrentAnimationState()
        {
            switch (DrawSpecifics.MovementPattern)
            {
                case MovementPatternType.Arc:
                    CurrentAnimationState = FacingDirection == Direction.Right
                      ? AnimationState.WalkRight
                      : AnimationState.WalkLeft;
                    break;
            }
        }
        public void SetCurrentAnimationStateToIdle()
        {
            CurrentAnimationState = FacingDirection == Direction.Right
             ? AnimationState.IdleRight
             : AnimationState.IdleLeft;
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play(CurrentAnimationState, Animation[CurrentAnimationState]);
        }
        public void UpdateMovement(GameTime gameTime)
        {
            Vector2 nextPoint = CurrentStats.MovePath[0];
            float speed = DrawSpecifics.MovementQuickness * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = nextPoint - CurrentStats.Pos;
            float distance = direction.Length();

            if (distance <= speed)
            {
                CurrentStats.Pos = nextPoint;
                CurrentStats.MovePath.RemoveAt(0);
                if (CurrentStats.MovePath.Count <= 0)
                {
                    SetCurrentAnimationStateToIdle();

                }
            }
            else
            {
                direction.Normalize();
                CurrentStats.Pos += direction * speed;
                SetFacingDirection(direction);
                SetCurrentAnimationState();

            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawTexture(spriteBatch);
        }
        public void DrawTexture(SpriteBatch spriteBatch)
        {
            Vector2 offset = TileManager.OffSetFromCenterOfDiamond(CurrentStats.Pos, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle dest = new Rectangle(
               (int)offset.X,
               (int)offset.Y,
               DrawSpecifics.Width,
               DrawSpecifics.Height
           );
            Rectangle source = AnimationController.GetCurrentFrame();
            spriteBatch.Draw(Icon, dest, source, DrawSpecifics.IsFlashingRed? Color.Red: Color.White);
        }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell)
        {
            Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle rect = new Rectangle((int)drawPoint.X, (int)drawPoint.Y - DrawSpecifics.Height / 2, DrawSpecifics.Width, DrawSpecifics.Height);
            spriteBatch.Draw(SpriteSheet, rect, AnimationController.GetCurrentFrame(), Color.White);
        }
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (MoveTarget != null)
            {
                List<Vector2> fullVectorPath = new List<Vector2>();
                Vector2 move = (Vector2)MoveTarget;
                TileCell startingCell = TileManager.GetCell(CurrentStats.Pos);
                List<TileCell> cellPath = CustomPathfinder.GetCellToCellPath(CurrentStats.Pos, move);
                foreach (var endPos in cellPath)
                {
                    List<Vector2> vectorRange = NPCMovement.GetMovementPatternVector2List(DrawSpecifics.MovementPattern, startingCell, endPos);
                    fullVectorPath.AddRange(vectorRange);
                    startingCell = endPos;

                }
                MoveTarget = null;
                CurrentStats.MovePath = fullVectorPath;
            }
        }
        public void Update(GameTime gameTime)
        {
            UpdateAnimation(gameTime);
            PopulateMovementPath(gameTime);
        }
    }

    public enum MonsterActionOrder
    {
        AttackClosestEnemy,
        MoveTowardsClosestEnemy,
        AttackSelf,
        Exist,
    }
    public enum ChooseWhichMonsterAttack
    {
        ShortestRange
    }
    public enum CombatMonsterType
    {
        Summoned,
        AI,
        Player
    }
}
