using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Managers.Tiles;
using PlayingAround.Utils;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using static CombatStateMachine;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatManager
    {

        private static int _summonOptionHeight = 64;
        private static int _summonOptionWidth = 64;
        private static int _summonOptionSpacing = 10;

        private static List<string> _log = new List<string>();
        private static int _maxStrings = 50;

        private CombatUIManager _combatUIManager;
        private CombatStateMachine _stateMachine;
        private VisualEffectManager _visualEffectManager;
        public VisualEffectManager VisualEffectManager => _visualEffectManager;
        public PlayerTurnState StatePlayerTurn => _stateMachine.CurrentPlayerTurnState;
        public CombatState StateCombat => _stateMachine.CurrentCombatState;
        public SummonedTurnState StateSummoned => _stateMachine.CurrentSummonedTurnState;
        public AITurnState StateAI => _stateMachine.CurrentAITurnState;
        private List<ICombatant> _referenceTurnOrder = new List<ICombatant>();
        public Queue<ICombatant> TurnOrder = new Queue<ICombatant>();

        private MapTile _currentMapTile;
        private Texture2D _playerCellOptions;//placeholder texture
        private SpriteFont _font;

        private int _tileWidth;
        private int _tileHeight;
        private Rectangle _backBackGroundButtonOptions = new Rectangle(1600, 720, 200, 100);
 
        private List<(Rectangle rect, SingleAttack attack)> _attackButtons = new();
        private Rectangle _summonRect, _attackRect, _endTurnRect, _moveRect;
        private Dictionary<ICombatant, Rectangle> _displayStatRectangles = new Dictionary<ICombatant, Rectangle>();
        private Rectangle _endScreenRect = new Rectangle(710, 440, 500, 200);
        private Rectangle _exitCombatButtonRect = new Rectangle(885, 580, 150, 50);

        public PlayMonsters PlayMonsters; // kept as reference as needed
        private List<TileCell> _playerSpawnableCells = new List<TileCell>();
        private List<TileCell> _monsterSpawnableCells = new List<TileCell>();
        private TileCell _statHoverCellHighlight;

        private TileCell _currentClickedCell;
        private TileCell _currentMouseHoverCell;
        private Vector2 _currentMousePos;

        private Dictionary<ICombatant, TileCell> _playerControlledMonsterMap = new();
        private Dictionary<ICombatant, TileCell> _aIControlledMonsterMap = new();
        public Dictionary<ICombatant, TileCell> AIControlledMonsterMap => _aIControlledMonsterMap;
        public Dictionary<ICombatant, TileCell> PlayerControlledMonsterMap => _playerControlledMonsterMap;

        private int? _numberOfCellsMoved = 0;

        private VisualEffect _currentAttackVisualEffect;
        public Dictionary<string, int> defeatedMonsters = new Dictionary<string, int>();

        private bool _attackComplete = false;
        private bool _attackPerformed = false;
        private List<TileCell> _summonSpawnableCells;
        private ICombatant _currentCombatant;
        public WhoWon TheWinner = WhoWon.None;

        private readonly Dictionary<MonsterActionOrder, Func<bool>> _actionExecutors;
        private readonly Dictionary<MonsterActionOrder, AITurnState> _actionStates;
        private float _timer = 0;
        private Player _currentPlayer => PlayerManager.CurrentPlayer;
        public ICombatant CurrentCombatant => _currentCombatant;


        public CombatManager(PlayMonsters playMonsters)
        {
            _stateMachine = new CombatStateMachine();
            _currentMapTile = TileManager.CurrentMapTile;
            _visualEffectManager = new VisualEffectManager();
            _playerCellOptions = AssetManager.GetTexture("fightBackground");
            _font = AssetManager.GetFont("mainFont");
            _tileHeight = MapTile.TileHeight;
            _tileWidth = MapTile.TileWidth;

            PlayMonsters = playMonsters;
            _currentPlayer.ToggleDrawn();
            _currentPlayer.ClearMovementPath();
            SetSpawnableCells();
            SetCombatantStartingPos();
            SetTurnOrder();
            UpdateCurrentMonster();
            InitilizeUIElements();
            _actionExecutors = CreateActionExecutorMap();
            _actionStates = CreateActionStateMap();
            

        }
        private Dictionary<MonsterActionOrder, Func<bool>> CreateActionExecutorMap()
        {
            // Every action a monster has, add it to the MonsterActionOrderENum.
            // Then, add it here along with the method it checks to see IF its possible to perform said action
            return new Dictionary<MonsterActionOrder, Func<bool>>
            {
        { MonsterActionOrder.MoveTowardsClosestEnemy, () => GetMovementCellPathToClosestEnemy() },
        { MonsterActionOrder.AttackClosestEnemy, () => AttackClosestEnemy() }
            };
        }
        private Dictionary<MonsterActionOrder, AITurnState> CreateActionStateMap()
        {
            return new Dictionary<MonsterActionOrder, AITurnState>
            {
        { MonsterActionOrder.MoveTowardsClosestEnemy, AITurnState.MovingAIControlled },
        { MonsterActionOrder.AttackClosestEnemy, AITurnState.AIAttacking }
            };
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
        public void SetPlayerTurnState(PlayerTurnState state)
        {
            _stateMachine.SetPlayerTurnState(state);
        }
        public void SetSummonedTurnState(SummonedTurnState state)
        {
            _stateMachine.SetSummonedTurnState(state);
        }
        public void SetWhoWon(WhoWon whoWon)
        {
            TheWinner = whoWon;
        }
        public void SetAITurnState(AITurnState state)
        {
            _stateMachine.SetAITurnState(state);
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

                    mon.CurrentStats.Pos = pos;
                    spawnableCells.RemoveAt(index);
                }

            } while (spawnableCells.Count < spawnableCells.Count - PlayMonsters.Monsters.Count);
        }
        private void InitilizeUIElements()
        {
            // Button dimensions and spacing
            int buttonHeight = _backBackGroundButtonOptions.Height;
            int spacing = 10;

            _endTurnRect = _backBackGroundButtonOptions;
            _attackRect = new Rectangle(
                _endTurnRect.X,
                _endTurnRect.Y - buttonHeight - spacing,
                _endTurnRect.Width,
                buttonHeight);
            _moveRect = new Rectangle(
                _attackRect.X,
                _attackRect.Y - buttonHeight - spacing,
                _endTurnRect.Width,
                buttonHeight);
            _summonRect = new Rectangle(
                _moveRect.X,
                _moveRect.Y - buttonHeight - spacing,
                _endTurnRect.Width,
                buttonHeight);

        }








        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            bool endingScreen = false;
            switch (StateCombat)
            {
                case CombatState.LocationSelection:
                    DrawLocationSelection(spriteBatch);
                    break;
                case CombatState.PlayerTurn:
                    DrawPlayerTurn(spriteBatch);
                    break;
                case CombatState.AITurn:

                    break;
                case CombatState.SummonedTurn:
                    DrawSummonedTurn(spriteBatch);
                    break;
                case CombatState.WinnerChosen:
                    endingScreen = true;
                    break;
            }
            DrawDebugInfo(spriteBatch);
            DrawDisplayStats(spriteBatch);
            DrawStatHoverHighlight(spriteBatch);
            _visualEffectManager.Draw(spriteBatch, _font);
            if (endingScreen) DrawCombatEndScreen(spriteBatch);

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

        public void DrawStatHoverHighlight(SpriteBatch spriteBatch)
        {
            if (_statHoverCellHighlight != null)
            {
                _statHoverCellHighlight.DrawCellHighlight(spriteBatch, ColorPalette.DarkColor);
            }
        }
        public void DrawDisplayStats(SpriteBatch spriteBatch)
        {
            int iconSize = 64;
            int spacingX = 150; // Horizontal space between icons
            int topY = 20; // Vertical offset from the top of the screen
            SpriteFont font = AssetManager.GetFont("mainFont");

            int count = _referenceTurnOrder.Count;
            int totalWidth = count * spacingX;

            // Center the row
            int screenWidth = ViewportManager.ScreenWidth; // Or use GraphicsDevice.Viewport.Width if you have access
            Vector2 startingPos = new Vector2((screenWidth - totalWidth) / 2f, topY);

            int index = 0;
            foreach (var mon in _referenceTurnOrder)
            {
                Vector2 iconPos = startingPos + new Vector2(index * spacingX, 0);
                Rectangle iconRect = new Rectangle((int)iconPos.X, (int)iconPos.Y, iconSize, iconSize);
                if (!_displayStatRectangles.TryGetValue(mon, out Rectangle existingRect) || existingRect != iconRect)
                {
                    _displayStatRectangles[mon] = iconRect;
                }
                // Determine texture key
                Texture2D icon = mon.Icon;

                // Set color depending on isDead
                Color col = Color.White;
                if (mon.Is == CombatMonsterType.Summoned)
                {
                    col = Color.White * 0.8f;
                }
                if (mon.isDead)
                    col = Color.Gray * 0.4f;

                if (mon == _currentCombatant) spriteBatch.Draw(_playerCellOptions, iconRect, col);
                // Draw monster icon
                spriteBatch.Draw(icon, iconRect, col);



                // Draw health below
                float currentHealth = MathF.Max(0, mon.CurrentStats.Health);
                string hpText = $"{currentHealth} / {mon.BaseStats.Health}";
                Vector2 textSize = font.MeasureString(hpText);
                Vector2 textPos = new Vector2(
                    iconRect.X + (iconSize - textSize.X) / 2,
                    iconRect.Bottom + 2
                );

                // Draw aspects below icon
                int aspectSize = 24;
                int aspectSpacing = 4;
                for (int i = 0; i < mon.Aspects.Count; i++)
                {
                    var aspect = mon.Aspects[i];
                    Vector2 aspectPos = new Vector2(
                        iconRect.X + i * (aspectSize + aspectSpacing),
                        iconRect.Bottom + 25
                    );
                    Rectangle aspectRect = new Rectangle((int)aspectPos.X, (int)aspectPos.Y, aspectSize, aspectSize);

                    spriteBatch.Draw(aspect.Icon, aspectRect, Color.White);

                    // Overlay duration
                    string turnsLeft = MathF.Ceiling(aspect.Duration).ToString();
                    Vector2 numSize = font.MeasureString(turnsLeft);
                    Vector2 numPos = new Vector2(
                        aspectRect.Center.X - numSize.X / 2,
                        aspectRect.Center.Y - numSize.Y / 2
                    );

                    spriteBatch.DrawString(font, turnsLeft, numPos, Color.Yellow);
                }

                spriteBatch.DrawString(font, hpText, textPos, Color.Black);
                // Draw MP below HP
                string mpText = $"MP: {mon.CurrentStats.MP} / {mon.CurrentStats.MP}";
                Vector2 mpTextSize = font.MeasureString(mpText);
                Vector2 mpTextPos = new Vector2(
                    iconRect.X + (iconSize - mpTextSize.X) / 2,
                    textPos.Y + textSize.Y + 2
                );
                spriteBatch.DrawString(font, mpText, mpTextPos, Color.Blue);
                // Draw AP below MP
                string apText = $"AP: {mon.CurrentStats.AP} / {mon.BaseStats.AP}";
                Vector2 apTextSize = font.MeasureString(apText);
                Vector2 apTextPos = new Vector2(
                    iconRect.X + (iconSize - apTextSize.X) / 2,
                    mpTextPos.Y + mpTextSize.Y + 2
                );
                spriteBatch.DrawString(font, apText, apTextPos, Color.Orange);

                index++;
            }
        }
        private void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            OnScreenDebug(spriteBatch);
            DrawTurnStateOverlay(spriteBatch);
        }
        private void DrawTurnStateOverlay(SpriteBatch spriteBatch)
        {
            string stateText = $"PlayerState: {StatePlayerTurn}";
            string combatStateText = $"CombatState: {StateCombat}";
            string summonedStateText = $"SummonedState: {StateSummoned}";
            string aiStateText = $"AIState: {StateAI}";
            SpriteFont font = AssetManager.GetFont("mainFont");
            Vector2 textSize = font.MeasureString(stateText);
            int screenWidth = ViewportManager.ScreenWidth;

            Vector2 position = new Vector2(screenWidth - textSize.X - 200, 10);
            spriteBatch.DrawString(font, stateText, position, Color.Orange);
            spriteBatch.DrawString(font, combatStateText, new Vector2(screenWidth - textSize.X - 200, 30), Color.Orange);
            spriteBatch.DrawString(font, summonedStateText, new Vector2(screenWidth - textSize.X - 200, 50), Color.Orange);
            spriteBatch.DrawString(font, aiStateText, new Vector2(screenWidth - textSize.X - 200, 70), Color.Orange);

        }
        private void DrawLocationSelection(SpriteBatch spriteBatch)
        {
            DrawSpawnableTiles(spriteBatch);

            if (_currentMouseHoverCell != null && _currentMouseHoverCell.PlayerSpawnable)
                DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, "Player", PlayerManager.CurrentPlayer);
        }
        private void DrawEntityPreviewOnCell(SpriteBatch spriteBatch, TileCell cell, string type, ICombatant mon = null)
        {
            if (mon == null) mon = _currentCombatant;
            if (type == "Summoned")
            {
                SummonedMonsterManager.DrawPreview(spriteBatch, cell, mon.CurrentStats.CurrentSelectedSummon);
                return;
            }
            mon.DrawEntityCellPreview(spriteBatch, cell);
        }
        private void DrawSpawnableTiles(SpriteBatch spriteBatch)
        {
            foreach (var tile in _playerSpawnableCells)
               tile.DrawCellHighlight(spriteBatch, Color.Green, 5);

            foreach (var tile in _monsterSpawnableCells)
                tile.DrawCellHighlight(spriteBatch, Color.Red, 5);
        }
        private void DrawPlayerTurn(SpriteBatch spriteBatch)
        {
            DrawPlayerButtonOptions(spriteBatch);
            switch (StatePlayerTurn)
            {
                case (PlayerTurnState.None):
                    break;
                case PlayerTurnState.PlayerClickedMoveButton:
                    DrawPlayerClickedMoveButton(spriteBatch);
                    break;
                case PlayerTurnState.PlayerClickedSummonButton:
                    DrawPlayClickedSummonButton(spriteBatch);
                    break;
   
                case PlayerTurnState.PlayerClickedSpecificSummoned:
                    DrawSummonSpawnLocationOptions(spriteBatch);
                    DrawSummonHover(spriteBatch);
                    break;


            }
        }
        private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string label)
        {
            spriteBatch.Draw(_playerCellOptions, rect, Color.Aqua);

            Vector2 textSize = _font.MeasureString(label);
            Vector2 textPosition = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2,
                rect.Y + (rect.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, label, textPosition, Color.White);
        }
        private void DrawPlayerButtonOptions(SpriteBatch spriteBatch)
        {
            DrawButton(spriteBatch, _moveRect, "Move");
            DrawButton(spriteBatch, _summonRect, "Summon");
            DrawButton(spriteBatch, _endTurnRect, "End Turn");

        }
        public void DrawPlayerClickedMoveButton(SpriteBatch spriteBatch)
        {
            ICombatant mon = _currentCombatant;
            if (_currentCombatant.CurrentStats.MP > 0)
            {
                foreach (var cell in mon.MoveableCells)
                {
                    if (cell.BlockedByMonster || !cell.IsWalkable) continue;
                    cell.DrawCellHighlight(spriteBatch, Color.Green, 5);
                }

                if (_currentMouseHoverCell != null && mon.MoveableCells.Contains(_currentMouseHoverCell))
                {
                    DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, "Player");
                }
            }
        }
        private void DrawPlayClickedSummonButton(SpriteBatch spriteBatch)
        {
            ICombatant mon = _currentCombatant;

            if (SummonedMonsterManager.UnlockedSummons != null && SummonedMonsterManager.UnlockedSummons.Count > 0)
            {
                int iteration = 0;
                foreach (var kvp in SummonedMonsterManager.UnlockedSummons)
                {
                    string name = kvp.Key;
                    SummonedSavedStats stats = kvp.Value;
                    Texture2D icon = stats.Icon;

                    Rectangle summonIconRect = new Rectangle(
                        _summonRect.X,
                        _summonRect.Y - ((iteration + 1) * (_summonOptionHeight + _summonOptionSpacing)),
                        _summonOptionWidth,
                        _summonOptionHeight
                    );

                    spriteBatch.Draw(icon, summonIconRect, Color.White);

                    // Optional: draw border or hover highlight
                    if (summonIconRect.Contains(_currentMousePos))
                        spriteBatch.Draw(_playerCellOptions, summonIconRect, Color.Yellow * 0.4f);
                    iteration++;
                }
                
            }
        }
        private void DrawSummonSpawnLocationOptions(SpriteBatch spriteBatch)
        {
           
            if (_currentCombatant.CurrentStats.CurrentSelectedSummon?.data == null) return;

            foreach (var cell in _summonSpawnableCells)
            {
                cell.DrawCellHighlight(spriteBatch, Color.LimeGreen, 5);
            }

        }
        private void DrawSummonHover(SpriteBatch spriteBatch)
        {
            if (!_summonSpawnableCells.Contains(_currentMouseHoverCell)) return;

            if (_currentMouseHoverCell != null && _summonSpawnableCells.Contains(_currentMouseHoverCell))
            {
                  DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, "Summoned" );
            }
        }



        private void DrawSummonedTurn(SpriteBatch spriteBatch)
        {
            DrawSummonedTurnButtons(spriteBatch);
            switch (StateSummoned)
            {
                case SummonedTurnState.SummonedWaitingInput:

                    break;
                case SummonedTurnState.SummonedClickedAttackButton:
                    DrawSummonedAttackOptions(spriteBatch);
                    break;
                case SummonedTurnState.SummonedChoosingTarget:
                    DrawSummonedAttackRangeOptions(spriteBatch);
                    break;
                case SummonedTurnState.SummonedClickedMoveButton:
                    DrawPlayerClickedMoveButton(spriteBatch);
                    break;

            }
        }
        private void DrawSummonedTurnButtons(SpriteBatch spriteBatch)
        {
            DrawButton(spriteBatch, _moveRect, "Move");
            DrawButton(spriteBatch, _attackRect, "Attack");
            DrawButton(spriteBatch, _endTurnRect, "End Turn");
        }
        private void DrawSummonedAttackOptions(SpriteBatch spriteBatch)
        {
            var mon = _currentCombatant;
            if (mon == null || mon.Attacks == null || mon.Attacks.Count == 0)
                return;

            _attackButtons.Clear(); // Clear previous frame's buttons

            int buttonWidth = 200;
            int buttonHeight = 50;
            int spacing = 10;
            int totalWidth = mon.Attacks.Count * (buttonWidth + spacing) - spacing;
            int screenWidth = 1920;
            int startX = (screenWidth - totalWidth) / 2;
            int bottomY = 1080 - buttonHeight - 40;

            for (int i = 0; i < mon.Attacks.Count; i++)
            {
                var attack = mon.Attacks[i];
                Rectangle buttonRect = new Rectangle(
                    startX + i * (buttonWidth + spacing),
                    bottomY,
                    buttonWidth,
                    buttonHeight
                );

                _attackButtons.Add((buttonRect, attack)); // 🧠 Save for click detection

                spriteBatch.Draw(_playerCellOptions, buttonRect, Color.DarkSlateGray);

                string attackName = attack.Name.ToString();
                if (attack.ElementDamage != ElementType.None) 
                {
                    string element = attack.ElementDamage.ToString();
                    attackName = $"{element} {attackName}";
                }
                Vector2 textSize = _font.MeasureString(attackName);
                Vector2 textPos = new Vector2(
                    buttonRect.X + (buttonRect.Width - textSize.X) / 2,
                    buttonRect.Y + (buttonRect.Height - textSize.Y) / 2
                );

                spriteBatch.DrawString(_font, attackName, textPos, Color.White);
            }
        }
        private void DrawSummonedAttackRangeOptions(SpriteBatch spriteBatch)
        {
            var mon = _currentCombatant;

            if (mon.CurrentStats.Attack != null)
            {
                foreach (var cell in mon.CurrentStats.AttackRange)
                {
                    Color col = Color.Red * 5f;
                    if (AIControlledMonsterMap.ContainsValue(cell) ) { col = Color.Green * 5f; }
                    cell.DrawCellHighlight(spriteBatch, col, 5);
                }
            }

        }










        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateMouseWhereabouts();
            UpdateInput(gameTime, delta);
            if (StateCombat == CombatState.WinnerChosen)
            {
                return;
            }
            _visualEffectManager.Update(delta);
            UpdateMonsterTakingDamage(delta);
            ToggleIsDead(); // toggles ISDead as well as clears aspects
            UpdateCurrentMonster();
            UpdateMonsterCellMap();
            if (WinnerChosen()) 
            { 
                SetCombatState(CombatState.WinnerChosen);
                CountDefeatedMonsters();
                return; 
            }

                switch (StateCombat)
            {
                case CombatState.TurnStart:
                    SkipMonsterIfDead(); // dequees and requeues monster if dead
                    UpdateMonsterTopOfRoundStats();
                    PickWhichEntitiesTurn();
                    break;
                case CombatState.AITurn:
                    
                    switch (StateAI)
                    {
                        case AITurnState.ActionNavigation:
                            SetTopOfActionChoiceStats();
                            if (CheckIfAIShouldEndTurn()) return;
                            if (!DecideAINextAction()) EndTurn();
                            break;
                        case AITurnState.MovingAIControlled:
                            
                            SetAITurnState(AITurnState.ExecutingMove);
                            break;
                        case AITurnState.ExecutingMove:
                            if (MonsterFinishedMoving()) { 
                                FinishedMoving(); AIActionNavigation(); }
                            break;
                        case AITurnState.AIAttacking:
                            SetAITurnState(AITurnState.ExecutingAttack);
                            if (AICanAttack()) _attackComplete = false;

                            break;
                        case AITurnState.ExecutingAttack:
                            if (_attackComplete) {
                                AIActionNavigation(); return; }
                            WaitForAttackToFinish(delta);
                            break;
                    }
                    break;
                case CombatState.SummonedTurn:
                    UpdatePlayerMoveableCells();
                    switch (StateSummoned)
                    {
                        case SummonedTurnState.SummonedWaitingInput:

                            break;
                        case SummonedTurnState.SummonedExecutingAttack:
                            if (!PlayerHasMovePath()) WaitForAttackToFinish(delta);
                            break;
                        case SummonedTurnState.SummonedClickedMoveButton:
                            if (PlayerHasMovePath()) SetSummonedTurnState(SummonedTurnState.SummonedExecutingMove);
                            break;
                            case SummonedTurnState.SummonedExecutingMove:
                            if (!PlayerHasMovePath()) { FinishedMoving(); SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput); }
                            break;
                    }
                    break;
                case CombatState.PlayerTurn:
                    UpdatePlayerMoveableCells();
                    switch (StatePlayerTurn)
                    {
                        case PlayerTurnState.PlayerClickedMoveButton:
                            if (PlayerHasMovePath()) {SetPlayerTurnState(PlayerTurnState.PlayerExecutingMove); SpendActionPoint();}
                            break;
                        case PlayerTurnState.PlayerExecutingMove:
                            if (!PlayerHasMovePath()) { FinishedMoving(); SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput); } 
                            break;
                        case PlayerTurnState.PlayerExecutingAttack:
                            if (!PlayerHasMovePath()) WaitForAttackToFinish(delta);
                            break;
                        case PlayerTurnState.PlayerExecutingSummoning:
                            _timer += delta;
                            if (_timer >= 1f) SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
                            break;
                        case PlayerTurnState.PlayerClickedSpecificSummoned:
                            GeneratePlayerSummonRange();
                            break;


                    }
                    break;
                case CombatState.ResolvingEndOfTurnEffects:
                    if (_timer == 0) ResolveAspects(TickedTiming.EndOfTurn);
                    _timer += delta;
                    if (_timer >= 1f) LeaveResolvingEndOfTurnEffects();
                    break;
                case CombatState.ResolvingStartOfTurnEffects:
                    if (_timer == 0) ResolveAspects(TickedTiming.StartOfTurn);
                    _timer += delta;
                    if (_timer >= 1f) LeaveResolvingStartOfTurnEffects();
                    break;
                    
            }
        }
        private bool WinnerChosen()
        {
            if (AIControlledMonsterMap.Count == 0) { SetWhoWon(WhoWon.Player); return true; }
            if (_currentPlayer.isDead){ SetWhoWon(WhoWon.Monster);  return true; }
            return false;
        }
        private void FinishedMoving()
        {
            _numberOfCellsMoved = 0;
        }
        private void LeaveResolvingStartOfTurnEffects()
        {
            _timer = 0;
            SetCombatState(CombatState.TurnStart);
        }
        private void LeaveResolvingEndOfTurnEffects()
        {
            _timer = 0;
            SendMonsterToBackOfQueue();
            SetCombatState(CombatState.ResolvingStartOfTurnEffects);
        }
        private void GeneratePlayerSummonRange()
        {
            
            ICombatant combatant = _currentCombatant;
            
            // Later put logic here to decide how far away the palyer can summon a monster
            float range = 2f;

            TileCell origin = _playerControlledMonsterMap[combatant];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)range);

            _summonSpawnableCells = cells;

        }
        public bool CheckIfAIShouldEndTurn()
        {
             
            if (_currentCombatant.CurrentStats.AP <= 0)
            {
                EndTurn();
                return true;
            }
            return false;
        }
        private void SpendActionPoint()
        {
            _currentCombatant.CurrentStats.AP -= 1;
        }
        private void SetTopOfActionChoiceStats()
        {
            ICombatant mon = _currentCombatant;
            mon.CurrentStats.MP = mon.BaseStats.MP;
            mon.CurrentStats.ChooseWhichAttack.Clear();
            mon.CurrentStats.ActionOrder.Clear();
            if (mon.Is == CombatMonsterType.AI)
            {
                foreach (var str in mon.BaseStats.DecideWhichAttack)
                {
                    mon.CurrentStats.ChooseWhichAttack.Enqueue(str);
                }
                foreach (var str in mon.BaseStats.ActionOrder)
                {
                    mon.CurrentStats.ActionOrder.Enqueue(str);
                }
            }
        }
        private void EndTurn()
        {
            ResetAllStatesToNone();
            SetCombatState(CombatState.ResolvingEndOfTurnEffects);

        }
        public void ResetAllStatesToNone()
        {
            SetPlayerTurnState(PlayerTurnState.None);
            SetAITurnState(AITurnState.None);
            SetSummonedTurnState(SummonedTurnState.None);
            SetCombatState(CombatState.None);
        }
        public bool DecideAINextAction()
        {
            ICombatant mon = _currentCombatant;

            while (mon.CurrentStats.ActionOrder.Count > 0)
            {
                var action = mon.CurrentStats.ActionOrder.Peek();

                bool success = _actionExecutors.TryGetValue(action, out var executor) && executor();
                if (success)
                {
                    SpendActionPoint();
                    SetAITurnState(_actionStates[action]);
                    return true;
                }
                else
                {
                    TryNextAction();
                }
            } 
            return false;
        }


        private void TryNextAction()
        {
            ICombatant mon = _currentCombatant;
            mon.CurrentStats.ActionOrder.Dequeue();
        }
        private bool AttackClosestEnemy()
        {
            ICombatant mon = _currentCombatant;
            TileCell origin = _aIControlledMonsterMap[mon];

            //inRangeMap send any and all attacks that have a valid range, including non monster cells
            var inRangeMap = GetInRangeCellsByAttack(mon.Attacks, origin);
            if (inRangeMap.Count == 0)
            {
                return false;
            }
            // Choose targets for each attack using the targeting strategy
            var targetData = new Dictionary<SingleAttack, Dictionary<ICombatant, List<TileCell>>>();

            foreach (var pair in inRangeMap)
            {
                pair.Value.Remove(origin);
                var (target, affectedCells) = AttackManager.TargetClosestEnemy(pair.Value, origin);
                if (target != null && affectedCells.Count > 0)
                {
                    targetData[pair.Key] = new Dictionary<ICombatant, List<TileCell>>
            {
                { target, affectedCells }
            };
                }
            }

            // Use the selected targeting behavior to pick the final attack
            var (chosenAttack, chosenMap) = AttackManager.ChooseWhichAttack(targetData, origin, mon.CurrentStats.ChooseWhichAttack);

            SetMonsterAttackPathingInformation(
                chosenAttack,
                chosenMap.Keys.ToList(),
                chosenMap.Values.SelectMany(x => x).ToList()
            );
            return true;
        }

        private void SetMonsterAttackPathingInformation(SingleAttack att, List<ICombatant> mons, List<TileCell> cells)
        {
            // this method will set the monster property of attack path(s), current attacks, specified current target info, and visualeffect
            ICombatant mon = _currentCombatant;
            var attackDetails = (att, mons, cells);
            if (attackDetails.Item1 == null)
            {
                AIActionNavigation();
                return;
            }
            TileCell currentCell = GetCombatantCurrentCell(mon);

            TileCell centerCell = FindCenterCell(attackDetails.Item3);
            List<Vector2> path = NPCMovement.GetMovementPatternVector2List(mon.DrawSpecifics.MovementPattern, currentCell, centerCell);
            var paths = Movement.CombatGrid.GridMovement.SplitAttackPath(path, attackDetails.Item1);
            mon.CurrentStats.AttackPath1 = paths.Item1;
            mon.CurrentStats.AttackPath2 = paths.Item2;
            mon.CurrentStats.Attack = attackDetails.Item1;
            mon.CurrentStats.AttackEffectedCombatants = attackDetails.Item2;
            mon.CurrentStats.AttackEffectedCells = attackDetails.Item3;
            if (mon.CurrentStats.Attack.Animated)
            {
                _currentAttackVisualEffect = new VisualEffect(GetCombatantCurrentCell(mon), mon.CurrentStats.Attack, centerCell);
            }


        }

        private Dictionary<SingleAttack, List<TileCell>> GetInRangeCellsByAttack(List<SingleAttack> attacks, TileCell origin)
        {
            Dictionary<SingleAttack, List<TileCell>> inRangeMap = new();

            foreach (var attack in attacks)
            {
                List<TileCell> inRangeCells = TileManager.GetCellsInRange(origin, attack.Range);
                if (inRangeCells != null && inRangeCells.Count > 0)
                {
                    inRangeMap[attack] = inRangeCells;
                }
            }

            return inRangeMap;
        }
        private bool GetMovementCellPathToClosestEnemy()
        {
            if (AIHasMP())
            {
                ICombatant mon = _currentCombatant;
                TileCell currentCell = _aIControlledMonsterMap[mon];

                List<TileCell> playerControlledCells = _playerControlledMonsterMap
                     .Select(pair => pair.Value)
                     .Where(cell => cell != null)
                     .ToList();

                // If already adjacent, return current position
                if (TileManager.IsNeighbor(playerControlledCells, currentCell))
                    return false;

                List<TileCell> listOfCellsPathToTarget = GridMovement.FindClosestPlayerControlledCell(currentCell, playerControlledCells, (int)mon.CurrentStats.MP);
                if (listOfCellsPathToTarget.Count <= 0) return false;
                GenerateMovementPath(listOfCellsPathToTarget);

            }
            else return false;
            return true;
        }
        private void GenerateMovementPath(List<TileCell> tileCellPath)
        {
            ICombatant combatant = _currentCombatant;
            _numberOfCellsMoved = tileCellPath.Count;
            List<Vector2> fullVectorPath = new();
            TileCell startingCell = GetCombatantCurrentCell(combatant);

            foreach (var endPos in tileCellPath)
            {
                List<Vector2> arc = NPCMovement.GetMovementPatternVector2List(combatant.DrawSpecifics.MovementPattern, startingCell, endPos);
                fullVectorPath.AddRange(arc);
                startingCell = endPos;

            }

            combatant.CurrentStats.MovePath = fullVectorPath;

        }
        public bool AIHasMP() => _currentCombatant.CurrentStats.MP >= 0;
        public bool MonsterFinishedMoving() =>  _currentCombatant.CurrentStats.MovePath == null || _currentCombatant.CurrentStats.MovePath.Count <= 0;
        public bool AICanAttack() => _currentCombatant.CurrentStats.Attack != null;
        public bool PlayerHasEndPoint() => _currentCombatant.CurrentStats.MovementEndPoint != null;
        public bool PlayerHasMovePath() => _currentCombatant.CurrentStats.MovePath.Count > 0;
        private void AIActionNavigation()
        {
            SetAITurnState(AITurnState.ActionNavigation);
        }
        private void AIFinishedAttack()
        {
            ICombatant mon = _currentCombatant;
            if (mon.CurrentStats.MovePath == null || mon.CurrentStats.MovePath.Count <= 0)
            {
                _attackComplete = true;
                _attackPerformed = false;
            }
        }
        private void WaitForAttackToFinish(float delta)
        {
            ICombatant mon = _currentCombatant;

            // 🔄 Visual Effect Handling (returns true if we should pause execution)
            if (HandleAttackVisualEffect())
                return;

            if (mon.CurrentStats.AttackPath1 != null && mon.CurrentStats.AttackPath1.Count > 0)
            {
                mon.CurrentStats.MovePath = mon.CurrentStats.AttackPath1;
                mon.CurrentStats.AttackPath1 = null;
                switch (mon.Is)
                {
                    case CombatMonsterType.Player:
                        SetPlayerTurnState(PlayerTurnState.PlayerExecutingAttack);
                        break;
                    case CombatMonsterType.Summoned:
                        if (StateSummoned != SummonedTurnState.SummonedExecutingAttack)
                            SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack);
                        break;
                }
                return;

            }
            else if (!_attackPerformed)
            {
                _attackPerformed = true;
                AttackManager.PerformAttack(mon.CurrentStats.Attack, mon, mon.CurrentStats.AttackEffectedCombatants, mon.CurrentStats.AttackEffectedCells);

                mon.CurrentStats.Attack = null;
                mon.CurrentStats.AttackEffectedCombatants = null;
                mon.CurrentStats.AttackEffectedCells = null;

                // 👇 Handle "AfterAttack" visuals here
                if (_currentAttackVisualEffect != null && _currentAttackVisualEffect.WhenToStart == VisualTiming.AfterAttack)
                {
                    VisualEffectManager.AddEffect(_currentAttackVisualEffect);
                    _currentAttackVisualEffect.WhenToStart = VisualTiming.IsRunning;
                    return;
                }

                return;
            }
            else if (mon.CurrentStats.AttackPath2 != null && mon.CurrentStats.AttackPath2.Count > 0)
            {
                mon.CurrentStats.MovePath = mon.CurrentStats.AttackPath2;
                mon.CurrentStats.AttackPath2 = null;
                if (StateCombat == CombatState.PlayerTurn)
                {
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingAttack);
                }
                if (StateCombat == CombatState.SummonedTurn) { SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack); }
                return;
            }
            //attack and movement associated it is finished, so go to next turn
            switch (mon.Is)
            {
                case CombatMonsterType.Player:
                    SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
                    break;
                    case CombatMonsterType.Summoned:
                    SummonedFinishedAttack();
                    break;
                case CombatMonsterType.AI:
                    AIFinishedAttack();
                    break;
            }       


        }
        private void SummonedFinishedAttack()
        {
            _attackComplete = true;
            _attackPerformed = false;
            SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput);
        }
        private bool HandleAttackVisualEffect()
        {
            if (_currentAttackVisualEffect == null)
                return false;

            switch (_currentAttackVisualEffect.WhenToStart)
            {
                case VisualTiming.BeforeAttack:

                    _visualEffectManager.AddEffect(_currentAttackVisualEffect);
                    _currentAttackVisualEffect.WhenToStart = VisualTiming.IsRunning;
                    return true; // Hold until finished

                case VisualTiming.DuringAttack:
                    var mon = _currentCombatant;
                    if (!_attackComplete && (mon.CurrentStats.AttackPath1 == null || mon.CurrentStats.AttackPath1.Count <= 0))
                    {
                        _visualEffectManager.AddEffect(_currentAttackVisualEffect);
                        _currentAttackVisualEffect = null;
                    }
                    return false;

                case VisualTiming.AfterAttack:
                    return false; // We'll handle this after the attack completes

                case VisualTiming.IsRunning:
                    if (!_currentAttackVisualEffect.IsFinished)
                        return true;
                    else
                        _currentAttackVisualEffect.WhenToStart = VisualTiming.Complete;
                    return true;

                case VisualTiming.Complete:
                    _currentAttackVisualEffect = null;
                    return false;
            }

            return false;
        }




        public void UpdateInput(GameTime gameTime, float delta)
        {
      
            switch (StateCombat)
            {
                case CombatState.LocationSelection:
                    HandleLocationSelectionInput();
                    break;

                case CombatState.PlayerTurn:
                    HandlePlayerTurnInput(delta);
                    break;
                case CombatState.SummonedTurn:
                    HandleSummonedTurnInput(delta);
                    break;

                    case CombatState.AITurn:

                    break;

                case CombatState.WinnerChosen:
                    HandlePlayerClickLeaveCombat();
                    break;
            }

        }
        private void HandlePlayerClickLeaveCombat()
        {
            if (InputManager.IsLeftClick() && _exitCombatButtonRect.Contains(_currentMousePos))
            {
                CombatGuard.EndCombat();
            }
        }
        private void HandleSummonedTurnInput(float delta)
        {
            ICombatant mon = _currentCombatant;
            if (StateSummoned is not (SummonedTurnState.SummonedExecutingMove or SummonedTurnState.SummonedExecutingAttack))
            {
                UpdateStatHoverMonsterCell();
                if (InputManager.IsRightClick()) SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput);
            }

            switch (StateSummoned)
            {
                case SummonedTurnState.SummonedWaitingInput:
                    HandlePlayerEndTurn();
                    if (mon.CurrentStats.AP > 0)
                        HandleMovementRectClick();
                    HandleAttackRectClick();
                    ResetClickValues();
                    break;
                case SummonedTurnState.SummonedClickedAttackButton:
                    if (mon.CurrentStats.AP > 0)
                        HandlePlayerSelectingSpecificAttackAndItsRange();
                    break;

                case SummonedTurnState.SummonedChoosingTarget:
                    HandleSummonedTargetingAttackClick();
                    break;
                case SummonedTurnState.SummonedExecutingAttack:

                    break;
                case SummonedTurnState.SummonedClickedMoveButton:
                    HandleMovementRectClick();
                    UpdatePlayerClickedMoveDestination();

                    break;
                   
            }
        }
        private void HandlePlayerTurnInput(float delta)
        {
            ICombatant mon = _currentCombatant;
            if (StatePlayerTurn is not (PlayerTurnState.PlayerExecutingMove or PlayerTurnState.PlayerExecutingAttack or PlayerTurnState.PlayerExecutingSummoning))
            {
                UpdateStatHoverMonsterCell();
                if (InputManager.IsRightClick()) SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
            }

            switch (StatePlayerTurn)
            {
                case PlayerTurnState.PlayerWaitingInput:
                    HandleSummonRectClick();
                    HandlePlayerEndTurn();
                    if (mon.CurrentStats.AP > 0)
                        HandleMovementRectClick();
                    ResetClickValues();
                    break;

                case PlayerTurnState.PlayerClickedSummonButton:
                    if (mon.CurrentStats.AP > 0)
                    HandleSummonOptionsClick();
                    break;
                case PlayerTurnState.PlayerClickedSpecificSummoned:
                    HandlePlayerChooseSummonedCell();
                    break;
                case PlayerTurnState.PlayerClickedMoveButton:
                    HandleMovementRectClick();
                    UpdatePlayerClickedMoveDestination();
                    break;
                 
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
                    _statHoverCellHighlight = GetCombatantCurrentCell(mon);
                    return;
                }
            }
            _statHoverCellHighlight = null;
        }
        private void HandlePlayerChooseSummonedCell()
        {
            ICombatant combatant = _currentCombatant;
            TileCell cell = _currentClickedCell;
            if (combatant.CurrentStats.CurrentSelectedSummon != null && _summonSpawnableCells.Contains(cell))
            { 
                    SummonSummonMonster(cell);
                combatant.CurrentStats.CurrentSelectedSummon = null;
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingSummoning);
                SpendActionPoint();
                
            }
        }
        private void ResetClickValues()
        {
            ICombatant mon = _currentCombatant;
            mon.CurrentStats.AttackEffectedCells = null;
            mon.CurrentStats.AttackEffectedCombatants = null;
            mon.CurrentStats.AttackPath1 = null;
            mon.CurrentStats.AttackPath2 = null;
            mon.CurrentStats.CurrentSelectedSummon = null;
            mon.CurrentStats.Attack = null;
            mon.CurrentStats.AttackRange = null;

        }
        private void HandlePlayerSelectingSpecificAttackAndItsRange()
        {
            ICombatant combatant = _currentCombatant;
            foreach (var (rect, attack) in _attackButtons)
            {
                if (InputManager.IsLeftClick() && rect.Contains(_currentMousePos))
                {
                    combatant.CurrentStats.Attack = attack;
                    combatant.CurrentStats.AttackRange = TileManager.GetFloodFillTileWithinRange(GetCombatantCurrentCell(combatant), combatant.CurrentStats.Attack.Range, includeMonsterTiles: true);
                    SpendActionPoint();
                    SetSummonedTurnState(SummonedTurnState.SummonedChoosingTarget);
                }
            }
        }
        private void UpdatePlayerClickedMoveDestination()
        {
            
            if (_currentCombatant.MoveableCells.Contains(_currentClickedCell))
            {
                ICombatant combatant = _currentCombatant;
                combatant.MoveTarget = _currentClickedCell.CenterPoint;
            }

        }
        private void UpdatePlayerMoveableCells()
        {
            ICombatant combatant = _currentCombatant;
            TileCell origin = _playerControlledMonsterMap[combatant];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)combatant.CurrentStats.MP);

            combatant.MoveableCells = cells;
        }
        private void HandleLocationSelectionInput()
        {
            if (_playerSpawnableCells.Contains(_currentMouseHoverCell) && InputManager.IsLeftClick())
            {
                _currentPlayer.CurrentStats.Pos = _currentMouseHoverCell.CenterPoint;
                _currentPlayer.ToggleDrawn();
                SetCombatState(CombatState.TurnStart);

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

            foreach (var mon in TurnOrder)
            {
                TileCell cell = TileManager.GetCell(mon.CurrentStats.Pos);
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
        private void UpdateMonsterTopOfRoundStats()
        {
            ICombatant mon = _currentCombatant;
            mon.CurrentStats.AP = mon.BaseStats.AP;
           
        }
        private void PickWhichEntitiesTurn()
        {
            ICombatant mon = _currentCombatant ;
            switch (mon.Is)
            {
                case CombatMonsterType.Player:
                    SetCombatState(CombatState.PlayerTurn);
                    SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
                    mon.CurrentStats.MovementEndPoint = null;
                    return;
                case CombatMonsterType.Summoned:
                    SetCombatState(CombatState.SummonedTurn);
                    SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput);
                    mon.CurrentStats.MovementEndPoint = null;
                    return;
                case CombatMonsterType.AI:
                    SetCombatState(CombatState.AITurn);
                    SetAITurnState(AITurnState.ActionNavigation);
                    return;
            }
            SetCombatState(CombatState.Debug);
           
}

     
      
     







        private void UpdateMonsterTakingDamage(float delta)
        {
            foreach (var mon in TurnOrder)
            {
                if (mon.DrawSpecifics.IsFlashingRed)
                {
                    mon.DrawSpecifics.DamageFlashTimer -= delta;
                    if (mon.DrawSpecifics.DamageFlashTimer <= 0f)
                    {
                        mon.DrawSpecifics.IsFlashingRed = false;
                    }
                }
            }

        }
        private void ResolveAspects(TickedTiming tick)
        {
            ICombatant mon = _currentCombatant;
            if (mon.Aspects == null || mon.Aspects.Count == 0)
            {
                return;
            }
            AspectManager.ResolveAspect(mon, tick);


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
        }
        private void RebuildTurnOrderExcludingDead()
        {
            Queue<ICombatant> newQueue = new Queue<ICombatant>();

            foreach (var mon in TurnOrder)
            {
                if (!mon.isDead)
                {
                    newQueue.Enqueue(mon);
                }
            }

            TurnOrder = newQueue;
        }

        private void SkipMonsterIfDead()
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



        private void HandleSummonedTargetingAttackClick()
        {
            ICombatant combatant = _currentCombatant;
            if (InputManager.IsLeftClick() && combatant.CurrentStats.AttackRange.Contains(_currentMouseHoverCell) && _currentMouseHoverCell.BlockedByMonster && !_playerControlledMonsterMap.ContainsValue(_currentMouseHoverCell))
            {
                SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack);
                TileCell currentTarget = _currentMouseHoverCell;
                _attackComplete = false;
                SetPlayerAttackEffectCellsAndMonsters(currentTarget);
                SetAttackPathForPlayer();
                if (combatant.CurrentStats.Attack.Animated)
                    _currentAttackVisualEffect = new VisualEffect(GetCombatantCurrentCell(combatant), combatant.CurrentStats.Attack, FindCenterCell(combatant.CurrentStats.AttackEffectedCells));

            }
        }
        private void SetPlayerAttackEffectCellsAndMonsters(TileCell targetCell)
        {
            ICombatant combatant = _currentCombatant;
            SingleAttack att = combatant.CurrentStats.Attack;
            TileCell target = targetCell;

            combatant.CurrentStats.AttackEffectedCells = new List<TileCell> { target };
            combatant.CurrentStats.AttackEffectedCombatants = new List<ICombatant>();

            foreach (var kvp in _aIControlledMonsterMap)
            {
                ICombatant aiMon = kvp.Key;
                TileCell aiCell = kvp.Value;

                if (combatant.CurrentStats.AttackEffectedCells.Contains(aiCell))
                {
                    combatant.CurrentStats.AttackEffectedCombatants.Add(aiMon);
                }
            }
        }
        private void SetAttackPathForPlayer()
        {
            ICombatant mon = _currentCombatant;
            List<Vector2> path = NPCMovement.GetMovementPatternVector2List(mon.DrawSpecifics.MovementPattern, _playerControlledMonsterMap[mon], FindCenterCell(mon.CurrentStats.AttackEffectedCells));
            var paths = GridMovement.SplitAttackPath(path, mon.CurrentStats.Attack);
            mon.CurrentStats.AttackPath1 = paths.Item1;
            mon.CurrentStats.AttackPath2 = paths.Item2;

        }







        private void HandleAttackRectClick()
        {
            if (InputManager.IsLeftClick() && _attackRect.Contains(_currentMousePos))
            {
                SetSummonedTurnState(SummonedTurnState.SummonedClickedAttackButton);
            }
        }
        private void HandleMovementRectClick()
        {
            if (InputManager.IsLeftClick() && _moveRect.Contains(_currentMousePos))
            {
                switch (StateCombat)
                {
                    case (CombatState.PlayerTurn):
                        SetPlayerTurnState(StatePlayerTurn == PlayerTurnState.PlayerClickedMoveButton
                      ? PlayerTurnState.PlayerWaitingInput
                      : PlayerTurnState.PlayerClickedMoveButton);
                        break;
                    case (CombatState.SummonedTurn):
                        SetSummonedTurnState(StateSummoned == SummonedTurnState.SummonedClickedMoveButton
                      ? SummonedTurnState.SummonedWaitingInput
                      : SummonedTurnState.SummonedClickedMoveButton);
                        break;
                }
            }
        }
        private void HandleSummonOptionsClick()
        {
            if (SummonedMonsterManager.UnlockedSummons == null || SummonedMonsterManager.UnlockedSummons.Count == 0)
                return;
            int iteration = 0;
            foreach  (var kvp in SummonedMonsterManager.UnlockedSummons)
            {
                string name = kvp.Key;  
                SummonedSavedStats stats = kvp.Value;

                Rectangle summonIconRect = new Rectangle(
                    _summonRect.X,
                    _summonRect.Y - ((iteration + 1) * (_summonOptionHeight + _summonOptionSpacing)),
                    _summonOptionWidth,
                    _summonOptionHeight
                );

                if (InputManager.IsLeftClick() && summonIconRect.Contains(_currentMousePos))
                {
                    _currentCombatant.CurrentStats.CurrentSelectedSummon = (name, stats);
                    SetPlayerTurnState(PlayerTurnState.PlayerClickedSpecificSummoned);
                    return; // Exit early — don’t try to summon yet
                }
                iteration++;
            }


        }
        private void HandlePlayerEndTurn()
        {
            if (_endTurnRect.Contains(_currentMousePos) && InputManager.IsLeftClick())
            {
                EndTurn();
            }
        }

      

        public void SummonSummonMonster(TileCell cell)
        {
            ICombatant combatant = _currentCombatant;
            SetPlayerTurnState(PlayerTurnState.PlayerExecutingSummoning);

            ICombatant comSumMon = (CombatMonsterManager.SummonMonsterToCombat(combatant.CurrentStats.CurrentSelectedSummon?.name));
            comSumMon.CurrentStats.Pos = cell.CenterPoint;
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
        }


        private void HandleSummonRectClick()
        {
            if (_summonRect.Contains(_currentMousePos) && InputManager.IsLeftClick())
                SetPlayerTurnState(StatePlayerTurn == PlayerTurnState.PlayerClickedSummonButton 
              ? PlayerTurnState.PlayerWaitingInput 
              : PlayerTurnState.PlayerClickedSummonButton);

        }
        private TileCell GetCombatantCurrentCell(ICombatant mon)
        {
            TileCell startCell;
            if (_playerControlledMonsterMap.ContainsKey(mon))
            {
                startCell = _playerControlledMonsterMap[mon];
            }
            else if (_aIControlledMonsterMap.ContainsKey(mon))
            {
                startCell = _aIControlledMonsterMap[mon];
            }
            else return null;
            return startCell;
        }
        private TileCell FindCenterCell(List<TileCell> cells)
        {
            if (cells.Count == 0) return null;
            if (cells.Count == 1) return cells[0];
            return cells[0];
        }
        public  List<TileCell> GetPathToPlayerSelectedCell(TileCell start, TileCell destination)
        {
            return GridMovement.FindPath(start, destination, int.MaxValue); // or -1 if your method supports it
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


        public  void Add(string message)
        {
            _log.Add(message);

            // Keep it from growing forever
            if (_log.Count > _maxStrings)
                 _log.RemoveAt(0);
        }
        private void OnScreenDebug(SpriteBatch spriteBatch)
        {
            Vector2 startPos = new Vector2(10, 10);
            int lineHeight = 18;

            // Calculate width & height of background box
            int maxWidth = _log.Any() ? _log.Max(line => (int)_font.MeasureString(line).X) : 0;
            int boxHeight = lineHeight * _log.Count + 10;
            Rectangle backgroundRect = new Rectangle((int)startPos.X - 5, (int)startPos.Y - 5, maxWidth + 10, boxHeight);

            // ✅ Draw black background
            spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), backgroundRect, Color.Black);

            for (int i = 0; i < _log.Count; i++)
            {
                Vector2 pos = startPos + new Vector2(0, i * lineHeight);
                string text = _log[i];

                // ✅ Draw "stroke" outline effect by drawing text offset in all directions
                spriteBatch.DrawString(_font, text, pos + new Vector2(-1, -1), Color.Black);
                spriteBatch.DrawString(_font, text, pos + new Vector2(1, -1), Color.Black);
                spriteBatch.DrawString(_font, text, pos + new Vector2(-1, 1), Color.Black);
                spriteBatch.DrawString(_font, text, pos + new Vector2(1, 1), Color.Black);

                // ✅ Then draw main white text
                spriteBatch.DrawString(_font, text, pos, Color.White);
            }
        }

    }
}
public enum WhoWon
{
    None,
    Player,
    Monster
}

