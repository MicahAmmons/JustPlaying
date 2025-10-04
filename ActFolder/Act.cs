using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.ButtonsFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Tiles;
using PlayingAround.Movement;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CombatStateMachine;

namespace PlayingAround.ActFolder
{
    public class ActManager
    {
        private  ICombatant _currentCombatant => CombatGuard.CurrentCombat.CurrentCombatant;
        private ActController _actController => _currentCombatant.ActController;
        private Dictionary<ICombatant, TileCell> AIMap => CombatGuard.CurrentCombat.AIControlledMonsterMap;
        private Dictionary<ICombatant, TileCell> PlayerMap => CombatGuard.CurrentCombat.PlayerControlledMonsterMap;
        public Act ConfirmedAct = null;
        public Act SelectedAct => _actController.SelectedAct;
        private Queue<Act> ActOrderTry = new Queue<Act>();


        public ActManager()
        {

        }
        public void Draw(SpriteBatch sb)
        {
            if (_actController == null) return;
            _actController.DrawButtons(sb);
        }
        public void Update()
        {
            if (_actController == null) return;
            _actController.Update(_currentCombatant.CurrentStats.AP, _currentCombatant.CurrentStats.MP);
            ConfirmEndTurnAct();
        }
        public CombatState? DecideNextAct()
        {
            PopulateActOrder();


            while (ActOrderTry.Count > 0)
            {
                var act = ActOrderTry.Dequeue();
                ActType type = act.ActType;
                CombatState state = StateHelper(type);

                if (act.TryAct(_currentCombatant, PlayerMap, AIMap)) 
                { 
                    ConfirmedAct = act;
                    _currentCombatant.BeginAct(act);
                    PopulateActOrder();
                    return state; 
                }

            }
            return null;
        }
        private void PopulateActOrder()
        {
            ActOrderTry.Clear();   
            foreach (var act in _actController.ActsOrder)
            {
                ActOrderTry.Enqueue(act);
            }
        }
        public CombatState StateHelper(ActType type)
        {
            switch (type)
            {
                case ActType.Attack: 
                    return CombatState.ExecutingAttack; 
                case ActType.Move: 
                    return CombatState.ExecutingMove;
                case ActType.EndTurn:
                    return CombatState.EndingTurn;
            }
            return CombatState.ExecutingAttack;
        }
        public void ResetSelectedAct()
        {   
            _actController.SelectedAct = null;
        }
        public void ResetActs()
        {
            ConfirmedAct = null;
            _actController.ResetController();
        }
        public void ConfirmMoveAct(List<TileCell> cells)
        {
            if (SelectedAct is MoveAct moveAct)
            {
                ResetSelectedAct();
                moveAct.ActMovementCellPath = cells;
                ConfirmedAct = moveAct;
            }
        }
        public void ConfirmSummonAct(TileCell cell)
        {
            if (SelectedAct is SummonAct summonAct)
            {
                ResetSelectedAct();
                summonAct.SummonedCell = cell;
                ConfirmedAct = summonAct;
            }
        }
        public void ConfirmEndTurnAct()
        {
            if (SelectedAct is EndturnAct end)
            {
                ResetSelectedAct();
                ConfirmedAct = end;
            }
        }
        public void ConfirmAttackAct(Dictionary<ICombatant, TileCell> dic)
        {
            if (SelectedAct is  AttackAct attackAct)
            {
                ResetSelectedAct();
                foreach (var kvp in dic)
                {
                    ICombatant combatant = kvp.Key;
                    TileCell cell = kvp.Value;
                    attackAct.EffectedTargets[combatant] = cell;
                }
                ConfirmedAct = attackAct;
            }
        }
    }

    public class ActController
    {
        public List<Act> ActsOrder {  get; set; } = new List<Act>();
        private readonly Dictionary<Button, Act> _buttonToAct = new();
        public Act SelectedAct { get; set; } = null;
        public ButtonManager ButtonManager { get; set; } = new ButtonManager();


        const int ButtonSize = 64;
        const int Buffer = 5;
        int startingX = (ViewportManager.ScreenWidth / 2);
        int y = (ViewportManager.ScreenHeight - ButtonSize - Buffer);

        //AI Controller
        public ActController(List<SpecificActData> data)
        {
            foreach (var act in data)
            {
                switch (act.Type)
                {
                    case ActType.Attack: ActsOrder.Add(new AttackAct(act)); break;
                    case ActType.Move: ActsOrder.Add(new MoveAct(act)); break;
                }
            }
            ActsOrder.Add(new EndturnAct());
            BuildButtonsAndMap();
            ButtonManager.ButtonSelected += b => SelectedAct = _buttonToAct[b];
            ButtonManager.ButtonDeselected += () =>
            {
                SelectedAct = null;
            };
        }
        // play controller
        public ActController()
        {
            ActsOrder.Add(new MoveAct()
            {
                Target = ActionTarget.Self,
                ActType = ActType.Move,
            });

                foreach (var sumMon in SummonedMonsterManager.UnlockedSummons)
            {
                string name = sumMon.Key;
                SummonedSavedStats stats = sumMon.Value;
                ActsOrder.Add(new SummonAct(name, stats));
            }
            ActsOrder.Add(new EndturnAct()
            {

            });
            BuildButtonsAndMap();
            ButtonManager.ButtonSelected += b => SelectedAct = _buttonToAct[b];
            ButtonManager.ButtonDeselected += () =>
            {
                SelectedAct = null;
            };

        }
        private void BuildButtonsAndMap()
        {
            for (int i = 0; i < ActsOrder.Count; i++)
            {
                int x = startingX - (i * (ButtonSize + Buffer));
                var rect = new Rectangle(x, y, ButtonSize, ButtonSize);
                var btn = new Button(rect);

                // decorate textures
                var act = ActsOrder[i];
                if (act.ActType == ActType.Summon) btn.Texture = act.Icon;
                if (act.ActType == ActType.Move) btn.Texture = AssetManager.GetTexture("MoveActIcon");

                // wire map + hand to ButtonManager
                _buttonToAct[btn] = act;
                ButtonManager.SetCurrentButtons(btn);
            }
        }
        public void Update(int ap, int mp)
        {
            ButtonManager.UpdateInput(_buttonToAct, ap, mp);
        }
        public void DrawButtons(SpriteBatch sb)
        {
            ButtonManager.Draw(sb); 
        }
        public void ResetController()
        {
            SelectedAct = null;
            ButtonManager.ResetButtons();
        }

    }
    public abstract class Act
    {
        public ActionTarget Target { get; set; }
        public ActType ActType { get; set; }
        public ICombatant _combatant { get; set; } = null;
        public Dictionary<ICombatant, TileCell> _playerMap { get; set; } = new Dictionary<ICombatant, TileCell>();
        public Dictionary<ICombatant, TileCell> _aiMap { get; set; } = new Dictionary<ICombatant, TileCell>();
        public Texture2D Icon { get; set; }
        public abstract void ClearActParams();
        public abstract CombatState ExecutingState();
        public abstract bool TryAct(ICombatant currentCombatant, Dictionary<ICombatant, TileCell> playerMap, Dictionary<ICombatant, TileCell> aIMap);
    }
    public class AttackAct : Act
    {
        public SingleAttack Attack {  get; set; }
        public Dictionary<ICombatant, TileCell> EffectedTargets { get; set; } = new Dictionary<ICombatant, TileCell>();
        public AttackAct(SpecificActData data)
        {
            Attack = new SingleAttack(data.AttackData);
            Target = data.ActionTarget;
            ActType = data.Type;
        }
        public override bool TryAct(ICombatant currentCombatant, Dictionary<ICombatant, TileCell> playerMap, Dictionary<ICombatant, TileCell> aIMap)
        {
            _combatant = currentCombatant;
            _playerMap = playerMap;
            _aiMap = aIMap;
            if (!MonsterHasEnoughAP(currentCombatant.CurrentStats.AP)) return false;
            if (!ValidTargets()) return false;
            return true;
        }
        private bool MonsterHasEnoughAP(int ap)
        {
            return ap > 0;
        }
        private bool ValidTargets()
        {
            var targetList = new Dictionary<ICombatant, TileCell>();

            foreach (var target in Attack.TargetType)
            {
                switch (target)
                {
                    case CombatMonsterType.Self:
                        targetList[_combatant] = TileManager.GetCell(_combatant.MovementController.CurrentPos);
                        break;
                    case CombatMonsterType.Player:
                    case CombatMonsterType.Summoned:
                        foreach (var kvp in _playerMap)
                        {
                            if (kvp.Key == _combatant) continue;
                            if (kvp.Key.Is == target)
                                targetList[kvp.Key] = kvp.Value;
                        }
                        break;

                    case CombatMonsterType.AI:
                        foreach (var kvp in _aiMap)
                        {
                            if (kvp.Key == _combatant) continue;
                            if (kvp.Key.Is == CombatMonsterType.AI)
                                targetList[kvp.Key] = kvp.Value;
                        }
                        break;
                }
            }
            if (targetList.Count == 0) return false;

            switch (Target)
            {
                case ActionTarget.ClosestEnemy:
                    if (!SetAttackClosestEnemyWithinRange(targetList)) return false;
                    break;
                    case ActionTarget.HighestHP:
                    if (!SetAttackHighestHP(targetList)) return false;
                    break;
            }
            return true;

        }
        public override CombatState ExecutingState()
        {
            return CombatState.ExecutingAttack;
        }
        private bool SetAttackHighestHP(Dictionary<ICombatant, TileCell> targetList)
        {
            int highestHP = 0;
            ICombatant comb = null;
            TileCell finalCell = null;
            foreach (var kvp in targetList)
            {
                ICombatant combatant = kvp.Key;
                TileCell cell = kvp.Value;
                int distance = TileManager.CheckManhattanDistance(TileManager.GetCell(_combatant.MovementController.CurrentPos), cell);
                if (distance > Attack.Range){continue;}
                if (combatant.CurrentStats.Health > highestHP) { highestHP = distance; comb = combatant; finalCell = cell; }
            }
            if (comb == null) return false;
            EffectedTargets[comb] = finalCell;

            return true;
        }
        private bool SetAttackClosestEnemyWithinRange(Dictionary<ICombatant, TileCell> targetList)
        {
            int dist = int.MaxValue;
            ICombatant combatant = null;
            TileCell cell = null;
            var range = Attack.Range;
            foreach (var kvp in targetList)
            {
                ICombatant comb = kvp.Key;
                TileCell cells = kvp.Value;
                int distance = TileManager.CheckManhattanDistance(TileManager.GetCell(_combatant.MovementController.CurrentPos), cells);
                if (distance < dist)
                {
                    dist = distance;
                    combatant = comb;
                    cell = cells;
                }
            }
            if (cell == null || dist > range) return false;
            
            EffectedTargets[combatant] = cell;
            return true;
        }
        public override void ClearActParams()
        {
            _playerMap.Clear();
            _aiMap.Clear();
            EffectedTargets.Clear();
            Attack.IsFinished = false;
        }
    }
    public class MoveAct : Act
    {
        public MovementAmount MovementAmount { get; set; }
        public List<TileCell> ActMovementCellPath { get; set; } = new List<TileCell>();
        public MoveAct(SpecificActData data)
        {
            MovementAmount = data.MovementAmount;
            Target = data.ActionTarget;
            ActType = data.Type;
        }
        public MoveAct() { }
        public override CombatState ExecutingState()
        {
            return CombatState.ExecutingMove;
        }
        public override bool TryAct(ICombatant currentCombatant, Dictionary<ICombatant, TileCell> playerMap, Dictionary<ICombatant, TileCell> aIMap)
        {
            int movementLeft = currentCombatant.CurrentStats.MP;
            if (movementLeft <= 0) return false;
            int maxMovementAllowed = movementLeft;
            switch (MovementAmount)
            {
                case MovementAmount.FullMP:
                    maxMovementAllowed = movementLeft;
                    break;
            }
            switch (Target)
            {
                case ActionTarget.ClosestEnemy:
                    if (!SetMovementPathToClosestEnemy(currentCombatant, maxMovementAllowed, playerMap)) return false ;
                    break;
                case ActionTarget.AwayFromEnemy:
                    if (!SetMovementPathAwayFromEnemy(currentCombatant, maxMovementAllowed, playerMap)) return false;
                    break;
                case ActionTarget.StayAtMaxAttackRange:
                    if (!SetMovementPathToMaxAttackRange(currentCombatant, maxMovementAllowed, playerMap)) return false;
                    break;
            }
            return true;
        }

        private bool SetMovementPathToMaxAttackRange(ICombatant currentCombatant, int maxMovementAllowed, Dictionary<ICombatant, TileCell> playerMap)
        {
            // 1) max attack range
            int maxDistanceFromEnemy = 0;
            foreach (var act in currentCombatant.ActController.ActsOrder)
                if (act is AttackAct att)
                    maxDistanceFromEnemy = Math.Max(maxDistanceFromEnemy, att.Attack.Range);

            TileCell currentCell = TileManager.GetCell(currentCombatant.MovementController.CurrentPos);
            if (currentCell == null) return false;


            // 2) reachable, legal cells
            var inRangeMovementCells = TileManager.GetFloodFillTileWithinRange(currentCell, maxMovementAllowed);

            if (inRangeMovementCells == null || inRangeMovementCells.Count == 0) return false;

            var validCells = new List<TileCell>(inRangeMovementCells.Count);
            foreach (var cell in inRangeMovementCells)
                if (cell != null && cell.IsWalkable && !cell.BlockedByCombatant)
                    validCells.Add(cell);
            //Take into consideration its own cell - potentially won't move
            validCells.Add(currentCell);
            if (validCells.Count == 0) return false;

            // 3) scoreboard: cell -> distance to closest enemy
            var scoreBoard = new Dictionary<TileCell, int>(validCells.Count);
            foreach (var cell in validCells)
            {
                int minDist = int.MaxValue;

                foreach (var enemyCell in playerMap.Values)
                {
                    if (enemyCell == null) continue;

                    int d = TileManager.CheckManhattanDistance(cell, enemyCell);
                    if (d < minDist) minDist = d;
                }
                scoreBoard[cell] = (minDist == int.MaxValue) ? int.MaxValue : (minDist);
            }
            if (scoreBoard.TryGetValue(currentCell, out int value) && value == maxDistanceFromEnemy)
            {
                return false;
            }

            // 4) best candidates near desired range
            var finalOptions = new Dictionary<TileCell, int>();
            var inRange = scoreBoard.Where(kv => kv.Value != int.MaxValue && kv.Value <= maxDistanceFromEnemy).ToList();
            if (inRange.Count > 0)
            {
                int best = inRange.Max(kv => kv.Value);
                foreach (var kv in inRange)
                    if (kv.Value == best) finalOptions[kv.Key] = kv.Value;
            }
            else
            {
                var outRange = scoreBoard.Where(kv => kv.Value != int.MaxValue && kv.Value > maxDistanceFromEnemy).ToList();
                if (outRange.Count == 0) return false; // no reachable enemies from any candidate
                int best = outRange.Min(kv => kv.Value); // closest outside range
                foreach (var kv in outRange)
                    if (kv.Value == best) finalOptions[kv.Key] = kv.Value;
            }

            if (finalOptions.Count == 0) return false;

            const int UNREACHABLE_D = 100000; // big but avoids overflow with small enemy counts
            TileCell destinationCell = null;
            long bestSum = long.MinValue;

            foreach (var kv in finalOptions)
            {
                var cell = kv.Key;
                long sum = 0;

                foreach (var enemyCell in playerMap.Values)
                {
                    if (enemyCell == null) continue;

                    var p = GridMovement.GetCellToCellPath(cell.CenterPoint, enemyCell.CenterPoint);
                    int d = (p == null || p.Count == 0) ? UNREACHABLE_D : p.Count;
                    sum += d;
                }

                if (sum > bestSum)
                {
                    bestSum = sum;
                    destinationCell = cell; // if equal, keep earlier (first wins)
                }
            }

            if (destinationCell == null) return false;

            // 6) build movement path, capped to maxMovementAllowed
            var movePath = GridMovement.GetCellToCellPath(currentCell.CenterPoint, destinationCell.CenterPoint);
            if (movePath.Contains(currentCell)) movePath.Remove(currentCell);
            if (movePath == null || movePath.Count == 0) return false;

            int steps = Math.Min(maxMovementAllowed, movePath.Count);
            ActMovementCellPath = movePath.Take(steps).ToList();

            return ActMovementCellPath.Count > 0;
        }
        private bool SetMovementPathAwayFromEnemy(
        ICombatant currentCombatant,
        int maxMovementAllowed,
        Dictionary<ICombatant, TileCell> playerMap)
        {
            var currentCell = TileManager.GetCell(currentCombatant.MovementController.CurrentPos);
            if (currentCell == null) return false;

            // 1) In-range cells (walkable region limited by movement allowance)
            var inRange = TileManager.GetFloodFillTileWithinRange(currentCell, maxMovementAllowed);
            if (inRange == null || inRange.Count == 0) return false;

            // 2) Filter to valid walkable & unblocked
            var validCells = new List<TileCell>(inRange.Count);
            foreach (var cell in inRange)
            {
                if (cell != null && cell.IsWalkable && !cell.BlockedByCombatant)
                    validCells.Add(cell);
            }
            if (validCells.Count == 0) return false;

            // 3) Choose the cell that maximizes the minimum distance to ANY enemy.
            //    Include unreachable enemies by treating their distance as "very large".
            const int UNREACHABLE_DIST = 1_000_000; // big but safe to add/compare
            TileCell bestCell = null;
            int bestMin = int.MinValue; // we want to maximize this; tie => keep first-tested

            foreach (var candidate in validCells) // first-tested tie-winner: don't update on equals
            {
                int minToAnyEnemy = int.MaxValue;

                foreach (var kvp in playerMap)
                {
                    var enemyCell = kvp.Value;
                    if (enemyCell == null) continue;

                    var path = GridMovement.GetCellToCellPath(candidate.CenterPoint, enemyCell.CenterPoint);

                    int d = (path == null || path.Count == 0) ? UNREACHABLE_DIST : path.Count;

                    if (d < minToAnyEnemy)
                        minToAnyEnemy = d;

                    // Early exit: if min already <= bestMin, this candidate cannot beat current best
                    if (minToAnyEnemy <= bestMin)
                        break;
                }

                // Prefer strictly larger minimum; ties are ignored (keep earlier tested)
                if (minToAnyEnemy > bestMin)
                {
                    bestMin = minToAnyEnemy;
                    bestCell = candidate;
                }
            }

            if (bestCell == null || bestCell == currentCell) return false;

            // 4) Build path to chosen cell with your pathfinder
            var bestPath = GridMovement.GetCellToCellPath(currentCell.CenterPoint, bestCell.CenterPoint);
            if (bestPath == null || bestPath.Count == 0) return false;

            // Remove starting cell if present
            if (bestPath.Count > 0 && bestPath[0] == currentCell)
                bestPath.RemoveAt(0);

            // Cap to movement allowance, just in case
            if (bestPath.Count > maxMovementAllowed)
                bestPath = bestPath.Take(maxMovementAllowed).ToList();

            if (bestPath.Count == 0) return false;

            ActMovementCellPath = bestPath;
            return true;
        }
        public bool SetMovementPathToClosestEnemy(ICombatant currentCombatant, int max, Dictionary<ICombatant, TileCell> playerMap)
        {
            TileCell currentCell = TileManager.GetCell(currentCombatant.MovementController.CurrentPos);
            List<TileCell> bestPath = FindClosestEnemy(currentCell, playerMap);

            if (TileManager.IsNeighbor(bestPath[bestPath.Count - 1], currentCell))
                return false;
            if (bestPath.Count <= 0) return false;

            ActMovementCellPath = bestPath.Count > max ? bestPath.Take(max).ToList() : bestPath;
            return true;
        }
        public List<TileCell> FindClosestEnemy(TileCell currentCell, Dictionary<ICombatant, TileCell> playerMap)
        {
            List<TileCell> bestPath = null;
            int shortestPathLength = 30;
            foreach (var kvp in playerMap)
            {
                List<TileCell> path = GridMovement.GetCellToCellPath(currentCell.CenterPoint, kvp.Value.CenterPoint);
                if (path.Count < shortestPathLength && path.Count > 0)
                {
                    shortestPathLength = path.Count;
                    bestPath = path;
                    if (bestPath.Contains(kvp.Value)) bestPath.Remove(kvp.Value);
                    if (bestPath.Contains(currentCell)) bestPath.Remove(currentCell);
                }
            }
            return bestPath;
        }
        public override void ClearActParams()
        {
            _playerMap.Clear();
            _aiMap.Clear();
            ActMovementCellPath.Clear();
        }


    }
    public class SummonAct : Act
    {
        public SummonedSavedStats SummonedMonsterStats {  get; set; }
        public string SummonedName { get; set; }
        public TileCell SummonedCell {  get; set; }
        public SummonAct(string name, SummonedSavedStats sumMon)
        {
            Icon = sumMon.Icon;
            SummonedName = name;
            ActType = ActType.Summon;
            SummonedMonsterStats = sumMon;

        }
        public override CombatState ExecutingState()
        {
            return CombatState.ExecutingSummon;
        }
        public override void ClearActParams()
        {
            _playerMap.Clear();
            _aiMap.Clear();
            SummonedCell = null;
        }
        public override bool TryAct(ICombatant currentCombatant, Dictionary<ICombatant, TileCell> playerMap, Dictionary<ICombatant, TileCell> aIMap)
        {
            throw new NotImplementedException();
        }
    }
    public class EndturnAct : Act
    {
        public EndturnAct()
        {
            Icon = AssetManager.GetTexture("EndTurnActIcon");
            ActType = ActType.EndTurn;
        }
        public override CombatState ExecutingState()
        {
            return CombatState.EndingTurn;
        }
        public override void ClearActParams()
        {
           
        }

        public override bool TryAct(ICombatant currentCombatant, Dictionary<ICombatant, TileCell> playerMap, Dictionary<ICombatant, TileCell> aIMap)
        {
            return true;
        }
    }

}
