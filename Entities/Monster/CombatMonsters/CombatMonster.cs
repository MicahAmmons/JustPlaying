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
        public AnimationState CurrentAnimationState { get; set; } = AnimationState.IdleRight;
        public List<TileCell> MoveableCells { get; set; } = new List<TileCell> { };
        public Vector2? MoveTarget { get; set; }
        public List<TileCell> MoveTargetCellList { get; set; } = new List<TileCell>();
        public int PositionInOrder { get; set; }
        public Vector2? AnimationDrawPoint { get; set; } = null;

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
                ActionOrder = new List<AiAction>()
            };
            CurrentStats = new CurrentCombatStats()
            {
                Health = BaseStats.Health,
                AP = BaseStats.AP,
                MP = BaseStats.MP,
                Resistances = ResistanceManager.GetResistances(ElementType),
                AttackPath1 = new List<Vector2>(),
                AttackPath2 = new List<Vector2>(),
                Actions = new List<AiAction>()
            };

            foreach (var action in data.ActionOrder)
            {
                BaseStats.ActionOrder.Add(action);
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
            try { Icon = AssetManager.GetTexture($"{UniqueId}Icon"); } catch { Icon = AssetManager.GetTexture("OozeIcon"); }
            Is = CombatMonsterType.AI;
            foreach (var kvp in data.AnimationData)
            {
                    AnimationState state = kvp.Key;
                    AnimationData datas = kvp.Value;
                    Animation[state] = new Animation(datas.FrameCount, datas.FrameWidth, datas.FrameHeight, (int)datas.FrameDurationMs, datas.Row, datas.IsLooping, datas.SpriteSheetName, datas.EndOfCyclePause);
                }
        }

        public CombatMonster()
        {

        }
        public void SetFacingDirection(Vector2 direction)
        {
            if (direction != Vector2.Zero)
                direction.Normalize();

            if (direction.X > 0 && direction.Y < 0)
                FacingDirection = Direction.UpRight;
            else if (direction.X < 0 && direction.Y < 0)
                FacingDirection = Direction.UpLeft;
            else if (direction.X > 0 && direction.Y > 0)
                FacingDirection = Direction.DownRight;
            else
                FacingDirection = Direction.DownLeft;
        }
        public void SetCurrentAnimationState()
        {

        }
        public void SetCurrentAnimationStateToIdle()
        {
            if (FacingDirection == Direction.Right ||
                FacingDirection == Direction.UpRight ||
                FacingDirection == Direction.DownRight)
            {
                CurrentAnimationState = AnimationState.IdleRight;
            }
            else if (FacingDirection == Direction.Left ||
                     FacingDirection == Direction.UpLeft ||
                     FacingDirection == Direction.DownLeft)
            {
                CurrentAnimationState = AnimationState.IdleLeft;
            }
        }
        public void SetAnimationWalkState(Vector2 direction)
        {
            SetFacingDirection(direction);
            CurrentAnimationState = FacingDirection switch
            {
                Direction.UpRight => AnimationState.WalkUpRight,
                Direction.UpLeft => AnimationState.WalkUpLeft,
                Direction.DownRight => AnimationState.WalkDownRight,
                Direction.DownLeft => AnimationState.WalkDownLeft,
                _ => CurrentAnimationState
            };
        }
        public void UpdateAnimation(GameTime gameTime)
        {
            AnimationController.Play(CurrentAnimationState, Animation[CurrentAnimationState]);
        }
        public void UpdateMovement(GameTime gameTime)
        {
            if (CurrentStats.DestinationPoint == null) return;
            if (!AnimationController.IsFinished) return;

            Vector2 direction = (Vector2)CurrentStats.DestinationPoint - CurrentStats.Pos;

            CurrentStats.Pos = (Vector2)CurrentStats.DestinationPoint;
            AnimationDrawPoint = null;
            SetFacingDirection(direction);
            SetCurrentAnimationStateToIdle();

        }
        public void MovedOneCell()
        {
            // Logic to check for traps or damages or things that stop movement or damage per movement
            CurrentStats.MP -= 1;
            CurrentStats.MovePath.RemoveAt(0);
            AnimationDrawPoint = CurrentStats.Pos;
            if (CurrentStats.MovePath.Count <= 0)
            {
                AnimationDrawPoint = null;
                SetCurrentAnimationStateToIdle();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawSpecifics.VEManager.Draw(spriteBatch);
            DrawTexture(spriteBatch);
        }
        public void DrawTexture(SpriteBatch spriteBatch)
        {
            if (AnimationController.CurrentAnimation == null) return;
            Vector2 drawPoint = new Vector2(0, 0);
            if (AnimationDrawPoint != null)
            {
                drawPoint = (Vector2)AnimationDrawPoint;
            }
            else drawPoint = CurrentStats.Pos;
            int width = AnimationController.CurrentAnimation.Width;
            int height = AnimationController.CurrentAnimation.Height;
            var pos = TileManager.OffSetFromCenterOfDiamond(drawPoint, width, height);
            Rectangle dest = new Rectangle(
                (int)pos.X,
                (int)pos.Y,
                width,
                height
            );
            Rectangle source = AnimationController.GetCurrentFrame();
            Texture2D texture = AnimationController.CurrentAnimation.SpriteSheet;
            spriteBatch.Draw(texture, dest, source, DrawSpecifics.IsFlashingRed ? Color.Red : Color.White);

        }
        public void DrawEntityCellPreview(SpriteBatch spriteBatch, TileCell cell)
        {
            Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint, DrawSpecifics.Width, DrawSpecifics.Height);
            Rectangle rect = new Rectangle((int)drawPoint.X, (int)drawPoint.Y - DrawSpecifics.Height / 2, DrawSpecifics.Width, DrawSpecifics.Height);
            spriteBatch.Draw(SpriteSheet, rect, AnimationController.GetCurrentFrame(), Color.White);
        }
        public void PopulateMovementPath(GameTime gameTime)
        {
            if (CurrentStats.MovePath.Count <= 0) return;
            if (!AnimationController.IsFinished) return;
            AnimationDrawPoint = CurrentStats.Pos;
            CurrentStats.DestinationPoint = CurrentStats.MovePath[0].CenterPoint;
            CurrentStats.MovePath.RemoveAt(0);
            Vector2 direction = (Vector2)CurrentStats.DestinationPoint - CurrentStats.Pos;
            SetAnimationWalkState(direction);
        }
        public void Update(GameTime gameTime, float delta)
        {
            UpdateAnimation(gameTime);
            PopulateMovementPath(gameTime);
            UpdateMonsterTakingDamage(gameTime);
            DrawSpecifics.VEManager.Update(delta);
            UpdateMovement(gameTime);
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
                    CurrentStats.Actions.Clear();
                    if (Is == CombatMonsterType.AI)
                    {
                        foreach (var str in BaseStats.ActionOrder)
                        {
                            CurrentStats.Actions.Add(str);
                        }
                    }
                    break;
                case CombatMonsterType.Summoned:
                    break;
            }

        }
        public AITurnState? DecideAction()
        {
            while (CurrentStats.Actions.Count > 0)
            {
                AiAction action = CurrentStats.Actions[0];
                AiActionType type = action.Action;

                var executor = ActionLibrary.Executors[type];
                bool success = executor(action, this);
                if (success)
                {
                    SpendActionPoint();
                    var turnState = ActionLibrary.ActionStates[type];
                    return turnState;
                }
                CurrentStats.Actions.RemoveAt(0);
            }
            return null;
        }
        public void SpendActionPoint()
        {
            CurrentStats.AP -= 1;
        }
        public bool GetMovementCellPathToClosestEnemy(float mp)
        {
            if (mp > 0)
            {
                TileCell currentCell = TileManager.GetCell(CurrentStats.Pos);

                List<TileCell> playerControlledCells = GetCombatMapHelper("PlayerControlled");

                // If already adjacent, return current position
                if (TileManager.IsNeighbor(playerControlledCells, currentCell))
                    return false;

                //This uses GetPath which excludes !walkable 
                List<TileCell> listOfCellsPathToTarget = GridMovement.BestPathToClosestCell(currentCell, playerControlledCells, (int)CurrentStats.MP);
                if (listOfCellsPathToTarget.Count <= 0) return false;
                CurrentStats.MovePath = listOfCellsPathToTarget;
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
        public bool AttackClosestEnemy(AttackName attName)
        {
            foreach (var att in Attacks)
            {
                if (att.Name == attName)
                {
                    CurrentStats.Attack = att;
                    break;
                }
            }

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
            if (combatant == null || cell == null) return false;
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
        public bool MoveUpToFullMP(ActionTarget target)
        {
            switch (target)
            {
                case ActionTarget.ClosestEnemy:
                    return GetMovementCellPathToClosestEnemy(CurrentStats.MP);
            }
            return false;
        }
        public void UpdateCombatPosition(int pos)
        {
            PositionInOrder = pos;
        }
    }
    public enum CombatMonsterType
    {
        Summoned,
        AI,
        Player,
        Self
    }
}
