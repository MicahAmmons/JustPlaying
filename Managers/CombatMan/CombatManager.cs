using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Monster.SummonedMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
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
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using static CombatStateMachine;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatManager
    {

        public CombatMonster PlayerMonster; // Need to update _player at the end of combat accordingly






        private static List<TileCell> _playerMoveableCells = new List<TileCell>();
        private static List<CombatMonster> _summonedMonsters = new List<CombatMonster>();

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
        private List<CombatMonster> _referenceTurnOrder = new List<CombatMonster>();
        public Queue<CombatMonster> _turnOrder = new Queue<CombatMonster>();

        private MapTile _currentMapTile;
        private Texture2D _playerCellOptions;//placeholder texture
        private Texture2D _diamondHighlight;
        private SpriteFont _font;

        private int _tileWidth;
        private int _tileHeight;
        private Rectangle _backBackGroundButtonOptions = new Rectangle(1600, 720, 200, 100);
 
        private List<(Rectangle rect, SingleAttack attack)> _attackButtons = new();
        private Rectangle _summonRect, _attackRect, _endTurnRect, _moveRect;
        private Dictionary<CombatMonster, Rectangle> _displayStatRectangles = new Dictionary<CombatMonster, Rectangle>();
        private Rectangle _endScreenRect = new Rectangle(710, 440, 500, 200);
        private Rectangle _exitCombatButtonRect = new Rectangle(885, 580, 150, 50);

        public PlayMonsters PlayMonsters; // kept as reference as needed
        private Player _player; // reference of player to update stats at end
        private List<TileCell> _playerSpawnableCells = new List<TileCell>();
        private List<TileCell> _monsterSpawnableCells = new List<TileCell>();
        private TileCell _statHoverCellHighlight;

        private TileCell _currentClickedCell;
        private TileCell _currentMouseHoverCell;
        private Vector2 _currentMousePos;

        private Dictionary<CombatMonster, TileCell> _playerControlledMonsterMap = new();
        private Dictionary<CombatMonster, TileCell> _aIControlledMonsterMap = new();
        public Dictionary<CombatMonster, TileCell> AIControlledMonsterMap => _aIControlledMonsterMap;
        public Dictionary<CombatMonster, TileCell> PlayerControlledMonsterMap => _playerControlledMonsterMap;

        private int? _numberOfCellsMoved = 0;

        private VisualEffect _currentAttackVisualEffect;

        private List<TileCell> _playerCurrentAttackRangeOptions;
        private bool _attackComplete = false;
        private bool _attackPerformed = false;
        private List<TileCell> _summonSpawnableCells;
        private (string name,SummonedSavedStats data) _playerSelectedSummon;
        private CombatMonster _currentMonster;

        private SingleAttack _playerCurrentAttack;
        public Dictionary<string, int> defeatedMonsters = new Dictionary<string, int>();
        public WhoWon TheWinner = WhoWon.None;

        private readonly Dictionary<MonsterActionOrder, Func<bool>> _actionExecutors;
        private readonly Dictionary<MonsterActionOrder, AITurnState> _actionStates;



        private float _timer = 0;
        public CombatMonster CurrentMonster => _currentMonster;

        public CombatManager(PlayMonsters playMonsters, Player player)
        {
            _stateMachine = new CombatStateMachine();
            _currentMapTile = TileManager.CurrentMapTile;
            _visualEffectManager = new VisualEffectManager();


            _diamondHighlight = DrawDiamondTexture.GetDiamond(128, 64, Color.White * 0.5f);
            _playerCellOptions = AssetManager.GetTexture("fightBackground");
            _font = AssetManager.GetFont("mainFont");
            _tileHeight = MapTile.TileHeight;
            _tileWidth = MapTile.TileWidth;

            PlayMonsters = playMonsters;
            var playerCopy = DeepCopyHelper.DeepCopy(player);
            PlayerMonster = new CombatMonster(playerCopy);
           // _playerMonster.Initiation = 5;
            SetSpawnableCells();
            SetCombatMonsterStartingPos();
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
            List<CombatMonster> allCombatants = new List<CombatMonster>();
            allCombatants.AddRange(PlayMonsters.Monsters);
            allCombatants.Add(PlayerMonster);
            allCombatants.AddRange(_summonedMonsters);
            allCombatants.Sort((a, b) => b.BaseStats.Initiative.CompareTo((int)a.BaseStats.Initiative));
            foreach (var entity in allCombatants)
            {
                _referenceTurnOrder.Add(entity);
                _turnOrder.Enqueue(entity);
            }

        }
        private void SetCombatMonsterStartingPos()
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

                    mon.currentPos = pos;
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
            DrawMapBackground(spriteBatch);
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
            DrawAllCombatMonsters(spriteBatch);
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
                DrawCellHighlight(spriteBatch, _statHoverCellHighlight, ColorPalette.DarkColor);
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
                if (mon.MonsterIs == CombatMonsterType.Summoned)
                {
                    col = Color.White * 0.8f;
                }
                if (mon.isDead)
                    col = Color.Gray * 0.4f;

                if (mon == _currentMonster) spriteBatch.Draw(_playerCellOptions, iconRect, col);
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
        private void DrawMapBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_currentMapTile.BackgroundTexture, Vector2.Zero, Color.White);
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
            if (_playerCurrentAttack != null)
            {
                int screenWidthh = ViewportManager.ScreenWidth;
                SpriteFont fontt = AssetManager.GetFont("mainFont");
                Vector2 textSizer = fontt.MeasureString(stateText);
                string currentAttack = $"Current Attack: {_playerCurrentAttack.Name}";
                spriteBatch.DrawString(fontt, currentAttack, new Vector2(screenWidthh - textSizer.X - 20, 60), Color.Orange);
            }
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
                DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, PlayerMonster);
        }
        private void DrawEntityPreviewOnCell(SpriteBatch spriteBatch, TileCell cell, CombatMonster mon = null, Color col = default)
        {
            if (mon == null) mon = _currentMonster;
            Texture2D texture = mon.Icon;
            Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint, mon.DrawSpecifics.Width, mon.DrawSpecifics.Height);

            Rectangle rect = new Rectangle((int)drawPoint.X, (int)drawPoint.Y - mon.DrawSpecifics.Height/2, mon.DrawSpecifics.Width, mon.DrawSpecifics.Height);

            spriteBatch.Draw(texture, rect, col == default ? Color.White : col);
        }
        private void DrawEntityPreviewOnCell(SpriteBatch spriteBatch, TileCell cell, int height, int width, Texture2D texture, Color col = default)
        {
            Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint, width, height);
            Rectangle rect = new Rectangle((int)drawPoint.X, (int)drawPoint.Y - height / 2, width, height);

            spriteBatch.Draw(texture, rect, col == default ? Color.White : col);
        }
        private void DrawSpawnableTiles(SpriteBatch spriteBatch)
        {
            foreach (var tile in _playerSpawnableCells)
                DrawCellHighlight(spriteBatch, tile, Color.Green, 5);

            foreach (var tile in _monsterSpawnableCells)
                DrawCellHighlight(spriteBatch, tile, Color.Red, 5);
        }
        private void DrawCellHighlight(SpriteBatch spriteBatch, TileCell cell, Color color, int shrink = 0)
        {
            Vector2 coords = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint);
            Rectangle rect = new Rectangle(
                (int)coords.X + shrink - MapTile.TileWidth/2,
                (int)coords.Y + shrink,
                128 - shrink * 2,
                64 - shrink * 2
            );
            spriteBatch.Draw(_diamondHighlight, rect, color);
        }
        private void DrawAllCombatMonsters(SpriteBatch spriteBatch)
        {
            foreach (var combatMon in _turnOrder)
            {
                if (combatMon.currentPos.X == 0 && combatMon.currentPos.Y == 0) { continue; }

                Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(combatMon.currentPos, combatMon.DrawSpecifics.Width, combatMon.DrawSpecifics.Height);

                
                Rectangle destination = new Rectangle((int)drawPoint.X , (int)drawPoint.Y - combatMon.DrawSpecifics.Height /2   , combatMon.DrawSpecifics.Width , combatMon.DrawSpecifics.Height );

                Texture2D icon = combatMon.Icon;
                
                Color col = Color.White;
                if (combatMon.MonsterIs == CombatMonsterType.Summoned)
                    col = new Color(Color.White, 0.6f);
                if (combatMon.DrawSpecifics.IsFlashingRed)
                    col = Color.Red * 0.5f;


                spriteBatch.Draw(icon, destination, col);
            }
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
            if (_currentMonster.CurrentStats.MP > 0)
            {
                foreach (var cell in _playerMoveableCells)
                {
                    if (cell.BlockedByMonster || !cell.IsWalkable) continue;
                    DrawCellHighlight(spriteBatch, cell, Color.Green, 5);
                }

                if (_currentMouseHoverCell != null && _playerMoveableCells.Contains(_currentMouseHoverCell))
                {
                    DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell);
                }
            }
        }
        private void DrawPlayClickedSummonButton(SpriteBatch spriteBatch)
        {
            CombatMonster mon = _turnOrder.Peek();

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
            if (_playerSelectedSummon.data == null) return;

            foreach (var cell in _summonSpawnableCells)
            {
                Vector2 cellCoords = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint);
                Rectangle cellRect = new Rectangle((int)cellCoords.X, (int)cellCoords.Y, _tileWidth - 5, _tileHeight - 5);
                DrawCellHighlight(spriteBatch, cell, Color.LimeGreen, 5);
            }

        }
        private void DrawSummonHover(SpriteBatch spriteBatch)
        {
            if (!_summonSpawnableCells.Contains(_currentMouseHoverCell)) return;

            if (_currentMouseHoverCell != null && _summonSpawnableCells.Contains(_currentMouseHoverCell))
            {
                var data = CombatMonsterManager.GetMonsterWidthAndHeight(_playerSelectedSummon.name);
                int width =(int) data.X;
                int height = (int)data.Y;;
                    
                DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, width, height, _playerSelectedSummon.data.Icon );
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
            CombatMonster mon = _currentMonster;
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
            CombatMonster mon = _currentMonster;

            if (_playerCurrentAttackRangeOptions != null)
            {
                foreach (var cell in _playerCurrentAttackRangeOptions)
                {
                    Color col = Color.Red * 5f;
                    if (AIControlledMonsterMap.ContainsValue(cell) ) { col = Color.Green * 5f; }
                    DrawCellHighlight(spriteBatch, cell, col, 5);
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
                            if (PlayerHasEndPoint()) PopulatePath(delta, _currentMonster.PlayerMovementEndPoint);
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
                        case PlayerTurnState.PlayerWaitingInput:
                            
                            break;
                        case PlayerTurnState.PlayerClickedMoveButton:
                            if (PlayerHasEndPoint()) PopulatePath(delta, _currentMonster.PlayerMovementEndPoint);
                            if (PlayerHasMovePath()) SetPlayerTurnState(PlayerTurnState.PlayerExecutingMove);
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
            if (PlayerMonster.isDead){ SetWhoWon(WhoWon.Monster);  return true; }
            return false;
        }
        private void FinishedMoving()
        {
            CombatMonster mon = _currentMonster;
            //mon.CurrentMP -= (int)_numberOfCellsMoved;
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
        private void PopulatePath(float delta, TileCell endTile)
        {
            // this assigned a vector path to mon.movepath
            TileCell end = endTile;
            CombatMonster mon = _currentMonster;
            mon.PlayerMovementEndPoint = null;
            List<TileCell> cellPath = GetPathToPlayerSelectedCell(_playerControlledMonsterMap[mon], end);
            GenerateMovementPath(cellPath);
        }
        private void GeneratePlayerSummonRange()
        {
            
            CombatMonster mon = _currentMonster;
            
            // Later put logic here to decide how far away the palyer can summon a monster
            float range = 2f;

            TileCell origin = _playerControlledMonsterMap[mon];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)range);

            _summonSpawnableCells = cells;

        }
        public bool CheckIfAIShouldEndTurn()
        {
            CombatMonster mon = _currentMonster; 
            if (mon.CurrentStats.AP <= 0)
            {
                EndTurn();
                return true;
            }
            return false;
        }
        private void SpendActionPoint()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentStats.AP -= 1;
        }
        private void SetTopOfActionChoiceStats()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentStats.MP = mon.BaseStats.MP;
            mon.CurrentStats.ChooseWhichAttack.Clear();
            mon.CurrentStats.ActionOrder.Clear();
            if (mon.MonsterIs == CombatMonsterType.AI)
            {
                foreach (var str in mon.DecideWhichAttack)
                {
                    mon.CurrentStats.ChooseWhichAttack.Enqueue(str);
                }
                foreach (var str in mon.ActionOrder)
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
            CombatMonster mon = _currentMonster;

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
            CombatMonster mon = _currentMonster;
            mon.CurrentStats.ActionOrder.Dequeue();
        }
        private bool AttackClosestEnemy()
        {
            CombatMonster mon = _currentMonster;
            TileCell origin = _aIControlledMonsterMap[mon];

            //inRangeMap send any and all attacks that have a valid range, including non monster cells
            var inRangeMap = GetInRangeCellsByAttack(mon.Attacks, origin);
            if (inRangeMap.Count == 0)
            {
                return false;
            }
            // Choose targets for each attack using the targeting strategy
            var targetData = new Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>>();

            foreach (var pair in inRangeMap)
            {
                pair.Value.Remove(origin);
                var (target, affectedCells) = AttackManager.TargetClosestEnemy(pair.Value, origin);
                if (target != null && affectedCells.Count > 0)
                {
                    targetData[pair.Key] = new Dictionary<CombatMonster, List<TileCell>>
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

        private void SetMonsterAttackPathingInformation(SingleAttack att, List<CombatMonster> mons, List<TileCell> cells)
        {
            // this method will set the monster property of attack path(s), current attacks, specified current target info, and visualeffect
            CombatMonster mon = _currentMonster;
            var attackDetails = (att, mons, cells);
            if (attackDetails.Item1 == null)
            {
                AIActionNavigation();
                return;
            }
            TileCell currentCell = GetMonsterCurrentCell(mon);

            TileCell centerCell = FindCenterCell(attackDetails.Item3);
            List<Vector2> path = NPCMovement.MoveMonsters(mon, currentCell, centerCell);
            var paths = Movement.CombatGrid.GridMovement.SplitAttackPath(path, attackDetails.Item1);
            mon.CurrentStats.AttackPath1 = paths.Item1;
            mon.CurrentStats.AttackPath2 = paths.Item2;
            mon.CurrentStats.Attack = attackDetails.Item1;
            mon.CurrentStats.AttackEffectedMonsters = attackDetails.Item2;
            mon.CurrentStats.AttackEffectedCells = attackDetails.Item3;
            if (mon.CurrentStats.Attack.Animated)
            {
                _currentAttackVisualEffect = new VisualEffect(GetMonsterCurrentCell(mon), mon.CurrentStats.Attack, centerCell);
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
                CombatMonster mon = _currentMonster;
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
            CombatMonster mon = _currentMonster;
            _numberOfCellsMoved = tileCellPath.Count;
            List<Vector2> fullVectorPath = new();
            TileCell startingCell = GetMonsterCurrentCell(mon);

            foreach (var endPos in tileCellPath)
            {
                List<Vector2> arc = NPCMovement.MoveMonsters(mon, startingCell, endPos);
                fullVectorPath.AddRange(arc);
                startingCell = endPos;

            }

            mon.MovePath = fullVectorPath;

        }
        public bool AIHasMP() => _currentMonster.CurrentStats.MP >= 0;
        public bool MonsterFinishedMoving() =>  _currentMonster.MovePath == null || _currentMonster.MovePath.Count <= 0;
        public bool AICanAttack() => _currentMonster.CurrentStats.Attack != null;
        public bool PlayerHasEndPoint() => _currentMonster.PlayerMovementEndPoint != null;
        public bool PlayerHasMovePath() => _currentMonster.MovePath.Count > 0;
        private void AIActionNavigation()
        {
            SetAITurnState(AITurnState.ActionNavigation);
        }
        private void AIFinishedAttack()
        {
            CombatMonster mon = _currentMonster;
            if (mon.MovePath == null || mon.MovePath.Count <= 0)
            {
                _attackComplete = true;
                _attackPerformed = false;
            }
        }
        private void WaitForAttackToFinish(float delta)
        {
            CombatMonster mon = _currentMonster;

            // 🔄 Visual Effect Handling (returns true if we should pause execution)
            if (HandleAttackVisualEffect())
                return;

            if (mon.CurrentStats.AttackPath1 != null && mon.CurrentStats.AttackPath1.Count > 0)
            {
                mon.MovePath = mon.CurrentStats.AttackPath1;
                mon.CurrentStats.AttackPath1 = null;
                switch (mon.MonsterIs)
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
                AttackManager.PerformAttack(mon.CurrentStats.Attack, mon, mon.CurrentStats.AttackEffectedMonsters, mon.CurrentStats.AttackEffectedCells);

                mon.CurrentStats.Attack = null;
                mon.CurrentStats.AttackEffectedMonsters = null;
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
                mon.MovePath = mon.CurrentStats.AttackPath2;
                mon.CurrentStats.AttackPath2 = null;
                if (StateCombat == CombatState.PlayerTurn)
                {
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingAttack);
                }
                if (StateCombat == CombatState.SummonedTurn) { SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack); }
                return;
            }
            //attack and movement associated it is finished, so go to next turn
            switch (mon.MonsterIs)
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
                    CombatMonster mon = _currentMonster;
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
            CombatMonster mon = _currentMonster;
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
            CombatMonster mon = _currentMonster;
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
                CombatMonster mon = kvp.Key;
                Rectangle rect = kvp.Value;
                if (rect.Contains(_currentMousePos))
                {
                    _statHoverCellHighlight = GetMonsterCurrentCell(mon);
                    return;
                }
            }
            _statHoverCellHighlight = null;
        }
        private void HandlePlayerChooseSummonedCell()
        {
            TileCell cell = _currentClickedCell;
            if (_playerSelectedSummon.data != null && _summonSpawnableCells.Contains(cell))
            { 
                    SummonSummonMonster(cell);
                    _playerSelectedSummon.data = null;
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingSummoning);
                SpendActionPoint();
                
            }
        }
        private void ResetClickValues()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentStats.AttackEffectedCells = null;
            mon.CurrentStats.AttackEffectedMonsters = null;
            mon.CurrentStats.AttackPath1 = null;
            mon.CurrentStats.AttackPath2 = null;
            _playerSelectedSummon.data = null;
            _playerCurrentAttack = null;
            _playerCurrentAttackRangeOptions = null;

        }
        private void HandlePlayerSelectingSpecificAttackAndItsRange()
        {
            foreach (var (rect, attack) in _attackButtons)
            {
                if (InputManager.IsLeftClick() && rect.Contains(_currentMousePos))
                {
                    _playerCurrentAttack = attack;
                    CombatMonster mon = _currentMonster;
                    _playerCurrentAttackRangeOptions = TileManager.GetFloodFillTileWithinRange(GetMonsterCurrentCell(mon), _playerCurrentAttack.Range, includeMonsterTiles: true);
                    SpendActionPoint();
                    SetSummonedTurnState(SummonedTurnState.SummonedChoosingTarget);
                }
            }
        }
        private void UpdatePlayerClickedMoveDestination()
        {
            
            if (_playerMoveableCells.Contains(_currentClickedCell))
            {
                CombatMonster mon = _currentMonster;
                mon.PlayerMovementEndPoint = _currentClickedCell;
                SpendActionPoint();
            }

        }
        private void UpdatePlayerMoveableCells()
        {
            CombatMonster mon = _currentMonster;
            TileCell origin = _playerControlledMonsterMap[mon];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)mon.CurrentStats.MP);

            _playerMoveableCells = cells;
        }
        private void HandleLocationSelectionInput()
        {
            if (_playerSpawnableCells.Contains(_currentMouseHoverCell) && InputManager.IsLeftClick())
            {
                PlayerMonster.currentPos = _currentMouseHoverCell.CenterPoint;
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
            if (_turnOrder.Count <= 0) return;

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

            foreach (var mon in _turnOrder)
            {
                TileCell cell = TileManager.GetCell(mon.currentPos);
                if (mon.MonsterIs == CombatMonsterType.Summoned || mon.MonsterIs == CombatMonsterType.Player)
                {
                    cell.BlockedByMonster = true;
                    _playerControlledMonsterMap[mon] = cell;

                }
                else if (mon.MonsterIs == CombatMonsterType.AI)
                {
                    cell.BlockedByMonster = true;
                    _aIControlledMonsterMap[mon] = cell;
                }

            }
        }
        private void UpdateMonsterTopOfRoundStats()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentStats.AP = mon.BaseStats.AP;
           
        }
        private void PickWhichEntitiesTurn()
        {
            CombatMonster mon = _currentMonster;
            switch (mon.MonsterIs)
            {
                case CombatMonsterType.Player:
                    SetCombatState(CombatState.PlayerTurn);
                    SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
                    mon.PlayerMovementEndPoint = null;
                    return;
                case CombatMonsterType.Summoned:
                    SetCombatState(CombatState.SummonedTurn);
                    SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput);
                    mon.PlayerMovementEndPoint = null;
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
            foreach (var mon in _turnOrder)
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
            CombatMonster mon = _currentMonster;
            if (mon.Aspects == null || mon.Aspects.Count == 0)
            {
                return;
            }
            AspectManager.ResolveAspect(mon, tick);


        }
        private void ToggleIsDead()
        {
            bool someOneDied = false;
            foreach (var mon in _turnOrder)
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
            Queue<CombatMonster> newQueue = new Queue<CombatMonster>();

            foreach (var mon in _turnOrder)
            {
                if (!mon.isDead)
                {
                    newQueue.Enqueue(mon);
                }
            }

            _turnOrder = newQueue;
        }

        private void SkipMonsterIfDead()
        {
            int maxTries = _turnOrder.Count;
            while (maxTries-- > 0)
            {
                CombatMonster mon = _currentMonster;
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
            CombatMonster mon = _turnOrder.Dequeue();
            _turnOrder.Enqueue(mon);
        }
        private void UpdateCurrentMonster()
        {
            if (_currentMonster == _turnOrder.Peek()) return;
            _currentMonster = _turnOrder.Peek();
        }



        private void HandleSummonedTargetingAttackClick()
        {
            if (InputManager.IsLeftClick() && _playerCurrentAttackRangeOptions.Contains(_currentMouseHoverCell) && _currentMouseHoverCell.BlockedByMonster && !_playerControlledMonsterMap.ContainsValue(_currentMouseHoverCell))
            {
                SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack);
                TileCell currentTarget = _currentMouseHoverCell;
                _attackComplete = false;
                SetPlayerAttackEffectCellsAndMonsters(currentTarget);

                CombatMonster mon = _currentMonster;
                mon.CurrentStats.Attack = _playerCurrentAttack;
                SetAttackPathForPlayer();
                if (_playerCurrentAttack.Animated)
                    _currentAttackVisualEffect = new VisualEffect(GetMonsterCurrentCell(mon), _playerCurrentAttack, FindCenterCell(mon.CurrentStats.AttackEffectedCells));

            }
        }
        private void SetPlayerAttackEffectCellsAndMonsters(TileCell targetCell)
        {
            CombatMonster mon = _currentMonster;
            SingleAttack att = _playerCurrentAttack;
            TileCell target = targetCell;

            mon.CurrentStats.AttackEffectedCells = new List<TileCell> { target };
            mon.CurrentStats.AttackEffectedMonsters = new List<CombatMonster>();

            foreach (var kvp in _aIControlledMonsterMap)
            {
                CombatMonster aiMon = kvp.Key;
                TileCell aiCell = kvp.Value;

                if (mon.CurrentStats.AttackEffectedCells.Contains(aiCell))
                {
                    mon.CurrentStats.AttackEffectedMonsters.Add(aiMon);
                }
            }
        }
        private void SetAttackPathForPlayer()
        {
            CombatMonster mon = _currentMonster;
            List<Vector2> path = NPCMovement.MoveMonsters(mon, _playerControlledMonsterMap[mon], FindCenterCell(mon.CurrentStats.AttackEffectedCells));
            var paths = GridMovement.SplitAttackPath(path, _playerCurrentAttack);
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
                    _playerSelectedSummon = (name, stats);
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
            SetPlayerTurnState(PlayerTurnState.PlayerExecutingSummoning);

            CombatMonster comSumMon = (CombatMonsterManager.SummonMonsterToCombat(_playerSelectedSummon.name));
            comSumMon.currentPos = cell.CenterPoint;
            AddComMonToTurnOrder(comSumMon);
        }
        private void AddComMonToTurnOrder(CombatMonster mon)
        {
            List<CombatMonster> updatedList = new List<CombatMonster>();

            bool inserted = false;

            foreach (var combatMon in _turnOrder)
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

            _turnOrder = new Queue<CombatMonster>(updatedList);
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
        private TileCell GetMonsterCurrentCell(CombatMonster mon)
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
                if (mon.isDead && mon.MonsterIs == CombatMonsterType.AI)
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


        public CombatMonster GetPlayerMonster()
        {
            return PlayerMonster;
        }


    }
}
public enum WhoWon
{
    None,
    Player,
    Monster
}

