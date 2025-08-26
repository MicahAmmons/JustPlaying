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

        private static int _summonOptionHeight = 64;
        private static int _summonOptionWidth = 64;
        private static int _summonOptionSpacing = 10;

        private static List<string> _log = new List<string>();

        private CombatStateMachine _stateMachine;
        private VisualEffectManager _visualEffectManager;
        private CombatUIManager _combatUIManager;
        public VisualEffectManager VisualEffectManager => _visualEffectManager;
        public CombatState StateCombat => _stateMachine.CurrentCombatState;
        // just a list of all monsters that have entered this combat at any time
        private List<ICombatant> _referenceTurnOrder = new List<ICombatant>();
        public int TotalCombatants = 0;
        // the current alive turn order
        public Queue<ICombatant> TurnOrder = new Queue<ICombatant>();

        private MapTile _currentMapTile;
        private Texture2D _playerCellOptions;//placeholder texture
        private SpriteFont _font;


        private List<(Rectangle rect, SingleAttack attack)> _attackButtons = new();

        private Dictionary<ICombatant, Rectangle> _displayStatRectangles = new Dictionary<ICombatant, Rectangle>();
        private Rectangle _endScreenRect = new Rectangle(710, 440, 500, 200);
        private Rectangle _exitCombatButtonRect = new Rectangle(885, 580, 150, 50);

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

        private int? _numberOfCellsMoved = 0;

        private VisualEffect _currentAttackVisualEffect;
        public Dictionary<string, int> defeatedMonsters = new Dictionary<string, int>();

        private List<TileCell> _summonSpawnableCells;
        private ICombatant _currentCombatant;
        public ICombatant CurrentCombatant => _currentCombatant;
        public WhoWon TheWinner = WhoWon.None;
        private float _timer = 0;

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
            _currentCombatant.ReadyToSummon += SummonSummonMonster;
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
        public void SetWhoWon(WhoWon whoWon)
        {
            TheWinner = whoWon;
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
            bool endingScreen = false;


            switch (StateCombat)
            {
                case CombatState.LocationSelection:
                    DrawLocationSelection(spriteBatch);
                    break;
                case CombatState.WaitingPlayerInput:
                    DrawActVisuals(spriteBatch);
                    break;
                case CombatState.WinnerChosen:
                    endingScreen = true;
                    break;
            }
            DrawTurnStateOverlay(spriteBatch);
            _visualEffectManager.Draw(spriteBatch);
            _combatUIManager.Draw(spriteBatch);
            if (StateCombat != CombatState.LocationSelection)
            _actManager.Draw(spriteBatch);
            if (endingScreen) DrawCombatEndScreen(spriteBatch);

        }
        public void DrawActVisuals(SpriteBatch sb)
        {
            Act act = _actManager.SelectedAct;
            if (act != null)
            {
                switch (act.ActType)
                {
                    case ActType.Move:
                        DrawMoveAct(sb);
                        break;
                    case ActType.Attack:

                        break;
                    case ActType.Summon:
                        DrawSummonAct(sb);
                        break;
                }
            }
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
            var openCells = cells.Where(cells => cells.IsWalkable && !cells.BlockedByMonster).ToList();

            foreach (var cell in openCells)
            {
                cell.DrawCellHighlight(sb, _cellHighlightColors.ValidTarget, 5);
                if (_currentMouseHoverCell == cell)
                {
                    Act act = _actManager.SelectedAct;
                    if (act is SummonAct summonAct)
                    {
                        string name = summonAct.SummonedName;
                        DrawEntityIdlePreviewOnCell(cell, CombatMonsterManager.GetIdleAnimationData(name));
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
            var openCells = cells.Where(cells => cells.IsWalkable && !cells.BlockedByMonster).ToList();
            var reachableCells = TileManager.GetReachableCellsFromSubset(origin, openCells, (int)combatant.CurrentStats.MP);
            foreach (var cell in reachableCells)
            {
                cell.DrawCellHighlight(sb, _cellHighlightColors.Walkable, 5);
                if (_currentMouseHoverCell == cell)
                {
                    AnimationData data = combatant.Is == CombatMonsterType.Player ? data = PlayerManager.GetIdleAnimationData() : data = CombatMonsterManager.GetIdleAnimationData(combatant.UniqueId);
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

        public void DrawCombatEndScreen(SpriteBatch spriteBatch)
        {
            // Background panel
            spriteBatch.Draw(_playerCellOptions, _endScreenRect, Color.DarkSlateGray * 0.9f);

            // Determine outcome and text
            string headerText = TheWinner == WhoWon.Player ? "Victory" : "Defeat";
            Color headerColor = TheWinner == WhoWon.Player ? Color.LightGreen : Color.Red;

            Vector2 headerSize = _font.MeasureString(headerText);
            Vector2 headerPos = new Vector2(
                _endScreenRect.X + (_endScreenRect.Width - headerSize.X) / 2,
                _endScreenRect.Y + 20
            );

            spriteBatch.DrawString(_font, headerText, headerPos, headerColor);

            // If player won, list defeated monsters
            if (TheWinner == WhoWon.Player)
            {
                int yOffset = 70;

                foreach (var entry in defeatedMonsters)
                {
                    string line = $"{entry.Value}x {entry.Key}";
                    Vector2 textSize = _font.MeasureString(line);
                    Vector2 linePos = new Vector2(
                        _endScreenRect.X + (_endScreenRect.Width - textSize.X) / 2,
                        _endScreenRect.Y + yOffset
                    );

                    spriteBatch.DrawString(_font, line, linePos, Color.White);
                    yOffset += 25;
                }
            }

            // Exit Combat button
            spriteBatch.Draw(_playerCellOptions, _exitCombatButtonRect, Color.DarkRed);

            string buttonText = "Exit Combat";
            Vector2 buttonTextSize = _font.MeasureString(buttonText);
            Vector2 buttonTextPos = new Vector2(
                _exitCombatButtonRect.X + (_exitCombatButtonRect.Width - buttonTextSize.X) / 2,
                _exitCombatButtonRect.Y + (_exitCombatButtonRect.Height - buttonTextSize.Y) / 2
            );

            spriteBatch.DrawString(_font, buttonText, buttonTextPos, Color.White);
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
                cell.AddAnimation(data);
                return;
        }
        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateMouseWhereabouts();
            UpdateCombatantCount();
            _combatUIManager.Update();
            if (StateCombat == CombatState.WinnerChosen) return;
            UpdateInput(gameTime);
            _visualEffectManager.Update(delta);
            ToggleIsDead(); // toggles ISDead as well as clears aspects
            UpdateTurnOrder();
            UpdateMonsterCellMap();
            _actManager.Update();
            if (WinnerChosen())
            {
                SetCombatState(CombatState.WinnerChosen);
                CountDefeatedMonsters();
                return;
            }
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
                    _actManager.ResetConfirmedAndSelectedAct();
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
                    if (!_currentCombatant.ExecutingSummon) EndOfAction();
                    break;
                case CombatState.EndingTurn:
                    _currentCombatant.ResolveEffects(TickedTiming.EndOfTurn);
                    SetCombatState(CombatState.ResolvingEndOfTurnEffects);
                    break;
                case CombatState.ResolvingEndOfTurnEffects:
                    if (_currentCombatant.EndOfTurnEffectsResolved) Endturn();

                    break;
            }
        }

        private void Endturn()
        {
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
        private bool WinnerChosen()
        {
            if (AIControlledMonsterMap.Count == 0) { SetWhoWon(WhoWon.Player); return true; }
            if (_currentPlayer.isDead) { SetWhoWon(WhoWon.Monster); return true; }
            return false;
        }
        public bool CheckIfAIShouldEndTurn()
        {
            if (_currentCombatant.CurrentStats.AP <= 0)
            {
                return true;
            }
            return false;
        }
        public void UpdateInput(GameTime gameTime)
        {
            switch (StateCombat)
            {
                case CombatState.LocationSelection: HandleLocationSelectionInput(); break;
            }
        }



        private void HandlePlayerClickLeaveCombat()
        {
            if (InputManager.IsLeftClick() && _exitCombatButtonRect.Contains(_currentMousePos))
            {
                CombatGuard.EndCombat();
            }
        }

        private void UpdateStatHoverMonsterCell()
        {
            foreach (var kvp in _displayStatRectangles)
            {
                ICombatant mon = kvp.Key;
                Rectangle rect = kvp.Value;
                if (rect.Contains(_currentMousePos))
                {
                    mon.DrawSpecifics.shrink = 5;
                    Color col = ColorPalette.DarkColor;
                    switch (mon.Is)
                    {
                        case CombatMonsterType.Player: col = Color.LimeGreen; break;
                        case CombatMonsterType.AI: col = Color.MediumVioletRed; break;
                        case CombatMonsterType.Summoned: col = Color.LightCoral; break;
                    }
                    mon.DrawSpecifics.HighlightCol = col;
                    mon.DrawSpecifics.DrawCellHightlight = true;
                    return;
                }
            }
        }
        private void HandlePlayerChooseSummonedCell()
        {
            ICombatant combatant = _currentCombatant;
            TileCell cell = _currentClickedCell;
            if (combatant.CurrentStats.CurrentSelectedSummon != null && _summonSpawnableCells.Contains(cell))
            {
                
                combatant.CurrentStats.CurrentSelectedSummon = null;
                combatant.SpendActionPoint();

            }
        }
        private void ResetClickValues()
        {
            ICombatant mon = _currentCombatant;
            mon.CurrentStats.CurrentSelectedSummon = null;
        }


        private void HandleLocationSelectionInput()
        {
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
                    cell.BlockedByMonster = true;
                    _playerControlledMonsterMap[mon] = cell;

                }
                else if (mon.Is == CombatMonsterType.AI)
                {
                    cell.BlockedByMonster = true;
                    _aIControlledMonsterMap[mon] = cell;
                }

            }
        }
        public void ClearEntityMaps()
        {
            foreach (var cell in _aIControlledMonsterMap.Values)
            {
                cell.BlockedByMonster = false;
            }
            foreach (var cell in _playerControlledMonsterMap.Values)
            {
                cell.BlockedByMonster = false;
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
public enum WhoWon
{
    None,
    Player,
    Monster
}

