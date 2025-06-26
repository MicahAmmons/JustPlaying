using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.Game.Map;
using PlayingAround.Game.Pathfinding;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.ActionLibrary;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Resistances;
using PlayingAround.Managers.Tiles;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using static CombatStateMachine;


namespace PlayingAround.Entities.Monster.CombatMonsters
{
    public class CombatMonster : ICombatant
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
        public CombatMonsterType Is { get; set; }
        public AnimationController AnimationController { get; set; } = new AnimationController();
        public Direction FacingDirection { get; set; } = Direction.Right;
        public AnimationState CurrentAnimationState { get; set; }
        public List<TileCell> MoveableCells { get; set; } = new List<TileCell> { };
        public Vector2? MoveTarget { get; set; }
        public List<TileCell> MoveTargetCellList { get; set; } = new List<TileCell>();
        public Dictionary<MonsterActionOrder, Func<bool>> ActionExecutors { get; private set; } = new();
        public Dictionary<MonsterActionOrder, AITurnState> ActionStates { get; private set; } = new();

        public CombatMonster(CombatMonsterData data, ElementType element = ElementType.None)
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
                Resistances = ResistanceManager.GetResistances(ElementType),
                AttackPath1 = new List<Vector2>(),
                AttackPath2 = new List<Vector2>(),
            };

            foreach (var action in data.ActionOrder)
            {
                CurrentStats.ActionOrder.Enqueue(action);
                BaseStats.ActionOrder.Enqueue(action);
                ActionExecutors[action] = () => ActionLibrary.Executors[action](this);
                ActionStates[action] = ActionLibrary.StateMap[action];
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
            FacingDirection = vec.X <= 0 ? Direction.Right : Direction.Left;
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
            DrawSpecifics.VEManager.Draw(spriteBatch);
            DrawTexture(spriteBatch);
            DrawCellHighlight(spriteBatch);
        }
        public void DrawCellHighlight(SpriteBatch spriteBatch)
        {
               if (DrawSpecifics.DrawCellHightlight)
            {
                DrawSpecifics.DrawCellHightlight = false;
                int shrink = DrawSpecifics.shrink;
                DrawSpecifics.shrink = 0;
                Color col = DrawSpecifics.HighlightCol;
                DrawSpecifics.HighlightCol = ColorPalette.DarkColor;
                Vector2 coords = TileManager.OffSetFromCenterOfDiamond(CurrentStats.Pos);
                Rectangle rect = new Rectangle(
                    (int)coords.X + shrink - MapTile.TileWidth / 2,
                    (int)coords.Y + shrink,
                    128 - shrink * 2,
                    64 - shrink * 2
                );
                Texture2D text = AssetManager.GetTexture("CellDiamond");
                spriteBatch.Draw(text, rect, col);
            }
            
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
            spriteBatch.Draw(SpriteSheet, dest, source, DrawSpecifics.IsFlashingRed ? Color.Red : Color.White);
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
                List<TileCell> list = CustomPathfinder.GetCellToCellPath(CurrentStats.Pos, (Vector2)MoveTarget);
                MoveTarget = null;
                //if (list[0] == TileManager.GetCell(CurrentStats.Pos)) list.RemoveAt(0);
                MoveTargetCellList = list;
            }
            if (MoveTargetCellList.Count > 0)
            {
                List<Vector2> fullVectorPath = new List<Vector2>();
                TileCell startingCell = MoveTargetCellList[0];
                foreach (var endPos in MoveTargetCellList)
                {
                    List<Vector2> vectorRange = NPCMovement.GetMovementPatternVector2List(DrawSpecifics.MovementPattern, startingCell, endPos);
                    fullVectorPath.AddRange(vectorRange);
                    startingCell = endPos;

                }
                MoveTargetCellList.Clear();
                CurrentStats.MovePath = fullVectorPath;
            }
        }
        public void Update(GameTime gameTime, float delta)
        {
            UpdateAnimation(gameTime);
            PopulateMovementPath(gameTime);
            UpdateMonsterTakingDamage(gameTime);
            DrawSpecifics.VEManager.Update(delta);
        }
        public void UpdateMonsterTakingDamage(GameTime gameTime)
        {
                if (DrawSpecifics.IsFlashingRed)
                {
                    DrawSpecifics.DamageFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds; ;
                    if (DrawSpecifics.DamageFlashTimer <= 0f)
                    {
                        DrawSpecifics.IsFlashingRed = false;
                    }
                }
            

        }
        public void UpdateTopOfActionStats()
        {
            switch (Is)
            {
                case CombatMonsterType.AI:
                    CurrentStats.MP = BaseStats.MP;
                    CurrentStats.ChooseWhichAttack.Clear();
                    CurrentStats.ActionOrder.Clear();
                    if (Is == CombatMonsterType.AI)
                    {
                        foreach (var str in BaseStats.DecideWhichAttack)
                        {
                            CurrentStats.ChooseWhichAttack.Enqueue(str);
                        }
                        foreach (var str in BaseStats.ActionOrder)
                        {
                            CurrentStats.ActionOrder.Enqueue(str);
                        }
                    }
                    break;
                case CombatMonsterType.Summoned:
                    CurrentStats.MP = BaseStats.MP;
                    break;
            }

        }
        public AITurnState? DecideAction()
        {
            while (CurrentStats.ActionOrder.Count > 0)
            {
                var action = CurrentStats.ActionOrder.Dequeue();
                var executor = ActionExecutors[action];
                if (executor())
                {
                    SpendActionPoint();
                    var turnState = ActionStates[action];
                    return turnState;
                }
            }
            return null;
        }
        public void SpendActionPoint()
        {
            CurrentStats.AP -= 1;
        }
        public bool GetMovementCellPathToClosestEnemy()
        {
            if (CurrentStats.MP > 0)
            {
                TileCell currentCell = TileManager.GetCell(CurrentStats.Pos);

                List<TileCell> playerControlledCells = GetCombatMapHelper("PlayerControlled");

                // If already adjacent, return current position
                if (TileManager.IsNeighbor(playerControlledCells, currentCell))
                    return false;

                List<TileCell> listOfCellsPathToTarget = GridMovement.PathToClosestCell(currentCell, playerControlledCells, (int)CurrentStats.MP);
                if (listOfCellsPathToTarget.Count <= 0) return false;
                MoveTargetCellList = listOfCellsPathToTarget;
            }
            else return false;
            return true;
        }
        private List<TileCell> GetCombatMapHelper(string combatants)
        {
            List<TileCell> list = new();
            switch (combatants)
            {
                case "Player":

                    break;
                case "Summons":

                    break;
                case "PlayerControlled":
                    list = CombatGuard.CurrentCombat.PlayerControlledMonsterMap
                     .Select(pair => pair.Value)
                     .Where(cell => cell != null)
                     .ToList();
                    break;
                case "AI":

                    break;
            }
            return list;
        }
        public bool AttackClosestEnemy()
        {
            List<SingleAttack> attks = new List<SingleAttack>();
            // Remove any attacks that don't have any targets within range
            foreach (var att in Attacks)
            {
                if (att.ActiveTargetMap.Count > 0) attks.Add(att);
            }
            if (attks.Count == 0) return false;

            // Sort the list by range distance
            attks = attks.OrderBy(att => att.Range).ToList();
            CurrentStats.Attack = attks[0];

            //Find the closest Target
            int dist = int.MaxValue;
            ICombatant combatant = null;
            TileCell cell = null;
            foreach (var kvp in CurrentStats.Attack.ActiveTargetMap)
            {
                ICombatant comb = kvp.Key;
                TileCell cells = kvp.Value;
                int distance = TileManager.CheckManhattanDistance(TileManager.GetCell(CurrentStats.Pos), cells);
                if (distance < dist)
                {
                    dist = distance;
                    combatant = comb;
                    cell = cells;
                }
            }
            SetCurrentEffected(combatant, cell);
            SetCombatantAttackPathingInformation();
            return true;
        }
        public void SetCurrentEffected(ICombatant combatant, TileCell cell)
        {
            CurrentStats.AttackEffectedCombatants = combatant;
            CurrentStats.AttackEffectedCells = cell;
        }
        public void SetCombatantAttackPathingInformation()
        {
            SingleAttack att = CurrentStats.Attack;
            ICombatant target = CurrentStats.AttackEffectedCombatants;
            TileCell cell = CurrentStats.AttackEffectedCells;

            List<Vector2> fullPath = NPCMovement.GetMovementPatternVector2List(DrawSpecifics.MovementPattern, TileManager.GetCell(CurrentStats.Pos), cell);
            var paths = GridMovement.SplitAttackPath(fullPath);
            if (att.Name == AttackName.Slam)
            {
                CurrentStats.AttackPath1 = paths.Item1;
                CurrentStats.AttackPath2 = paths.Item2;
            }
        }
        public void PerformAttack()
        {
            if (CurrentStats.Attack.IsFinished) return;
            CurrentStats.Attack.IsFinished = true;
            AttackManager.PerformAttack(CurrentStats.Attack, CurrentStats.AttackEffectedCombatants);

        }
        public bool IsAttackComplete()
        {
            if (CurrentStats.Attack.Visual != null && !CurrentStats.Attack.Visual.IsFinished) return false;
            return (CurrentStats.AttackPath1.Count == 0 && CurrentStats.AttackPath2.Count == 0 && CurrentStats.MovePath.Count == 0 && CurrentStats.Attack.IsFinished);
          
        }
        public void ApplyAspect(string aspect, ElementType elementDamage)
        {
            Aspect asp = AspectManager.GetAspect(aspect, elementDamage);
            Aspects.Add(asp);
        }
        public void ApplyDamage(float damage, ElementType elementDamage)
        {
            int finalDamage = (int)MathF.Round(CurrentStats.Resistances[elementDamage] * damage);
            CurrentStats.Health -= finalDamage;
            CreateNumericalDamageVE(finalDamage, elementDamage);
           
            DrawSpecifics.IsFlashingRed = true;
            DrawSpecifics.DamageFlashTimer = 0.5f;
        }
        public void CreateNumericalDamageVE(int damage, ElementType elementDamage)
        {
            Vector2 perimeterPos = GetRandomPointOnPerimeter(CurrentStats.Pos, DrawSpecifics.Width, DrawSpecifics.Height);
            DrawSpecifics.VEManager.AddEffect(new Visuals.VisualEffect(perimeterPos, new Vector2(0, -1), 1)
            {
                Color = ColorPalette.GetElementColor(elementDamage),
                Text = $"{damage}",
            });
        }
        private static Vector2 GetRandomPointOnPerimeter(Vector2 bottomCenter, int width, int height)
        {
            float halfWidth = width / 2f;
            float topY = bottomCenter.Y - height;
            float leftX = bottomCenter.X - halfWidth;
            float rightX = bottomCenter.X + halfWidth;

            int side = RandomHut.rng.Next(4); // 0=top, 1=right, 2=bottom, 3=left
            switch (side)
            {
                case 0: // top
                    return new Vector2(leftX + RandomHut.rng.Next(width), topY);
                case 1: // right
                    return new Vector2(rightX, topY + RandomHut.rng.Next(height));
                case 2: // bottom
                    return new Vector2(leftX + RandomHut.rng.Next(width), bottomCenter.Y);
                case 3: // left
                default:
                    return new Vector2(leftX, topY + RandomHut.rng.Next(height));
            }
        }



        public void CreateNewAttackVisual()
        {
            SingleAttack att = CurrentStats.Attack;
            att.Visual = new VisualEffect(CurrentStats.Pos, CurrentStats.AttackEffectedCombatants.CurrentStats.Pos, att.Name, att.VisualVelocity);
        }
        public void ClearAttackCycle()
        {
            if (CurrentStats.Attack != null)
            {
                CurrentStats.Attack.IsFinished = false;
                CurrentStats.Attack.Visual = null;
                CurrentStats.Attack = null;
            }
            CurrentStats.AttackEffectedCells = null;
            CurrentStats.AttackEffectedCombatants = null;
            CurrentStats.AttackPath1.Clear();
            CurrentStats.AttackPath2.Clear();
            CurrentStats.CurrentSelectedSummon = null;

        }
        public void ResolveAspects(TickedTiming ticked)
        {

            if (Aspects.Count == 0) return;
            foreach (var aspect in Aspects)
            {
                if (aspect.WhenTicked != ticked) continue;
                    if (aspect.IsDamage)
                    {
                        ApplyDamage(aspect.Damage, aspect.DamageType);
                        aspect.Duration -= 1;
                    }
                if (aspect.Duration == 0) Aspects.Remove(aspect);
            }
        }
        public void ClearAllAspects()
        {
            Aspects.Clear();
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
        Player,
        Self
    }
}
