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
using PlayingAround.Utils;
using PlayingAround.Visuals;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static CombatStateMachine;
using static PlayingAround.Entities.Monster.CombatMonsters.CombatMonster;
using static PlayingAround.Managers.SceneManager;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatManager
    {

        private static CombatMonster _playerMonster; // Need to update _player at the end of combat accordingly






        private static List<TileCell> _playerMoveableCells = new List<TileCell>();
        private static List<CombatMonster> _summonedMonsters = new List<CombatMonster>();





        //  private static CombatMonster _standInMonster = new CombatMonster();
        private static List<CombatMonster> _defeatedMonsters = new List<CombatMonster>();
        private static bool _firstRound = true;
        private static bool _actionComplete = false;
        private static float _playerBaseSpeed;
        private static int _playerBaseSP;
        private static bool _playerIsSummoning = false;
        private static int _summonOptionHeight = 64;
        private static int _summonOptionWidth = 64;
        private static int _summonOptionSpacing = 10;
        private static SingleAttack _drawnAttack = null;
        private static bool _attackAnimationBeforeHit;






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
        private Rectangle _summonRect, _attackRect, _endTurnRect, _moveRect, _attackOptionsRect;
        private Dictionary<CombatMonster, Rectangle> _displayStatRectangles = new Dictionary<CombatMonster, Rectangle>();
        private Rectangle _endScreenRect = new Rectangle(710, 440, 500, 200);
        private Rectangle _exitCombatButtonRect = new Rectangle(885, 580, 150, 50);

        private PlayMonsters _playMonsters; // kept as reference as needed
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
        private SummonedMonster _playerSelectedSummon;
        private CombatMonster _currentMonster;

        private SingleAttack _playerCurrentAttack;
        public WhoWon TheWinner = WhoWon.None;


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

            _playMonsters = playMonsters;
            _player = player;
            _playerMonster = new CombatMonster(player);
            _playerMonster.DrawEnlargementFacetor = 20;
           // _playerMonster.Initiation = 5;
            SetSpawnableCells();
            SetCombatMonsterStartingPos();
            SetTurnOrder();
            UpdateCurrentMonster();
            InitilizeUIElements();

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
            allCombatants.AddRange(_playMonsters.Monsters);
            allCombatants.Add(_playerMonster);
            allCombatants.AddRange(_summonedMonsters);
            allCombatants.Sort((a, b) => b.Initiation.CompareTo((int)a.Initiation));
            int idCounter = 0;
            foreach (var entity in allCombatants)
            {
                entity.CurrentMP = entity.MP;
                entity.ID = idCounter++;
                _referenceTurnOrder.Add(entity);
                _turnOrder.Enqueue(entity);
            }

        }
        private void SetCombatMonsterStartingPos()
        {
            if (_monsterSpawnableCells.Count < _playMonsters.Monsters.Count) return;
            Random ran = new Random();
            List<TileCell> spawnableCells = new List<TileCell>(_monsterSpawnableCells);
            List<CombatMonster> comMon = new List<CombatMonster>(_playMonsters.Monsters);
            do
            {
                foreach (var mon in comMon)
                {
                    int index = ran.Next(spawnableCells.Count);
                    Vector2 pos = spawnableCells[index].CenterPoint;
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
                    DrawCombatEndScreen(spriteBatch);
                    break;
            }
            DrawDebugInfo(spriteBatch);
            DrawDisplayStats(spriteBatch);
            DrawStatHoverHighlight(spriteBatch);
            _visualEffectManager.Draw(spriteBatch, _font);
            DrawAllCombatMonsters(spriteBatch);
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
                Dictionary<string, int> defeated = CountDefeatedMonsters();
                int yOffset = 70;

                foreach (var entry in defeated)
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
                string textureKey = mon.IsSummon ? mon.IconTextureKey : (mon.isPlayer || mon.isSummoned ? "Hero_Blonde" : mon.IconTextureKey);
                Texture2D icon = AssetManager.GetTexture(textureKey);

                // Set color depending on isDead
                Color col = mon.IsSummon ? new Color(Color.White, 0.8f) : Color.White;
                if (mon.isDead)
                    col = Color.Gray * 0.4f;

                if (mon == _currentMonster) spriteBatch.Draw(_playerCellOptions, iconRect, col);
                // Draw monster icon
                spriteBatch.Draw(icon, iconRect, col);



                // Draw health below
                float currentHealth = MathF.Max(0, mon.CurrentHealth);
                string hpText = $"{currentHealth} / {mon.BaseHealth}";
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
                string mpText = $"MP: {mon.CurrentMP} / {mon.MP}";
                Vector2 mpTextSize = font.MeasureString(mpText);
                Vector2 mpTextPos = new Vector2(
                    iconRect.X + (iconSize - mpTextSize.X) / 2,
                    textPos.Y + textSize.Y + 2
                );
                spriteBatch.DrawString(font, mpText, mpTextPos, Color.Blue);

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
                DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, _playerMonster);
        }
        private void DrawEntityPreviewOnCell(SpriteBatch spriteBatch, TileCell cell, CombatMonster mon, Color col = default )
        {
            if (mon == null) return;
            Texture2D texture = mon.IconTexture;

            if (_currentMonster != null && _currentMonster.isSummoned) texture = _currentMonster.IconTexture;
            Vector2 coords = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint);
            Rectangle rect = new Rectangle((int)coords.X, (int)coords.Y, 64, 64);
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

                Vector2 drawPoint = TileManager.OffSetFromCenterOfDiamond(combatMon.currentPos, combatMon.DrawEnlargementFacetor);

                
                Rectangle destination = new Rectangle((int)drawPoint.X , (int)drawPoint.Y , 64 + combatMon.DrawEnlargementFacetor, 64 + combatMon.DrawEnlargementFacetor  );

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
            if (_currentMonster.CurrentMP > 0)
            {
                foreach (var cell in _playerMoveableCells)
                {
                    if (cell.BlockedByMonster || !cell.IsWalkable) continue;
                    DrawCellHighlight(spriteBatch, cell, Color.Green, 5);
                }

                if (_currentMouseHoverCell != null && _playerMoveableCells.Contains(_currentMouseHoverCell))
                {
                    DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, _playerMonster);
                }
            }
        }
        private void DrawPlayClickedSummonButton(SpriteBatch spriteBatch)
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
                Vector2 cellCoords = TileManager.OffSetFromCenterOfDiamond(cell.CenterPoint);
                Rectangle cellRect = new Rectangle((int)cellCoords.X, (int)cellCoords.Y, _tileWidth - 5, _tileHeight - 5);
                DrawCellHighlight(spriteBatch, cell, Color.LimeGreen, 5);
            }

        }
        private void DrawSummonHover(SpriteBatch spriteBatch)
        {
            if (!_summonSpawnableCells.Contains(_currentMouseHoverCell)) return;

            // Step 2: Only if we're hovering over a summonable cell, draw the summon icon
            if (_currentMouseHoverCell != null && _summonSpawnableCells.Contains(_currentMouseHoverCell))
            {
                Vector2 hoverCoords = TileManager.OffSetFromCenterOfDiamond(_currentMouseHoverCell);
                Rectangle hoverRect = new Rectangle((int)hoverCoords.X, (int)hoverCoords.Y, _tileWidth, _tileHeight);

                DrawEntityPreviewOnCell(spriteBatch, _currentMouseHoverCell, new CombatMonster(_playerSelectedSummon.IconTextureString));
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
            if (WinnerChosen()) { SetCombatState(CombatState.WinnerChosen); return; }
                switch (StateCombat)
            {
                case CombatState.TurnStart:

  
                   
                    SkipMonsterIfDead(); // dequees and requeues monster if dead
                    _currentMonster.TurnNumber++;
                    UpdateMonsterTopOfRoundStats();
                    PickWhichEntitiesTurn();
                    break;
                case CombatState.AITurn:

                    switch (StateAI)
                    {
                        case (AITurnState.None):
                            
                            break;
                        case AITurnState.ActionNavigation:
                            if (CheckIfAIShouldEndTurn()) return;
                            DecideAINextAction();
                            break;
                        case AITurnState.MovingAIControlled:
                            
                            SetAITurnState(AITurnState.ExecutingMove);
                            break;
                        case AITurnState.ExecutingMove:
                            if (MonsterFinishedMoving()) { FinishedMoving(); AIFinishedAction(); }
                            break;
                        case AITurnState.AIAttacking:
                            SetAITurnState(AITurnState.ExecutingAttack);
                            if (AICanAttack()) _attackComplete = false;

                            break;
                        case AITurnState.ExecutingAttack:
                            if (_attackComplete) { 
                                AIFinishedAction(); return; }
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
            if (_playerMonster.isDead){ SetWhoWon(WhoWon.Monster);  return true; }
            return false;
        }
        private void FinishedMoving()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentMP -= (int)_numberOfCellsMoved;
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
            if (mon.CurrentOrderOfActions.Count <= 0)
            {
                EndTurn();
                return true;
            }
            return false;
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
        public void DecideAINextAction()
        {
            CombatMonster mon = _currentMonster;
            MonsterActionOrder action = mon.CurrentOrderOfActions.Peek();

            switch (action)
            {
                case MonsterActionOrder.MoveTowardsClosestEnemy:
                    GetMovementCellPathToClosestEnemy();
                    SetAITurnState(AITurnState.MovingAIControlled);
                    break;

                case MonsterActionOrder.AttackClosestEnemy:
                    AttackClosestEnemy();
                    SetAITurnState(AITurnState.AIAttacking);
                    break;
                case MonsterActionOrder.AttackSelf:
                    AttackSelf();
                    SetAITurnState(AITurnState.AIAttacking);
                    break;


            }

        }
        private void AttackSelf()
        {
            CombatMonster mon = _currentMonster;
            SingleAttack attack = mon.Attacks.First();
            SetMonsterAttackPathingInformation(attack, new List<CombatMonster>() { mon }, new List<TileCell>() { GetMonsterCurrentCell(mon) });

        }
        private void AttackClosestEnemy()
        {
            CombatMonster mon = _currentMonster;
            TileCell origin = _aIControlledMonsterMap[mon];

            //inRangeMap send any and all attacks that have a valid range, including non monster cells
            var inRangeMap = GetInRangeCellsByAttack(mon.Attacks, origin);
            if (inRangeMap.Count == 0)
            {
                AIFinishedAction();
                return;
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

            if (targetData.Count == 0)
            {
                AIFinishedAction();
                return;
            }

            // Use the selected targeting behavior to pick the final attack
            var (chosenAttack, chosenMap) = AttackManager.ChooseWhichAttack(targetData, origin, mon.CurrentChooseWhichAttack);

            if (chosenAttack == null || chosenMap == null)
            {
                AIFinishedAction();
                return;
            }

            SetMonsterAttackPathingInformation(
                chosenAttack,
                chosenMap.Keys.ToList(),
                chosenMap.Values.SelectMany(x => x).ToList()
            );
        }

        private void SetMonsterAttackPathingInformation(SingleAttack att, List<CombatMonster> mons, List<TileCell> cells)
        {
            // this method will set the monster property of attack path(s), current attacks, specified current target info, and visualeffect
            CombatMonster mon = _currentMonster;
            var attackDetails = (att, mons, cells);
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





        private void GetMovementCellPathToClosestEnemy()
        {
            if (AIHasMP())
            {
                CombatMonster mon = _currentMonster;
                TileCell currentCell = _aIControlledMonsterMap[mon];

                List<TileCell> playerControlledCells = _playerControlledMonsterMap
                     .Select(pair => pair.Value)
                     .Where(cell => cell != null)
                     .ToList();

                // If no targets or already adjacent, return current position
                if (TileManager.IsNeighbor(playerControlledCells, currentCell))
                    return;

                List<TileCell> listOfCellsPathToTarget = GridMovement.FindClosestTargetPath(currentCell, playerControlledCells, (int)mon.CurrentMP);
                GenerateMovementPath(listOfCellsPathToTarget);
            }
        }
        private void GenerateMovementPath(List<TileCell> tileCellPath)
        {
            CombatMonster mon = _currentMonster;
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
                List<Vector2> arc = NPCMovement.MoveMonsters(mon, startingCell, endPos);
                fullVectorPath.AddRange(arc);
                startingCell = endPos;

            }

            mon.MovePath = fullVectorPath;

        }
        public bool AIHasMP() => _currentMonster.CurrentMP >= 0;
        public bool MonsterFinishedMoving() =>  _currentMonster.MovePath == null || _currentMonster.MovePath.Count <= 0;
        public bool AICanAttack() => _currentMonster.CurrentAttack != null;
        public bool PlayerHasEndPoint() => _currentMonster.PlayerMovementEndPoint != null;
        public bool PlayerHasMovePath() => _currentMonster.MovePath.Count > 0;
        private void AIFinishedAction()
        {
            CombatMonster mon = _currentMonster;
            if (mon.CurrentOrderOfActions.Count > 0) mon.CurrentOrderOfActions.Dequeue();
            SetAITurnState(AITurnState.ActionNavigation);
        }
        private void AIFinishedAttack()
        {
            _attackComplete = true;
            _attackPerformed = false;
            
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
                    if (StateSummoned != SummonedTurnState.SummonedExecutingAttack)
                    SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack);
                }
                return;

            }
            else if (!_attackPerformed)
            {
                _attackPerformed = true;
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
                if (StateCombat == CombatState.SummonedTurn) { SetSummonedTurnState(SummonedTurnState.SummonedExecutingAttack); }
                return;
            }
            //attack and movement associated it if finished, so go to next turn state
            if (mon.isPlayer)
            {
                SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
            }
            else if (mon.isSummoned)
            {
                SummonedFinishedAttack();

            }

            else if (mon.isMonster)
            {
                AIFinishedAttack();
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
                SetCombatState(CombatState.ExitingCombat);
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
                    HandleMovementRectClick();
                    HandleAttackRectClick();
                    ResetClickValues();
                    break;
                case SummonedTurnState.SummonedClickedAttackButton:
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
                    
                    //case PlayerTurnState.PlayerExecutingMove:

                    //    break;

                    //case PlayerTurnState.PlayerTargeting:

                    //    break;
                    //case PlayerTurnState.PlayerClickedMoveButton:
                    //    HandleMovementRectClick();
                    //    UpdatePlayerClickedMoveDestination();
                    //    break;
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
                    HandleMovementRectClick();
                    ResetClickValues();
                    break;

                case PlayerTurnState.PlayerClickedSummonButton:
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
            if (_playerSelectedSummon != null && _summonSpawnableCells.Contains(cell))
            { 
                    SummonSummonMonster(cell);
                    _playerSelectedSummon = null;
                    SetPlayerTurnState(PlayerTurnState.PlayerExecutingSummoning);
                
            }
        }
        private void ResetClickValues()
        {
            CombatMonster mon = _currentMonster;
            mon.CurrentAttackEffectedCells = null;
            mon.CurrentAttackEffectedMonsters = null;
            mon.attackPath1 = null;
            mon.attackPath2 = null;
            _playerSelectedSummon = null;
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
                _playerMonster.currentPos = _currentMouseHoverCell.CenterPoint;
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
            if (mon.isMonster)
            {
                foreach (var str in mon.BaseChooseWhichAttack) {
                    mon.CurrentChooseWhichAttack.Enqueue(str);
                        }
                foreach (var str in mon.BaseOrderOfActions)
                {
                    mon.CurrentOrderOfActions.Enqueue(str);
                }
            }
        }
        private void PickWhichEntitiesTurn()
        {
            CombatMonster mon = _currentMonster;
            if (mon.isPlayer)
            {
                SetCombatState(CombatState.PlayerTurn);
                SetPlayerTurnState(PlayerTurnState.PlayerWaitingInput);
                mon.PlayerMovementEndPoint = null;
                return;
            }
            else if (mon.isSummoned)
            {
                SetCombatState(CombatState.SummonedTurn);
                SetSummonedTurnState(SummonedTurnState.SummonedWaitingInput);
                mon.PlayerMovementEndPoint = null;
                return;
            }
            else if (mon.isMonster)
            {
                SetCombatState(CombatState.AITurn);
                SetAITurnState(AITurnState.ActionNavigation);
                return;
            }
            SetCombatState(CombatState.Debug);
}
        //private void DecideOrderOfOperations()
        //{
        //    CombatMonster mon = _currentMonster;
        //    if (mon.isPlayer || mon.isSummoned) { return; }

        //    if (mon.TurnBehavior == "getCloseAsPossible")
        //    {
        //        mon.OrderOfActions = new Queue<string>(new[] { "moveClose", "attack" });
        //    }

        //}
     
      
     







        private void UpdateMonsterTakingDamage(float delta)
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
                if (mon.CurrentHealth <= 0)
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
                mon.CurrentAttack = _playerCurrentAttack;
                SetAttackPathForPlayer();
                if (_playerCurrentAttack.Animated)
                    _currentAttackVisualEffect = new VisualEffect(GetMonsterCurrentCell(mon), _playerCurrentAttack, FindCenterCell(mon.CurrentAttackEffectedCells));

            }
        }
        private void SetPlayerAttackEffectCellsAndMonsters(TileCell targetCell)
        {
            CombatMonster mon = _currentMonster;
            SingleAttack att = _playerCurrentAttack;
            TileCell target = targetCell;

            mon.CurrentAttackEffectedCells = new List<TileCell> { target };
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
        private void SetAttackPathForPlayer()
        {
            CombatMonster mon = _currentMonster;
            List<Vector2> path = NPCMovement.MoveMonsters(mon, _playerControlledMonsterMap[mon], FindCenterCell(mon.CurrentAttackEffectedCells));
            var paths = GridMovement.SplitAttackPath(path, _playerCurrentAttack);
            mon.attackPath1 = paths.Item1;
            mon.attackPath2 = paths.Item2;

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
                    SetPlayerTurnState(PlayerTurnState.PlayerClickedSpecificSummoned);
                    return; // Exit early — don’t try to summon yet
                }

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
            CombatMonster mon = _currentMonster;
            SummonedMonster sumMon = _playerSelectedSummon;
            int currentSP = mon.CurrentSP;
            if (sumMon.SummonCost > currentSP) 
            { 
                Add($"Need {sumMon.SummonCost} / have {currentSP}"); 
                return; 
            }

            CombatMonster comSumMon = (CombatMonsterManager.SummonMonsterToCombat(sumMon));
            comSumMon.CurrentCell = cell;
            comSumMon.currentPos = cell.CenterPoint;
            AddComMonToTurnOrder(comSumMon);
            mon.CurrentSP -= comSumMon.BaseSummonCost;
            //maby pause the state -turn it to executing action before
        }
        private void AddComMonToTurnOrder(CombatMonster mon)
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
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var mon in _referenceTurnOrder)
            {
                if (mon.isDead && mon.isMonster)
                {
                    if (dict.ContainsKey(mon.Name))
                    {
                        dict[mon.Name]++;
                    }
                    else
                    {
                         dict.Add(mon.Name, 1);
                    }
                }
            }
            return dict;
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
            return _playerMonster;
        }


    }
}
public enum WhoWon
{
    None,
    Player,
    Monster
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