using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ButtonsFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Manager;
using PlayingAround.Managers;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.CombatAttacks;
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
        private Act _confirmedAct = null;
        private Queue<Act> ActOrderTry = new Queue<Act>();


        public ActManager()
        {

        }
        public void Draw(SpriteBatch sb)
        {
            _actController.DrawButtons(sb);
        }
        public void Update()
        {

        }
        public CombatState? DecideNextAct()
        {
            foreach (var act in _actController.ActsOrder)
            {
                ActOrderTry.Enqueue(act);
            }

            while (ActOrderTry.Count > 0)
            {
                var act = ActOrderTry.Dequeue();
                ActType type = act.ActType;
                CombatState state = StateHelper(type);

                if (act.TryAct(_currentCombatant, PlayerMap, AIMap)) 
                { 
                    _confirmedAct = act;
                    _currentCombatant.BeginAct(act);
                    return state; 
                }

            }
            return null;
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
        public void UpdateConfirmedAct()
        {
            _confirmedAct = _actController.ConfirmedAct;
            _actController.ResetController();
        }

    }

    public class ActController
    {
        public List<Act> ActsOrder {  get; set; } = new List<Act>();
        public Act SelectedAct { get; set; } = null;
        public Act ConfirmedAct { get; set; } = null;


        const int ButtonSize = 64;
        const int Buffer = 5;
        int startingX = (ViewportManager.ScreenWidth / 2);
        int y = (ViewportManager.ScreenHeight - ButtonSize - Buffer);


        public ActController(ActData data)
        {
            foreach (var act in data.ActionOrder)
            {
                switch (act.Type)
                {
                    case ActType.Attack:
                        ActsOrder.Add(new AttackAct(act));
                    break;

                    case ActType.Move:
                        ActsOrder.Add(new MoveAct(act));
                        break;
                }
            }
            for (int i = 0; i < ActsOrder.Count - 1; i++)
            {
                int x = startingX - (i * (ButtonSize + Buffer));
                Rectangle rect = new Rectangle(x, y, ButtonSize, ButtonSize);
                ActsOrder[i].ActButton = new Button(rect);
            }
        }
        public void UpdateInput()
        {
            var mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);
            bool leftPressedThisFrame = InputManager.IsLeftClick();
            if (leftPressedThisFrame)
            {
                foreach (var act in ActsOrder)
                {
                    if (act.ActButton.DrawRectangle.Contains(mousePoint))
                    {
                        ResetAllButtons();
                        act.ActButton.CurrentlySelected = true;
                        SelectedAct = act;
                    }
                }
            }
        }
        public void ConfirmAct()
        {
            ConfirmedAct = SelectedAct;
            ResetAllButtons() ;
            ActConfirmed?.Invoke();
        }
        public void ResetAllButtons()
        {
            SelectedAct = null;
            foreach (var act in ActsOrder)
            {
                act.ActButton.ResetPermissions();
            }
        }
        public void DrawButtons(SpriteBatch sb)
        {
            foreach (var act in ActsOrder)
            {
                act.ActButton.Draw(sb); 
            }
        }
        public void ResetController()
        {
            ConfirmedAct = null;
            SelectedAct = null;
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
        public abstract void ClearActParams();

        public abstract bool TryAct(ICombatant currentCombatant, Dictionary<ICombatant, TileCell> playerMap, Dictionary<ICombatant, TileCell> aIMap);
    }
    public class AttackAct : Act
    {
        public SingleAttack Attack {  get; set; }
        public Dictionary<ICombatant, TileCell> EffectedTargets { get; set; } = new Dictionary<ICombatant, TileCell>();
        public AttackAct(SpecificActData data)
        {
            Attack = AttackManager.GetAttack(data.AttackName, data.ElementType);
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
            _combatant = null;
            _playerMap.Clear();
            _aiMap.Clear();
            EffectedTargets.Clear();
        }

    }
    public class MoveAct : Act
    {
        public MovementAmount MovementAmount { get; set; }
        public MoveAct(SpecificActData data)
        {
            MovementAmount = data.MovementAmount;
            Target = data.ActionTarget;
            ActType = data.Type;
        }
    }
    public class SummonAct : Act
    {
        public SummonAct(SpecificActData data)
        {

        }
    }
   
}
