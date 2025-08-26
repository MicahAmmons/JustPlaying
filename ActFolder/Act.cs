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
            _actController.Update();
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
            }
            return CombatState.ExecutingAttack;
        }
        public void ResetConfirmedAndSelectedAct()
        {
           
            _actController.SelectedAct = null;
            ConfirmedAct = null;
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
                moveAct.ActMovementCellPath = cells;
                ConfirmedAct = moveAct;
            }
        }
        public void ConfirmSummonAct(TileCell cell)
        {
            if (SelectedAct is SummonAct summonAct)
            {
                summonAct.SummonedCell = cell;
                ConfirmedAct = summonAct;
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
        public void Update()
        {

            ButtonManager.UpdateInput();
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
        public Button ActButton { get; set; }
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

            if (!ValidTargets()) return false;
            return true;
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
            }
            return true;

        }
        public override CombatState ExecutingState()
        {
            return CombatState.ExecutingAttack;
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
            }
            return true;
        }
        public bool SetMovementPathToClosestEnemy(ICombatant currentCombatant, int max, Dictionary<ICombatant, TileCell> playerMap)
        {
            TileCell currentCell = TileManager.GetCell(currentCombatant.MovementController.CurrentPos);
            Dictionary<ICombatant, TileCell> playerControlledCells = playerMap;
            List<TileCell> bestPath = null;
            int shortestPathLength = 30;
            foreach (var kvp in playerControlledCells)
            {
                if (TileManager.IsNeighbor(kvp.Value, currentCell))
                    return false;
                //already next to enemy

                List<TileCell> path = GridMovement.GetCellToCellPath(currentCell.CenterPoint, kvp.Value.CenterPoint);
                if (path.Count < shortestPathLength && path.Count > 0)
                {
                    shortestPathLength = path.Count;
                    bestPath = path;
                    if (bestPath.Contains(kvp.Value)) bestPath.Remove(kvp.Value);
                    if (bestPath.Contains(currentCell)) bestPath.Remove(currentCell);
                }
            }
            if (bestPath.Count <= 0) return false;

            ActMovementCellPath = bestPath.Count > max ? bestPath.Take(max).ToList() : bestPath;
            return true;


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
   
}
