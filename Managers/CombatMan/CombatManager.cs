using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.CombatMan.Aspects;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Managers.Proximity;
using PlayingAround.Managers.Tiles;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static CombatStateMachine;
using static PlayingAround.Managers.CombatMan.CombatStateMachine;
using static PlayingAround.Managers.SceneManager;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatManager
    {

        private static CombatMonster _playerMonster; // Need to update _player at the end of combat accordingly






        private static List<TileCell> _playerMoveableCells = new List<TileCell>();
        private static List<CombatMonster> _summonedMonsters = new List<CombatMonster>();



        private static TileCell _currentTarget;

        //  private static CombatMonster _standInMonster = new CombatMonster();
        private static List<CombatMonster> _defeatedMonsters = new List<CombatMonster>();
        private static bool _firstRound = true;
        private static bool _actionComplete = false;
        private static bool _movementComplete = false;
        private static float _playerBaseSpeed;
        private static int _playerBaseSP;
        private static bool _playerIsSummoning = false;
        private static int _summonOptionHeight = 64;
        private static int _summonOptionWidth = 64;
        private static int _summonOptionSpacing = 10;
        private static SummonedMonster _playerSelectedSummon;
        private static List<TileCell> _summonSpawnableCells;
        private static SingleAttack _playerCurrentAttack;
        private static List<TileCell> _playerCurrentAttackRangeOptions;

        private static bool _attackComplete = false;


        private static SingleAttack _drawnAttack = null;
        private static bool _attackAnimationBeforeHit;

        public static VisualEffectManager VisualEffectManager => _visualEffectManager;
        private static float _timer = 0;




        private static List<string> _log = new List<string>();
        private static int _maxStrings = 50;





        private CombatUIManager _combatUIManager;
        private CombatStateMachine _stateMachine;
        private VisualEffectManager _visualEffectManager;
        public PlayerTurnState StatePlayerTurn => _stateMachine.CurrentPlayerTurnState;
        public CombatState StateCombat => _stateMachine.CurrentCombatState;
        public SummonedTurnState StateSummoned => _stateMachine.CurrentSummonedTurnState;
        public AITurnState StateAI => _stateMachine.CurrentAITurnState;
        private List<CombatMonster> _referenceTurnOrder = new List<CombatMonster>();
        public Queue<CombatMonster> _turnOrder = new Queue<CombatMonster>();

        private MapTile _currentMapTile;
        private Texture2D _playerCellOptions;//placeholder texture
        private SpriteFont _font;

        private int _tileWidth;
        private int _tileHeight;
        private Rectangle _backBackGroundButtonOptions = new Rectangle(1600, 720, 200, 100);
        private List<(Rectangle rect, SingleAttack attack)> _attackButtons = new();
        private Rectangle _summonRect, _attackRect, _endTurnRect, _moveRect, _attackOptionsRect;

        private PlayMonsters _playMonsters; // kept as reference as needed
        private Player _player; // reference of player to update stats at end
        private List<TileCell> _playerSpawnableCells = new List<TileCell>();
        private List<TileCell> _monsterSpawnableCells = new List<TileCell>();

        private TileCell _currentClickedCell;
        private TileCell _currentMouseHoverCell;
        private Vector2 _currentMousePos;

        private Dictionary<CombatMonster, TileCell> _playerControlledMonsterMap = new();
        private Dictionary<CombatMonster, TileCell> _aIControlledMonsterMap = new();

        private int? _numberOfCellsMoved = 0;

        private VisualEffect _currentAttackVisualEffect;


        private CombatMonster _currentMonster;


        public CombatManager(PlayMonsters playMonsters, Player player)
        {
            _stateMachine = new CombatStateMachine();
            _currentMapTile = TileManager.CurrentMapTile;
            _combatUIManager = new CombatUIManager(_stateMachine, _turnOrder, _referenceTurnOrder);
            _visualEffectManager = new VisualEffectManager();



            _playerCellOptions = AssetManager.GetTexture("fightBackground");
            _font = AssetManager.GetFont("mainFont");
            _tileHeight = MapTile.TileHeight;
            _tileWidth = MapTile.TileWidth;

            _playMonsters = playMonsters;
            _player = player;
            _playerMonster = new CombatMonster(player);

            SetCombatMonsterStartingPos();
            SetTurnOrder();
            UpdateCurrentMonster();
            InitilizeUIElements();

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
        public void SetAITurnState(AITurnState state)
        {
            _stateMachine.SetAITurnState(state);
        }
        private void SetTurnOrder()
        {
            List<CombatMonster> allCombatants = new List<CombatMonster>();
            allCombatants.AddRange(_playMonsters.Monsters);
            allCombatants.Add(_playerMonster);
            allCombatants.AddRange(_summonedMonsters);
            allCombatants.Sort((a, b) => b.Initiation.CompareTo((int)a.Initiation));
            int idCounter = 0;
            foreach (var entity in allCombatants)
            {
                entity.ID = idCounter++;
                _referenceTurnOrder.Add(entity);
                _turnOrder.Enqueue(entity);
            }

        }
        private void SetCombatMonsterStartingPos()
        {
            if (_monsterSpawnableCells.Count < _playMonsters.Monsters.Count) { Debug.WriteLine($"More Monstesr than cells to spawn in"); }
            Random ran = new Random();
            List<TileCell> spawnableCells = new List<TileCell>(_monsterSpawnableCells);
            List<CombatMonster> comMon = new List<CombatMonster>(_playMonsters.Monsters);
            do
            {
                foreach (var mon in comMon)
                {
                    int index = ran.Next(spawnableCells.Count);
                    Vector2 pos = (TileManager.GetCellCords(spawnableCells[index]));
                    mon.startingPos = pos;
                    mon.currentPos = pos;
                    spawnableCells.RemoveAt(index);
                }

            } while (spawnableCells.Count < spawnableCells.Count - _playMonsters.Monsters.Count);
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
            }
            DrawMapBackground(spriteBatch);
            _combatUIManager.Draw(spriteBatch);
            _visualEffectManager.Draw(spriteBatch, _font);
            DrawDebugInfo(spriteBatch);
            DrawAllCombatMonsters(spriteBatch);
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
            string stateText = $"State: {_playerTurnState}";
            string combatStateText = $"CombatState: {_currentState}";
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

            Vector2 position = new Vector2(screenWidth - textSize.X - 20, 10);
            spriteBatch.DrawString(font, stateText, position, Color.Orange);
            spriteBatch.DrawString(font, combatStateText, new Vector2(screenWidth - textSize.X - 20, 30), Color.Orange);

        }
        private void DrawLocationSelection(SpriteBatch spriteBatch)
        {
            DrawSpawnableTiles(spriteBatch);

            if (_currentMouseHoverCell != null && _currentMouseHoverCell.HeroSpawnable)
                DrawHeroPreviewOnCell(spriteBatch, _currentMouseHoverCell);
        }
        private void DrawHeroPreviewOnCell(SpriteBatch spriteBatch, TileCell cell, Color col = default)
        {
            Vector2 coords = TileManager.GetCellCords(cell);
            Rectangle rect = new Rectangle((int)coords.X, (int)coords.Y, 64, 64);
            spriteBatch.Draw(_player.Texture, rect, col == default ? Color.White : col);
        }
        private void DrawSpawnableTiles(SpriteBatch spriteBatch)
        {
            foreach (var tile in _playerSpawnableCells)
                DrawCellHighlight(spriteBatch, tile, Color.White);

            foreach (var tile in _monsterSpawnableCells)
                DrawCellHighlight(spriteBatch, tile, Color.Black);
        }
        private void DrawCellHighlight(SpriteBatch spriteBatch, TileCell cell, Color color, int shrink = 0)
        {
            Vector2 coords = TileManager.GetCellCords(cell);
            Rectangle rect = new Rectangle(
                (int)coords.X + shrink,
                (int)coords.Y + shrink,
                64 - shrink * 2,
                64 - shrink * 2
            );
            spriteBatch.Draw(_playerCellOptions, rect, color);
        }
        private void DrawAllCombatMonsters(SpriteBatch spriteBatch)
        {
            foreach (var combatMon in _turnOrder)
            {
                if (combatMon.currentPos.X == 0 && combatMon.currentPos.Y == 0) { continue; }
                Rectangle destination = new Rectangle(
                    (int)(combatMon.currentPos.X),
                    (int)(combatMon.currentPos.Y),
                    64, 64);

                string textureKey;
                if (combatMon.isSummoned)
                {
                    textureKey = combatMon.IconTextureKey; // Summons use their own icon texture
                }
                else if (combatMon.isMonster)
                {
                    textureKey = combatMon.IconTextureKey;
                }
                else if (combatMon.isPlayer)
                {
                    textureKey = "Hero_Blonde";
                }
                else textureKey = "Hero_Blonde";
                Color col = Color.White;
                if (combatMon.IsFlashingRed)
                    col = Color.Red * 0.5f;
                else if (combatMon.isSummoned)
                    col = new Color(Color.White, 0.6f);

                spriteBatch.Draw(AssetManager.GetTexture(textureKey), destination, col);
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

                case PlayerTurnState.PlayerChoosingSummoned:
                    DrawSummonOptions(spriteBatch);
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
            DrawButton(spriteBatch, _attackRect, "Attack");
            DrawButton(spriteBatch, _endTurnRect, "End Turn");

        }
        public void DrawPlayerClickedMoveButton(SpriteBatch spriteBatch)
        {
            if (_currentMonster.CurrentMP > 0)
            {
                foreach (var cell in _playerMoveableCells)
                {
                    if (cell.BlockedByMonster || !cell.IsWalkable) continue;
                    DrawCellHighlight(spriteBatch, cell, Color.Black, 5);
                }

                if (_currentMouseHoverCell != null && _playerMoveableCells.Contains(_currentMouseHoverCell))
                {
                    DrawHeroPreviewOnCell(spriteBatch, _currentMouseHoverCell);
                }
            }
        }
        private void DrawSummonOptions(SpriteBatch spriteBatch)
        {
            CombatMonster mon = _turnOrder.Peek();

            if (_player.stats.UnlockedSummons != null && _player.stats.UnlockedSummons.Count > 0)
            {

                for (int i = 0; i < _player.stats.UnlockedSummons.Count; i++)
                {
                    var summOption = _player.stats.UnlockedSummons[i];

                    Rectangle summonIconRect = new Rectangle(
                        _summonRect.X,
                        _summonRect.Y - ((i + 1) * (_summonOptionHeight + _summonOptionSpacing)),
                        _summonOptionWidth,
                        _summonOptionHeight
                    );

                    spriteBatch.Draw(AssetManager.GetTexture(summOption.IconTextureString), summonIconRect, Color.White);

                    // Optional: draw border or hover highlight
                    if (summonIconRect.Contains(_currentMousePos))
                        spriteBatch.Draw(_playerCellOptions, summonIconRect, Color.Yellow * 0.4f);
                }
            }
        }
        private void DrawSummonSpawnLocationOptions(SpriteBatch spriteBatch)
        {
            if (_playerSelectedSummon == null) return;

            foreach (var cell in _summonSpawnableCells)
            {
                Vector2 cellCoords = TileManager.GetCellCords(cell);
                Rectangle cellRect = new Rectangle((int)cellCoords.X, (int)cellCoords.Y, _tileWidth - 5, _tileHeight - 5);

                spriteBatch.Draw(_playerCellOptions, cellRect, Color.Red * 0.4f);
            }

        }
        private void DrawSummonHover(SpriteBatch spriteBatch)
        {
            if (!_summonSpawnableCells.Contains(_currentMouseHoverCell)) return;

            // Step 2: Only if we're hovering over a summonable cell, draw the summon icon
            if (_currentMouseHoverCell != null && _summonSpawnableCells.Contains(_currentMouseHoverCell))
            {
                Vector2 hoverCoords = TileManager.GetCellCords(_currentMouseHoverCell);
                Rectangle hoverRect = new Rectangle((int)hoverCoords.X, (int)hoverCoords.Y, _tileWidth, _tileHeight);

                spriteBatch.Draw(AssetManager.GetTexture(_playerSelectedSummon.IconTextureString), hoverRect, Color.White * 0.7f);
            }
        }




        private void DrawSummonedTurn(SpriteBatch spriteBatch)
        {
            switch (StateSummoned)
            {
                case SummonedTurnState.SummonClickedAttack:
                    DrawSummonedAttackOptions(spriteBatch);
                    break;
                case SummonedTurnState.SummonTargetingAttack:
                    DrawSummonedAttackRangeOptions(spriteBatch);
                    break;

            }
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

                string attackName = attack.Name.ToUpper();
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
                    DrawCellHighlight(spriteBatch, cell, Color.Red * 5f, 5);
                }
            }

        }










        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateMouseWhereabouts();
            UpdateInput(gameTime, delta);
            _visualEffectManager.Update(delta);

            UpdateMonsterTakingDamage(delta);
            UpdateMonsterCellMap();
            switch (StateCombat)
            {
                case CombatState.TurnStart:

                    UpdateCurrentMonster();
                    ToggleIsDead(); // toggles ISDead as well as clears aspects
                    SkipMonsterIfDead(); // dequees and requeues monster if dead
                    _currentMonster.TurnNumber++;
                    UpdateMonsterTopOfRoundStats();
                    PickWhichEntitiesTurn();
                    break;

                case CombatState.AITurn:

                    switch (StateAI)
                    {
                        case AITurnState.ActionNavigation:
                            if (CheckIfAIShouldEndTurn()) return;
                            DecideAINextAction();
                            break;
                        case AITurnState.MovingAIControlled:
                            if (AIHasMP()) GenerateMovementPath();
                            SetAITurnState(AITurnState.ExecutingMove);
                            break;
                        case AITurnState.ExecutingMove:
                            if (MonsterFinishedMoving()) AIFinishedAction();
                            break;
                        case AITurnState.AIAttacking:
                            SetAITurnState(AITurnState.ExecutingAttack);
                            SetMonsterAttackPathingInformation();
                            if (AICanAttack()) _attackComplete = false;

                            break;
                        case AITurnState.ExecutingAttack:
                            if (_attackComplete) { AIFinishedAction(); return; }
                            WaitForAttackToFinish(delta);
                            break;
                    }
                    break;
                case CombatState.SummonedTurn:

                    break;
                case CombatState.PlayerTurn:
                    UpdatePlayerMoveableCells();
                    GeneratePlayerSummonRange();
                    switch (StatePlayerTurn)
                    {
                        case PlayerTurnState.PlayerWaitingInput:
                            if (PlayerHasEndPoint()) PopulatePath(delta, _currentMonster.PlayerMovementEndPoint);
                            if (PlayerHasMovePath()) SetPlayerTurnState(PlayerTurnState.PlayerExecutingMove);
                            break;
                        case PlayerTurnState.PlayerExecutingMove:
                            if (!PlayerHasMovePath()) SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
                            break;
                        case PlayerTurnState.PlayerExecutingAttack:
                            if (!PlayerHasMovePath()) WaitForAttackToFinish(delta);
                            break;
                           
                    }
                    break;
                case CombatState.ResolvingEndOfTurnEffects:
                    if (_timer == 0) ResolveAspects(TickedTiming.EndOfTurn, _currentMonster);
                    _timer += delta;
                    if (_timer >= 1f) LeaveResolvingEndOfTurnEffects();
                    break;
                case CombatState.ResolvingStartOfTurnEffects:
                    if (_timer == 0) ResolveAspects(TickedTiming.StartOfTurn, _currentMonster);
                    _timer += delta;
                    if (_timer >= 1f) LeaveResolvingStartOfTurnEffects();
                    break;
                    
            }
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

            CombatMonster mon = _turnOrder.Peek();
            float range = _playerBaseSpeed;

            TileCell origin = _playerControlledMonsterMap[mon];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)range);

            _summonSpawnableCells = cells;

        }
        public bool CheckIfAIShouldEndTurn()
        {
            CombatMonster mon = _currentMonster; 
            if (mon.OrderOfActions.Count <= 0)
            {
                EndTurn();
                return true;
            }
            return false;
        }
        private void EndTurn()
        {

            CombatMonster mon = _currentMonster;
            ResolveAspects(TickedTiming.EndOfTurn, mon);


            ResetAllStatesToNone();
            SetCombatState(CombatState.ResolvingEndOfTurnEffects);

        }
        public void ResetAllStatesToNone()
        {
            SetPlayerTurnState(PlayerTurnState.None);
            SetAITurnState(AITurnState.None);
            SetSummonedTurnState(SummonedTurnState.None);
        }
        public void DecideAINextAction()
        {
            CombatMonster mon = _currentMonster;
            string action = mon.OrderOfActions.Peek();

            switch (action)
            {
                case "moveCloser":
                    SetAITurnState(AITurnState.MovingAIControlled);
                    break;

                case "moveFurther":
                    SetAITurnState(AITurnState.MovingAIControlled); // Replace with correct state if different
                    break;

                case "attack":
                    SetAITurnState(AITurnState.AIAttacking);
                    break;

                default:
                    if (mon.OrderOfActions.Count == 0)
                    {
                        SetCombatState(CombatState.Debug);
                    }
                    break;
            }

        }
        public bool AIHasMP() => _currentMonster.CurrentMP >= 0;
        public bool MonsterFinishedMoving() =>  _currentMonster.MovePath == null || _currentMonster.MovePath.Count <= 0;
        public bool AICanAttack() => _currentMonster.CurrentAttack != null;
        public bool PlayerHasEndPoint() => _currentMonster.PlayerMovementEndPoint != null;
        public bool PlayerHasMovePath() => _currentMonster.MovePath != null || _currentMonster.MovePath.Count > 0;
        private void AIFinishedAction()
        {
            CombatMonster mon = _currentMonster;
            mon.OrderOfActions.Dequeue();
            SetAITurnState(AITurnState.ActionNavigation);
        }
        private void AIFinishedAttack()
        {
            _attackComplete = true;
            
        }
        private void WaitForAttackToFinish(float delta)
        {
            CombatMonster mon = _currentMonster;

            // 🔄 Visual Effect Handling (returns true if we should pause execution)
            if (HandleAttackVisualEffect())
                return;

            if (mon.attackPath1 != null && mon.attackPath1.Count > 0)
            {
                mon.MovePath = mon.attackPath1;
                mon.attackPath1 = null;
                if (mon.isPlayer)
                {
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingAttack);
                }
                else if (mon.isSummoned)
                {
                    SetSummonedTurnState(SummonedTurnState.SummonExecutingAttack);
                }
                return;

            }
            else if (!_attackComplete)
            {
                _attackComplete = true;
                AttackManager.PerformAttack(mon.CurrentAttack, mon, mon.CurrentAttackEffectedMonsters, mon.CurrentAttackEffectedCells);

                mon.CurrentAttack = null;
                mon.CurrentAttackEffectedMonsters = null;
                mon.CurrentAttackEffectedCells = null;

                // 👇 Handle "AfterAttack" visuals here
                if (_currentAttackVisualEffect != null && _currentAttackVisualEffect.WhenToStart == VisualTiming.AfterAttack)
                {
                    VisualEffectManager.AddEffect(_currentAttackVisualEffect);
                    _currentAttackVisualEffect.WhenToStart = VisualTiming.IsRunning;
                    return;
                }

                return;
            }
            else if (mon.attackPath2 != null && mon.attackPath2.Count > 0)
            {
                mon.MovePath = mon.attackPath2;
                mon.attackPath2 = null;
                if (StateCombat == CombatState.PlayerTurn)
                {
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingAttack);
                }
                if (StateCombat == CombatState.SummonedTurn) { SetSummonedTurnState(SummonedTurnState.SummonExecutingAttack); }
                return;
            }
            //attack and movement associated it if finished, so go to next turn state
            if (mon.isPlayer)
            {
                SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
            }
            else if (mon.isSummoned)
            {
                SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput);
            }

            else if (mon.isMonster)
            {
                AIFinishedAction();
            }


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

                    break;

                    case CombatState.AITurn:

                    break;

            }

        }
        private void HandlePlayerTurnInput(float delta)
        {
            CombatMonster mon = _currentMonster;
            if (InputManager.IsRightClick()) SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
            switch (StatePlayerTurn)
            {
                case PlayerTurnState.PlayerWaitingInput:
                    HandleSummonRectClick();
                    HandlePlayerEndTurn();
                    HandleMovementRectClick();
                    HandleAttackRectClick();
                    ResetClickValues(mon);
                    break;

                case PlayerTurnState.PlayerSummoning:
                    HandleSummonDropdownClick();
                    break;

                case PlayerTurnState.PlayerAttacking:
                    HandleDisplayAttackOptionsClick();

                    break;
                case PlayerTurnState.PlayerMoving:
                    UpdatePlayerClickedMoveDestination();
                    break;

                case PlayerTurnState.PlayerTargeting:
                    HandlePlayerTargetingAttackClick();
                    break;

                case PlayerTurnState.PlayerExecutingAction:

                    break;
                case PlayerTurnState.PlayerExecutingAttack:

                    break;

                case PlayerTurnState.PlayerEndingTurn:
                    // Optional — confirm dialog, visual delay, etc.
                    break;
            }
        }
        private void UpdatePlayerClickedMoveDestination()
        {
            
            if (_playerMoveableCells.Contains(_currentClickedCell))
            {
                CombatMonster mon = _currentMonster;
                mon.PlayerMovementEndPoint = _currentClickedCell;
            }

        }
        private void UpdatePlayerMoveableCells()
        {
            CombatMonster mon = _currentMonster;
            TileCell origin = _playerControlledMonsterMap[mon];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, (int)mon.CurrentMP);

            _playerMoveableCells = cells;
        }
        private void HandleLocationSelectionInput()
        {
            if (_playerSpawnableCells.Contains(_currentMouseHoverCell) && InputManager.IsLeftClick())
            {
                _playerMonster.currentPos = TileManager.GetCellCords(_currentMouseHoverCell);
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
                if (mon.isPlayer || mon.isSummoned)
                {
                    cell.BlockedByMonster = true;
                    _playerControlledMonsterMap[mon] = cell;

                }
                else if (mon.isMonster)
                {
                    cell.BlockedByMonster = true;
                    _aIControlledMonsterMap[mon] = cell;
                }

            }
        }
        private void UpdateMonsterTopOfRoundStats()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentMP = mon.MP;
            mon.CurrentSP = mon.SP;
            mon.CurrentOrderOfActions = mon.OrderOfActions;
        }
        private void PickWhichEntitiesTurn()
        {
            CombatMonster mon = _currentMonster;
            if (mon.isPlayer)
            {
                SetCombatState(CombatState.PlayerTurn);
                mon.PlayerMovementEndPoint = null;
                return;
            }
            else if (mon.isSummoned)
            {
                SetCombatState(CombatState.SummonedTurn);
                mon.PlayerMovementEndPoint = null;
                return;
            }
            else if (mon.isMonster)
            {
                SetCombatState(CombatState.AITurn);
                DecideOrderOfOperations();
                return;
            }
            SetCombatState(CombatState.Debug);
}
        private void DecideOrderOfOperations()
        {
            CombatMonster mon = _currentMonster;
            if (mon.isPlayer || mon.isSummoned) { return; }

            if (mon.TurnBehavior == "getCloseAsPossible")
            {
                mon.OrderOfActions = new Queue<string>(new[] { "moveClose", "attack" });
            }

        }
        private void GenerateMovementPath(List<TileCell> tileCellPath = null)
        {
            CombatMonster mon = _currentMonster;
            if (tileCellPath == null)
            {
                tileCellPath = GetMovementCellPath();
            }
            if (tileCellPath.Count == 0)
            {
                mon.MovePath.Clear();
                return;
            }
            _numberOfCellsMoved = tileCellPath.Count;
            List<Vector2> fullVectorPath = new();
            TileCell startingCell = null;
            if (mon.isPlayer || mon.isSummoned)
            {
                startingCell = _playerControlledMonsterMap[mon];
            }
            if (mon.isMonster)
            {
                startingCell = _aIControlledMonsterMap[mon];
            }

            foreach (var endPos in tileCellPath)
            {
                Vector2 end = TileManager.GetCellCords(endPos);
                List<Vector2> arc = NPCMovement.MoveMonsters(mon, startingCell, endPos);
                fullVectorPath.AddRange(arc);
                startingCell = endPos;

            }

            mon.MovePath = fullVectorPath;

        }
        private List<TileCell> GetMovementCellPath()
        {
            CombatMonster mon = _currentMonster;
            if (mon.OrderOfActions.Peek() == "moveClose")
            {

                TileCell currentCell = _aIControlledMonsterMap[mon];

                List<TileCell> playerControlledCells = _playerControlledMonsterMap
                    .Select(pair => pair.Value)
                    .Where(cell => cell != null)
                    .ToList();

                // If no targets or already adjacent, return current position
                if (TileManager.IsNeighbor(playerControlledCells, currentCell))
                    return new List<TileCell>();

                List<TileCell> listOfCellsPathToTarget = GridMovement.FindClosestTargetPath(currentCell, playerControlledCells, (int)mon.CurrentMP);
                return listOfCellsPathToTarget;
            }

            return null;
        }
        private void SetMonsterAttackPathingInformation()
        {
            // this method will set the monster property of attack path(s), current attacks, specified current target info, and visualeffect
            CombatMonster mon = _currentMonster;
            var attackDetails = AIChooseAttackAndTarget();
            if (attackDetails.Item1 == null)
            {
                AIFinishedAction();
                return;
            }
            TileCell currentCell = GetMonsterCurrentCell(mon);

            TileCell centerCell = FindCenterCell(attackDetails.Item3);
            List<Vector2> path = NPCMovement.MoveMonsters(mon, currentCell, centerCell);
            var paths = Movement.CombatGrid.GridMovement.SplitAttackPath(path, attackDetails.Item1);
            mon.attackPath1 = paths.Item1;
            mon.attackPath2 = paths.Item2;
            mon.CurrentAttack = attackDetails.Item1;
            mon.CurrentAttackEffectedMonsters = attackDetails.Item2;
            mon.CurrentAttackEffectedCells = attackDetails.Item3;
            if (mon.CurrentAttack.Animated)
            {
                _currentAttackVisualEffect = new VisualEffect(GetMonsterCurrentCell(mon), mon.CurrentAttack, centerCell);
            }


        }
        public (SingleAttack, List<CombatMonster>, List<TileCell>) AIChooseAttackAndTarget()
        {
            (SingleAttack chosenAttack, Dictionary<CombatMonster, List<TileCell>> attackCells) finalAttack;
            CombatMonster mon = _currentMonster;
            List<SingleAttack> attackOptions = new List<SingleAttack>();
            foreach (var attack in mon.Attacks)
            {
                attackOptions.Add(attack);
            }

            Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>> attackAndCellOptions = DecideWhichAttacksInRange(attackOptions);
            if (attackAndCellOptions.Count > 0)
            {
                finalAttack = AttackManager.GetAttackSpecificBehavior(attackAndCellOptions, _aIControlledMonsterMap[mon], mon.ChooseAttackBehavior);
            }
            else finalAttack = (null, null);
            if (finalAttack == (null, null))
            {
                _actionComplete = true;
                return (null, new List<CombatMonster>(), new List<TileCell>());
            }
            SingleAttack chosenAttack = finalAttack.chosenAttack;
            List<CombatMonster> targets = finalAttack.attackCells.Keys.ToList();
            List<TileCell> affectedCells = finalAttack.attackCells.Values.SelectMany(c => c).ToList();
            return (chosenAttack, targets, affectedCells);

        }
        private Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>> DecideWhichAttacksInRange(List<SingleAttack> attacks)
        {
            Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>> attackDic = new Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>>();
            foreach (var attack in attacks)
            {
                Dictionary<CombatMonster, List<TileCell>> targetCells = GetPotentialAttackTargets(attack);
                if (targetCells.Count == 0)
                {
                    continue;
                }
                attackDic.Add(attack, targetCells);
            }
            return attackDic;
        }
        private Dictionary<CombatMonster, List<TileCell>> GetPotentialAttackTargets(SingleAttack attack)
        {
            CombatMonster attacker = _currentMonster;
            TileCell origin = _aIControlledMonsterMap[attacker];

            Dictionary<SingleAttack, List<CombatMonster>> attackTargets = new();
            List<TileCell> inRangeCells = TileManager.GetCellsInRange(origin, attack.Range);

            //This return a list of the cell (or cells if AOE) that this attack will target
            Dictionary<CombatMonster, List<TileCell>> targetCells = AttackManager.GetAttackSpecificBehavior(attack.Target, "Target", inRangeCells, origin);
            return targetCells;
        }






        private void ToggleIsDead()
        {

            foreach (var mon in _turnOrder)
            {
                if (mon.CurrentHealth <= 0)
                {
                    mon.isDead = true;
                    mon.Aspects.Clear();
                }
            }


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
            _currentMonster = _turnOrder.Peek();
        }






        private static void ResolveAspects(TickedTiming tick, CombatMonster mon)
        {
            if (mon.Aspects == null || mon.Aspects.Count == 0)
            {
                return;
            }
            AspectManager.ResolveAspect(mon, tick);

        
        }
        private static void UpdateMonsterTakingDamage(float delta)
        {
            foreach (var mon in _turnOrder)
            {
                if (mon.IsFlashingRed)
                {
                    mon.DamageFlashTimer -= delta;
                    if (mon.DamageFlashTimer <= 0f)
                    {
                        mon.IsFlashingRed = false;
                    }
                }
            }

        }




        private static void ResetClickValues(CombatMonster mon)
        {
            mon.CurrentAttackEffectedCells = null;
            mon.CurrentAttackEffectedMonsters = null;
            mon.attackPath1 = null;
            mon.attackPath2 = null;
            _playerSelectedSummon = null;
            _playerCurrentAttack = null;
            _playerCurrentAttackRangeOptions = null;

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
                    if (!_attackComplete && (mon.attackPath1 == null || mon.attackPath1.Count <= 0))
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

      
        private static void HandlePlayerTargetingAttackClick()
        {
            if (InputManager.IsLeftClick() && _playerCurrentAttackRangeOptions.Contains(_currentMouseHoverCell) && _currentMouseHoverCell.BlockedByMonster)
            {
                _playerTurnState = PlayerTurnState.PlayerExecutingAttack;
                _currentTarget = _currentMouseHoverCell;
                _attackComplete = false;
                SetPlayerAttackEffectCellsAndMonsters();
                
                CombatMonster mon = _turnOrder.Peek();
                mon.CurrentAttack = _playerCurrentAttack;
                SetAttackPathForPlayer();
                if (_playerCurrentAttack.Animated)
                _currentAttackVisualEffect = new VisualEffect(GetMonsterCurrentCell(mon), _playerCurrentAttack, FindCenterCell(mon.CurrentAttackEffectedCells ));
               
            }
        }
        private static void SetPlayerAttackEffectCellsAndMonsters()
        {
            UpdateMonsterCellMap();
            CombatMonster mon = _turnOrder.Peek();
            SingleAttack att = _playerCurrentAttack;

            mon.CurrentAttackEffectedCells = new List<TileCell> { _currentTarget };
            mon.CurrentAttackEffectedMonsters = new List<CombatMonster>();

            foreach (var kvp in _aIControlledMonsterMap)
            {
                CombatMonster aiMon = kvp.Key;
                TileCell aiCell = kvp.Value;

                if (mon.CurrentAttackEffectedCells.Contains(aiCell))
                {
                    mon.CurrentAttackEffectedMonsters.Add(aiMon);
                }
            }
        }
        private static void SetAttackPathForPlayer()
        {
            CombatMonster mon = _turnOrder.Peek();
            List<Vector2> path = NPCMovement.MoveMonsters(mon, _playerControlledMonsterMap[mon], FindCenterCell(mon.CurrentAttackEffectedCells));
            var paths = GridMovement.SplitAttackPath(path, _playerCurrentAttack);
            mon.attackPath1 = paths.Item1;
            mon.attackPath2 = paths.Item2;

        }
        private static void HandleDisplayAttackOptionsClick()
        {
                foreach (var (rect, attack) in _attackButtons)
                {
                    if (InputManager.IsLeftClick() && rect.Contains(_currentMousePos))
                    {
                    _playerCurrentAttack = attack;
                    CombatMonster mon = _turnOrder.Peek();
                    _playerCurrentAttackRangeOptions = TileManager.GetFloodFillTileWithinRange(GetMonsterCurrentCell(mon), _playerCurrentAttack.Range, includeMonsterTiles: true);
                    _playerTurnState = PlayerTurnState.PlayerTargeting;
                    }
                }
        }
        private static void HandleAttackRectClick()
        {
            if (InputManager.IsLeftClick() && _attackRect.Contains(_currentMousePos))
            {
                _playerTurnState = PlayerTurnState.PlayerAttacking;
            }
        }
        private static void HandleSummonDropdownClick()
        {
            if (_player.stats.UnlockedSummons == null || _player.stats.UnlockedSummons.Count == 0)
                return;

            for (int i = 0; i < _player.stats.UnlockedSummons.Count; i++)
            {
                var summOption = _player.stats.UnlockedSummons[i];

                Rectangle summonIconRect = new Rectangle(
                    _summonRect.X,
                    _summonRect.Y - ((i + 1) * (_summonOptionHeight + _summonOptionSpacing)),
                    _summonOptionWidth,
                    _summonOptionHeight
                );

                if (InputManager.IsLeftClick() && summonIconRect.Contains(_currentMousePos))
                {
                    _playerSelectedSummon = summOption;
                    return; // Exit early — don’t try to summon yet
                }
                if (InputManager.IsLeftClick() && _summonRect.Contains(_currentMousePos))
                {
                    _playerTurnState = PlayerTurnState.PlayerWaitingInput;
                }
            }

            // 2. If a summon is selected, and the player clicks a valid tile, summon it
            if (_playerSelectedSummon != null && InputManager.IsLeftClick())
            {
                TileCell clicked = TileManager.GetCell(_currentMousePos);
                if (_summonSpawnableCells.Contains(clicked))
                {
                    SummonSummonMonster(clicked);
                    _playerSelectedSummon = null;
                    _playerTurnState = PlayerTurnState.PlayerWaitingInput;
                }
            }
        }
        private static void HandlePlayerEndTurn()
        {
            if (_endTurnRect.Contains(_currentMousePos) && InputManager.IsLeftClick())
            {
                _movementComplete = false;
                EndTurn();
            }
        }

      

        public static void SummonSummonMonster(TileCell cell)
        {
            CombatMonster mon = _turnOrder.Peek();
            SummonedMonster sumMon = _playerSelectedSummon;
            int currentSP = mon.CurrentSP;
            if (sumMon.SummonCost > currentSP) 
            { 
                Add($"Need {sumMon.SummonCost} / have {currentSP}"); 
                return; 
            }
            _playerTurnState = PlayerTurnState.PlayerExecutingAction;
            CombatMonster comSumMon = (CombatMonsterManager.SummonMonsterToCombat(sumMon));
            comSumMon.CurrentCell = cell;
            comSumMon.currentPos = TileManager.GetCellCords(cell);
            AddComMonToTurnOrder(comSumMon);
            mon.CurrentSP -= comSumMon.BaseSummonCost;
            //maby pause the state -turn it to executing action before
        }
        private static void HandleMovementRectClick()
        {
            if (InputManager.IsLeftClick() && _moveRect.Contains(_currentMousePos))
            {
                _playerTurnState = PlayerTurnState.PlayerMoving;
            }
        }
        private static void HandlePlayerControlMoveClick(CombatMonster mon)
        {
            if (mon.CurrentMP > 0)
            {
                if (_playerMoveableCells.Contains(_currentMouseHoverCell) && InputManager.IsLeftClick())
                {
                    mon.PlayerMovementEndPoint = _currentMouseHoverCell;
                    _playerTurnState = PlayerTurnState.PlayerExecutingMove;
                }
            }
        }
        private static void HandleSummonRectClick()
        {
            if (_summonRect.Contains(_currentMousePos) && InputManager.IsLeftClick())
            {
                _playerTurnState = PlayerTurnState.PlayerSummoning;
            }
        }
       





      














       





      

        private static TileCell GetMonsterCurrentCell(CombatMonster mon)
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
        private static TileCell FindCenterCell(List<TileCell> cells)
        {
            if (cells.Count == 0) return null;
            if (cells.Count == 1) return cells[0];
            return cells[0];
        }




        public static Dictionary<CombatMonster, TileCell> GetCombatMonMap(string playeOrAI)
        {
            if (playeOrAI == "player")
            {
                return _playerControlledMonsterMap;
            }
            else if (playeOrAI == "ai")
            {
                return _aIControlledMonsterMap;
            }
            Add("ERROR IN GETCOMBATMONMAP");
            return _playerControlledMonsterMap;
        }






        public static List<TileCell> GetPathToPlayerSelectedCell(TileCell start, TileCell destination)
        {
            return GridMovement.FindPath(start, destination, int.MaxValue); // or -1 if your method supports it
        }




        private static void AddComMonToTurnOrder(CombatMonster mon)
        {
            List<CombatMonster> updatedList = new List<CombatMonster>();

            bool inserted = false;

            foreach (var combatMon in _turnOrder)
            {
                if (!inserted && mon.Initiation >= combatMon.Initiation)
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
            updatedList.Sort((a, b) => b.Initiation.CompareTo(a.Initiation));
            _referenceTurnOrder = updatedList;
        }





        public static void Add(string message)
        {
            _log.Add(message);

            // Keep it from growing forever
            if (_log.Count > _maxStrings)
                 _log.RemoveAt(0);
        }
        private static void OnScreenDebug(SpriteBatch spriteBatch)
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


        public static CombatMonster GetPlayerMonster()
        {
            return _playerMonster;
        }


    }
}

//    if (attackToTargets.Count <= 0)
//    {
//        return;
//    }
//    CombatMonster attacker = _turnOrder.Peek();

//    do
//    {
//        foreach (var pair in attackToTargets)
//        {
//            SingleAttack att = pair.Key;
//            List<CombatMonster> targets = pair.Value;

//            if (targets.Count > 0 && _attackPowerLeft > 0)
//            {
//                CombatMonster target = AttackManager.ChooseTarget(targets, att); // Pick first target for now
//                Add("Target chosen");
//                AttackManager.PerformAttack(att, attacker, target); // You’d implement this
//                Add("AttackPerformed");
//                attacker.AttackPower -= att.Cost;
//                Add($"Current AttackPower {_attackPowerLeft}");
//                break; // Prevent double-use per loop
//            }
//        }

//    } while (attacker.AttackPower > 0);
//}