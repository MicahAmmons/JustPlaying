using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.ActFolder;
using PlayingAround.AnimationFolder;
using PlayingAround.ButtonsFolder;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Tiles;
using PlayingAround.Managers.UI;
using PlayingAround.Managers.UI.Combat;
using PlayingAround.Visuals;
using PlayingAround.World.MapTiles.CellHighlights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using static CombatStateMachine;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatManager
    {
        private CombatStateMachine _stateMachine;
        private VisualEffectManager _visualEffectManager;
        private CombatUIManager _combatUIManager;
        public VisualEffectManager VisualEffectManager => _visualEffectManager;
        public CombatState StateCombat => _stateMachine.CurrentCombatState;

        private List<ICombatant> _referenceTurnOrder = new List<ICombatant>();

        public int TotalCombatants = 0;
        // the current alive turn order
        public Queue<ICombatant> TurnOrder = new Queue<ICombatant>();

        private MapTile _currentMapTile;
        private Texture2D _playerCellOptions;//placeholder texture
        private SpriteFont _font;

        private Dictionary<ICombatant, Rectangle> _displayStatRectangles = new Dictionary<ICombatant, Rectangle>();
      

        public PlayMonsters PlayMonsters; // kept as reference as needed
        private List<TileCell> _playerSpawnableCells = new List<TileCell>();
        private List<TileCell> _monsterSpawnableCells = new List<TileCell>();

        private TileCell _currentClickedCell;
        private TileCell _currentMouseHoverCell;
        private Vector2 _currentMousePos;

        private TileCellHighlights _cellHighlightColors;

        private Dictionary<ICombatant, TileCell> _playerControlledMonsterMap = new();
        private Dictionary<ICombatant, TileCell> _aIControlledMonsterMap = new();
        public Dictionary<ICombatant, TileCell> AIControlledMonsterMap => _aIControlledMonsterMap;
        public Dictionary<ICombatant, TileCell> PlayerControlledMonsterMap => _playerControlledMonsterMap;
        public Dictionary<string, int> defeatedMonsters = new Dictionary<string, int>();

        private ICombatant _currentCombatant;
        public ICombatant CurrentCombatant => _currentCombatant;
        public CombatMonsterType TheWinner;
        private ExitCombatController _exitCombatContr;
        public ExitCombatController ExitCombatContr => _exitCombatContr;    

        private Player _currentPlayer => PlayerManager.CurrentPlayer;
        private ActManager _actManager;


        public CombatManager(PlayMonsters playMonsters)
        {
            _stateMachine = new CombatStateMachine();
            _currentMapTile = TileManager.CurrentMapTile;
            _visualEffectManager = new VisualEffectManager();
            _playerCellOptions = AssetManager.GetTexture("fightBackground");
            _font = AssetManager.GetFont("mainFont");
            _combatUIManager = new CombatUIManager();
            _cellHighlightColors = TileManager.CurrentMapTile.CellHighlights;
            _actManager = new ActManager();

            PlayMonsters = playMonsters;
            _currentPlayer.MovementController.ClearMovementPath();
            _currentPlayer.MovementController.CachPos();


            SetSpawnableCells();
            SetCombatantStartingPos();
            SetTurnOrder();
            UpdateCombatantPositions();

            UpdateCurrentMonster();

            InitilizeUIElements();
            SceneManager.SetState(SceneState.Combat);
        }
        public void SetSpawnableCells()
        {
            foreach (var cell in _currentMapTile.MonsterSpawnableCells)
            {
                _monsterSpawnableCells.Add(cell);
            }
            foreach (var cell in _currentMapTile.PlayerSpawnableCells)
            {
                _playerSpawnableCells.Add(cell);
            }
        }
        public void SetCombatState(CombatState state)
        {
            _stateMachine.SetCombatState(state);
        }
        public bool SetWhoWon()
        {
            if (AIControlledMonsterMap.Count == 0 )
            {
                TheWinner = CombatMonsterType.Player;
                return true; 
            }
            if (_currentPlayer.isDead)
            {
                TheWinner = CombatMonsterType.AI;
                return true;
            }
            return false;
        }
        private void SetTurnOrder()
        {
            List<ICombatant> allCombatants = new List<ICombatant>();
            allCombatants.AddRange(PlayMonsters.Monsters);
            allCombatants.Add(_currentPlayer);
            allCombatants.Sort((a, b) => b.BaseStats.Initiative.CompareTo((int)a.BaseStats.Initiative));
            foreach (var entity in allCombatants)
            {
                _referenceTurnOrder.Add(entity);
                TurnOrder.Enqueue(entity);
            }

        }
        private void SetCombatantStartingPos()
        {
            if (_monsterSpawnableCells.Count < PlayMonsters.Monsters.Count)
                throw new InvalidOperationException("Not enough spawnable cells for monsters.");
            List<TileCell> spawnableCells = new List<TileCell>(_monsterSpawnableCells);
            List<CombatMonster> comMon = new List<CombatMonster>(PlayMonsters.Monsters);
            do
            {
                foreach (var mon in comMon)
                {
                    int index = RandomHut.rng.Next(spawnableCells.Count);
                    Vector2 pos = spawnableCells[index].CenterPoint;

                    mon.MovementController.SetCurrentPos(pos);
                    spawnableCells.RemoveAt(index);
                }

            } while (spawnableCells.Count < spawnableCells.Count - PlayMonsters.Monsters.Count);
        }
        private void InitilizeUIElements()
        {

            foreach (var comb in TurnOrder)
            {
                AddCombatantUIInfo(comb);
            }
        }



        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            DrawActVisuals(spriteBatch);
            DrawLocationSelection(spriteBatch);
            DrawTurnStateOverlay(spriteBatch);// debug overlay
            _combatUIManager.Draw(spriteBatch);
            if (StateCombat != CombatState.LocationSelection)
                _actManager.Draw(spriteBatch);
        }
        public void DrawActVisuals(SpriteBatch sb)
        {
            if (StateCombat != CombatState.WaitingPlayerInput) return;
            Act act = _actManager.SelectedAct;
            if (act != null)
            {
                switch (act.ActType)
                {
                    case ActType.Move:
                        DrawMoveAct(sb);
                        break;
                    case ActType.Summon:
                        DrawSummonAct(sb);
                        break;
                    case ActType.Attack:
                        DrawAttackAct(sb, act);
                        break;
                }
            }
        }
        private void DrawAttackAct(SpriteBatch sb, Act act)
        {
            DrawAttackRangeOptions(sb, act);
        }
        private void DrawAttackRangeOptions(SpriteBatch sb, Act act)
        {
            var comb = _currentCombatant;
            if (comb.Is == CombatMonsterType.AI) return;
            if (act is not AttackAct attackAct) return;

            TileCell origin = _playerControlledMonsterMap[comb];
            int range = attackAct.Attack.Range;

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, range);

            List<TileCell> invalidCells = new List<TileCell>();
            List<TileCell> validCells = new List<TileCell>();

            var targetTypes = attackAct.Attack.TargetType;

            foreach (var cell in TileManager.GetFloodFillTileWithinRange(origin, range))
            {
                if (cell.IsWalkable && !cell.BlockedByCombatant)
                {
                    invalidCells.Add(cell);
                    continue;
                }
                if (cell.IsWalkable && cell.BlockedByCombatant)
                {
                    if (targetTypes.Contains(GetCombatantAtCell(cell).Is))
                    {
                        invalidCells.Add(cell);
                        continue;
                    }
                    else validCells.Add(cell);
                }
            }
            foreach (var cell in invalidCells)
            {
                cell.DrawCellHighlight(sb, _cellHighlightColors.InvalidTarget);
            }
            foreach (var cell in validCells)
            {
                Color col = _cellHighlightColors.ValidTarget;
                if (_currentMouseHoverCell == cell)
                {
                    col = Color.Black;
                }
                cell.DrawCellHighlight(sb, col);
                if (_currentClickedCell == cell)
                {
                    PlayerSelectedAttackTarget(cell);
                    return;
                }
            }

        }
        private void PlayerSelectedAttackTarget(TileCell cell)
        {
            Dictionary<ICombatant, TileCell> effected = new Dictionary<ICombatant, TileCell> ();
            effected[GetCombatantAtCell(cell)] = cell;
            _actManager.ConfirmAttackAct(effected);
        }
        private void DrawSummonAct(SpriteBatch sb)
        {
            DrawSummonableCells(sb);
        }
        private void DrawSummonableCells(SpriteBatch sb)
        {
            ICombatant comb = _currentCombatant;
            if (comb.Is == CombatMonsterType.AI) return;
            TileCell origin = _playerControlledMonsterMap[comb];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, 2);
            var openCells = cells.Where(cells => cells.IsWalkable && !cells.BlockedByCombatant).ToList();

            foreach (var cell in openCells)
            {
                cell.DrawCellHighlight(sb, _cellHighlightColors.ValidTarget, 5);
                if (_currentMouseHoverCell == cell)
                {
                    Act act = _actManager.SelectedAct;
                    if (act is SummonAct summonAct)
                    {
                        string name = summonAct.SummonedName;
                        DrawEntityIdlePreviewOnCell(cell, AnimationLibrary.GetIdleAnimationData(name));
                    }
                }
                if (_currentClickedCell == cell)
                {
                    PlayerSelectedSummonDestination(cell);
                    return;
                }
            }
        }
        private void PlayerSelectedSummonDestination(TileCell cell)
        {
            _actManager.ConfirmSummonAct(cell);
        }
        public void DrawMoveAct(SpriteBatch sb)
        {
            DrawMoveableCells(sb);
        }
        public void DrawMoveableCells(SpriteBatch sb)
        {
            ICombatant combatant = _currentCombatant;
            if (combatant.Is == CombatMonsterType.AI) return;
            TileCell origin = _playerControlledMonsterMap[combatant];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)combatant.CurrentStats.MP);
            var openCells = cells.Where(cells => cells.IsWalkable && !cells.BlockedByCombatant).ToList();
            var reachableCells = TileManager.GetReachableCellsFromSubset(origin, openCells, (int)combatant.CurrentStats.MP);
            foreach (var cell in reachableCells)
            {
                cell.DrawCellHighlight(sb, _cellHighlightColors.Walkable, 5);
                if (_currentMouseHoverCell == cell)
                {
                    AnimationData data = combatant.Is == CombatMonsterType.Player ? data = PlayerManager.GetIdleAnimationData() : data = AnimationLibrary.GetIdleAnimationData(combatant.UniqueId);
                    DrawEntityIdlePreviewOnCell(cell, data);
                }
                if (_currentClickedCell == cell)
                {
                    PlayerSelectedMoveDestination(GridMovement.GetCellToCellPath(combatant.MovementController.CurrentPos, cell.CenterPoint));
                   
                    return;
                }
            }
        }
        public void PlayerSelectedMoveDestination(List<TileCell> cells)
        {
                cells.RemoveAll(c => c == TileManager.GetCell(_currentCombatant.MovementController.CurrentPos));
                _actManager.ConfirmMoveAct(cells);

        }
        private void DrawTurnStateOverlay(SpriteBatch spriteBatch)
        {
            string combatStateText = $"CombatState: {StateCombat}";
            SpriteFont font = AssetManager.GetFont("mainFont");
            Vector2 textSize = font.MeasureString(combatStateText);
            int screenWidth = ViewportManager.ScreenWidth;

            Vector2 position = new Vector2(screenWidth - textSize.X - 200, 10);
            spriteBatch.DrawString(font, combatStateText, new Vector2(screenWidth - textSize.X - 200, 30), Color.Orange);

        }
        private void DrawLocationSelection(SpriteBatch spriteBatch)
        {
            if (StateCombat != CombatState.LocationSelection) return;
            DrawSpawnableTiles(spriteBatch);

            if (_playerSpawnableCells.Contains( _currentMouseHoverCell))
                DrawEntityIdlePreviewOnCell(_currentMouseHoverCell, PlayerManager.GetIdleAnimationData());
        }
        private void DrawSpawnableTiles(SpriteBatch spriteBatch)
        {
            foreach (var tile in _playerSpawnableCells)
                tile.DrawCellHighlight(spriteBatch, _cellHighlightColors.PlayerStartable, 5);

            foreach (var tile in _monsterSpawnableCells)
                tile.DrawCellHighlight(spriteBatch, _cellHighlightColors.MonsterStartable, 5);
        }
        private void DrawEntityIdlePreviewOnCell(TileCell cell, AnimationData data )
        {
                TileCellManager.AddActiveAnimationCell(cell);
                cell.AddIdleAnimation(data);
                return;
        }





        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateEachFrameMethods();
            //_combatButtonController.Update();
            _combatUIManager.Update();
            _actManager.Update();


            
            switch (StateCombat)
            {
                case CombatState.TurnStart:
                    ICombatant com = _currentCombatant;
                    com.UpdateTopOfRoundStats();
                    com.ResolveEffects(TickedTiming.StartOfTurn);
                    SetCombatState(CombatState.ResolvingStartOfTurnEffects);
                    break;
                case CombatState.ResolvingStartOfTurnEffects:
                    if (_currentCombatant.StartOfTurnEffectsResolved) SetCombatState(CombatState.TopOfAction);
                    break;
                case CombatState.TopOfAction:
                    _currentCombatant.UpdateTopOfActionStats();
                    SetCombatState(
                              _currentCombatant.Is == CombatMonsterType.AI
                              ? CombatState.ActionNavigation
                              : CombatState.WaitingPlayerInput);
                    break;
                case CombatState.WaitingPlayerInput:
                    if (_actManager.ConfirmedAct == null) return;
                    Act confirmedAct = _actManager.ConfirmedAct;
                    _actManager.ResetSelectedAct();
                    _currentCombatant.BeginAct(confirmedAct);
                    SetCombatState(confirmedAct.ExecutingState());
                    break;
                case CombatState.ActionNavigation:
                    if (CheckIfAIShouldEndTurn()) {SetCombatState(CombatState.EndingTurn); return; }

                    CombatState? state = _actManager.DecideNextAct();
                    if (state != null) {SetCombatState((CombatState)state); break;}

                    SetCombatState(CombatState.EndingTurn);
                    break;
                case CombatState.ExecutingAttack:
                    if (!_currentCombatant.ExecutingAttack)
                    {
                        EndOfAction();
                    }
                    break;
                case CombatState.ExecutingMove:
                    if (!_currentCombatant.ExecutingMove) EndOfAction();
                    break;
                case CombatState.ExecutingSummon:
                    if (!_currentCombatant.ExecutingSummon) 
                    { 
                        SummonSummonMonster((SummonAct)_actManager.ConfirmedAct);
                        EndOfAction();
                    };
                    break;
                case CombatState.EndingTurn:
                    _currentCombatant.ResolveEffects(TickedTiming.EndOfTurn);
                    SetCombatState(CombatState.ResolvingEndOfTurnEffects);
                    break;
                case CombatState.ResolvingEndOfTurnEffects:
                    if (_currentCombatant.EndOfTurnEffectsResolved) Endturn();

                    break;
                case CombatState.WinnerChosen:
                    _exitCombatContr?.Update();
                    CombatGuard.EndCombat();
                    return;
            }
            TryEndCombat();
        }
        private void UpdateEachFrameMethods()
        {
            UpdateMouseWhereabouts();
            HandleLocationSelectionInput();
            UpdateCombatantCount();
            ToggleIsDead(); // toggles ISDead as well as clears aspects
            UpdateTurnOrder();
            UpdateMonsterCellMap();
        }
        private void Endturn()
        {
            _actManager.ResetActs();
            SendMonsterToBackOfQueue();
            SetCombatState(CombatState.TurnStart);
        }
        private void EndOfAction()
        { 
            _actManager.ResetActs();
            SetCombatState(CombatState.TopOfAction);
        }
        private void UpdateCombatantCount()
        {
            if (TotalCombatants != TurnOrder.Count)
            {
                TotalCombatants = TurnOrder.Count;
            }
        }
        private void TryEndCombat()
        {
            if (!SetWhoWon()) return;

            SetCombatState(CombatState.WinnerChosen);
            CombatMonsterType winnerType = TheWinner; 

            _exitCombatContr = new ExitCombatController(winnerType, CountDefeatedMonsters());
        }

        public bool CheckIfAIShouldEndTurn()
        {
            if (_actManager.ConfirmedAct is EndturnAct act)
            {
                return true;
            }
            return false;
        }
        private void HandleLocationSelectionInput()
        {
            if (StateCombat != CombatState.LocationSelection) return;
            if (_playerSpawnableCells.Contains(_currentMouseHoverCell) )
            {
                if (InputManager.IsLeftClick())
                {
                    _currentPlayer.MovementController.SetCurrentPos(_currentMouseHoverCell.CenterPoint);
                    _currentPlayer.MovementController.ToggleAllowedToBeDrawn(true);
                    _currentPlayer.CreateNewActController();
                    SetCombatState(CombatState.TurnStart);
                }

            }
        }
        private void UpdateMouseWhereabouts()
        {
            _currentMousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);
            _currentMouseHoverCell = TileManager.GetCell(_currentMousePos);
            if (InputManager.IsLeftClick())
            {
                _currentClickedCell = TileManager.GetCell(_currentMousePos);
            }

        }
        public void UpdateMonsterCellMap()
        {
            if (TurnOrder.Count <= 0) return;
            ClearEntityMaps();
            foreach (var mon in TurnOrder)
            {
                TileCell cell = TileManager.GetCell((Vector2)mon.MovementController.CurrentPos);
                if (mon.Is == CombatMonsterType.Summoned || mon.Is == CombatMonsterType.Player)
                {
                    cell.AssignCombatant(mon);
                    _playerControlledMonsterMap[mon] = cell;

                }
                else if (mon.Is == CombatMonsterType.AI)
                {
                    cell.AssignCombatant(mon);
                    _aIControlledMonsterMap[mon] = cell;
                }

            }
        }
        public void ClearEntityMaps()
        {
            foreach (var cell in _aIControlledMonsterMap.Values)
            {
                cell.UnassignCombatant();
            }
            foreach (var cell in _playerControlledMonsterMap.Values)
            {
                cell.UnassignCombatant();
            }
            _playerControlledMonsterMap.Clear();
            _aIControlledMonsterMap.Clear();
        }
        private void ToggleIsDead()
        {
            bool someOneDied = false;
            foreach (var mon in TurnOrder)
            {
                if (mon.CurrentStats.Health <= 0)
                {
                    if (mon.isDead) continue;
                    TileCell deadCell = null;
                    if (_aIControlledMonsterMap.TryGetValue(mon, out var aiCell))
                    {
                        deadCell = aiCell;
                    }
                    else if (_playerControlledMonsterMap.TryGetValue(mon, out var playerCell))
                    {
                        deadCell = playerCell;
                    }
                    deadCell.UnassignCombatant();
                    mon.isDead = true;
                    mon.Aspects.Clear();
                    someOneDied = true;
                }
            }
            if (someOneDied) RebuildTurnOrderExcludingDead();
            UpdateCombatantPositions();
        }
        private void RebuildTurnOrderExcludingDead()
        {
            Queue<ICombatant> newQueue = new Queue<ICombatant>();

            foreach (var mon in TurnOrder)
            {
                // remove summoned monster if they die
                if (mon.Is == CombatMonsterType.Summoned && mon.isDead)
                {
                    _combatUIManager.RemoveCombatantUI(mon);
                }
                if (!mon.isDead)
                {
                    newQueue.Enqueue(mon);
                }
            }

            TurnOrder = newQueue;
        }
        private void UpdateTurnOrder()
        {
            int maxTries = TurnOrder.Count;
            while (maxTries-- > 0)
            {
                ICombatant mon = _currentCombatant;
                if (!mon.isDead)
                {
                    UpdateCurrentMonster();
                    return;
                }
                SendMonsterToBackOfQueue();
            }
        }
        private void SendMonsterToBackOfQueue()
        {
            ICombatant mon = TurnOrder.Dequeue();
            TurnOrder.Enqueue(mon);
        }
        private void UpdateCurrentMonster()
        {
            if (_currentCombatant == TurnOrder.Peek()) return;
            _currentCombatant = TurnOrder.Peek();
        }
        private ICombatant GetCombatantAtCell(TileCell cell)
        {
            
 

            foreach (var kvp in _aIControlledMonsterMap)
            {
                var target = kvp.Key;
                var targetCell = kvp.Value;

                if (targetCell.X == cell.X && targetCell.Y == cell.Y)
                    return target;
            }
            foreach (var kvp in _playerControlledMonsterMap)
            {
                var target = kvp.Key;
                var targetCell = kvp.Value;
                if (targetCell.X == cell.X && targetCell.Y == cell.Y)
                    return target;
            }

            return null;
        }




        public void SummonSummonMonster(SummonAct act)
        {
            ICombatant comSumMon = (CombatMonsterManager.SummonMonsterToCombat(act.SummonedName));
            comSumMon.MovementController.SetCurrentPos(act.SummonedCell.CenterPoint);
            AddComMonToTurnOrder(comSumMon);
        }
        private void AddComMonToTurnOrder(ICombatant mon)
        {
            List<ICombatant> updatedList = new List<ICombatant>();

            bool inserted = false;

            foreach (var combatMon in TurnOrder)
            {
                if (!inserted && mon.BaseStats.Initiative >= combatMon.BaseStats.Initiative)
                {
                    updatedList.Add(mon);
                    inserted = true;
                }
                updatedList.Add(combatMon);
            }

            if (!inserted)
            {
                updatedList.Add(mon);
            }
            if (updatedList[0] == mon)
            {
                updatedList.Remove(mon);
                updatedList.Add(mon);
            }

            TurnOrder = new Queue<ICombatant>(updatedList);
            updatedList.Sort((a, b) => b.BaseStats.Initiative.CompareTo(a.BaseStats.Initiative));
            _referenceTurnOrder = updatedList;
            UpdateCombatantPositions();
            AddCombatantUIInfo(mon);
        }
        private void UpdateCombatantPositions()
        {
            List<ICombatant> comList = new List<ICombatant>(TurnOrder);
            for (int i = 0; i < comList.Count; i++)
            {
                var comb = comList[i];
                comb.UpdateCombatPosition(i);
            }
        }
        private void AddCombatantUIInfo(ICombatant comb)
        {
            _combatUIManager.AddCombatantUIInfo(new CombatantInfoUI(comb));
        }
        public Dictionary<string, int> CountDefeatedMonsters()
        {

            foreach (var mon in _referenceTurnOrder)
            {
                if (mon.isDead && mon.Is == CombatMonsterType.AI)
                {
                    if (defeatedMonsters.ContainsKey(mon.UniqueId))
                    {
                        defeatedMonsters[mon.UniqueId]++;
                    }
                    else
                    {
                        defeatedMonsters.Add(mon.UniqueId, 1);
                    }
                }
            }
            return defeatedMonsters;
        }


    }
}

