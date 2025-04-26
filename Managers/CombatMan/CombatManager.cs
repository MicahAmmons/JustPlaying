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
using PlayingAround.Managers.CombatMan.CombatAttacks;
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Movement.CombatGrid;
using PlayingAround.Managers.Proximity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static PlayingAround.Managers.SceneManager;

namespace PlayingAround.Managers.CombatMan
{
    public static class CombatManager
    {
        private static PlayMonsters _playMonsters;//brought in at start, deleted or ignored at end
        private static Player _player; // Copied to local _playerMonster
        private static CombatMonster _playerMonster; // Need to update _player at the end of combat accordingly
        private static MapTile _currentMapTile;
        private static Texture2D _playerCellOptions;//placeholder texture
        private static SpriteFont _font;
        private static TileCell _currentClickedCell;
        private static TileCell _currentMouseHoverCell;
        private static Vector2 _currentMousePos;
        private static List<TileCell> _heroSpawnableCells = new List<TileCell>();
        private static List<TileCell> _monsterSpawnableCells = new List<TileCell>();
        private static List<TileCell> _playerMoveableCells = new List<TileCell>();  
        private static List<CombatMonster> _summonedMonsters = new List<CombatMonster>();
        private static Queue<CombatMonster> _turnOrder = new Queue<CombatMonster>();
        private static int _startingSpeed;
        private static int? _numberOfCellsMoved = 0;
        private static TileCell _currentTarget;
        private static bool _turnEnd = false;
        private static int _attackPowerLeft;
        private static int _test = 0;
        private static Dictionary<CombatMonster, TileCell> _playerControlledMonsterMap = new();
        private static Dictionary<CombatMonster, TileCell> _aIControlledMonsterMap = new();
        private static CombatMonster _standInMonster = new CombatMonster();
        private static List<CombatMonster> _defeatedMonsters = new List<CombatMonster>();
        private static bool _firstRound = true;
        private static bool _actionComplete = false;
        private static bool _movementComplete = false;
        private static float _playerBaseSpeed;
        private static bool _playerIsSummoning = false;



        private static List<string> _log = new List<string>();
        private static int _maxStrings = 50;

        private static Rectangle _backBackGroundButtonOptions = new Rectangle(1600, 720, 200, 100);
        private static Rectangle _summonRect, _attackRect, _endTurnRect;


        public enum CombatState
        {
            Waiting,
            LocationSelection,
            TurnStart, // Once at the top of each turn 
            ActionNavigation,
            MovingPlayerControlled,
            MovingAIControlled,
            AIAttacking,
            ExecutingAttack,
            AwaitingPlayerInput,
            PlayerTurn,

            ResolvingEffects,
            EndingTurn,
            CombatOver

        }
        private static PlayerTurnState _playerTurnState = PlayerTurnState.PlayerWaitingInput;
        private enum PlayerTurnState
        {
            PlayerWaitingInput,
            PlayerExecutingAction,
            PlayerSummoning,
            PlayerAttacking,
            PlayerEndingTurn,
        }

        private static CombatState _currentState = CombatState.Waiting;
        public static CombatState CurrentState => _currentState;

        public static void Initialize()
        {
            _currentMapTile = TileManager.CurrentMapTile;
            _playerCellOptions = AssetManager.GetTexture("fightBackground");
            _font = AssetManager.GetFont("mainFont");
            InitilizeUIElements();
        }

        public static void BeginCombat(PlayMonsters playMonsters, Player player)
        {
            Add("BeginCombat");

            _playMonsters = playMonsters;
            _player = player;
            _playerBaseSpeed = player.Speed;
            _playerMonster = new CombatMonster(player);
            _playerMonster.PathGenerated = false;
            SetTurnOrder();
            FindSpawnablCellsForPlayerAndMons();
            SetCombatMonsterStartingPos();
            CombatManager.SetState(CombatState.LocationSelection);
            Add("State = LocationSelection");
        }

        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            
            OnScreenDebug(spriteBatch);
            // 🆕 Display current PlayerTurnState at top-right
            if (SceneManager.CurrentState == SceneState.Combat)
            {
                string stateText = $"State: {_playerTurnState}";
                SpriteFont font = AssetManager.GetFont("mainFont");
                Vector2 textSize = font.MeasureString(stateText);
                int screenWidth = graphicsDevice.Viewport.Width;

                Vector2 position = new Vector2(screenWidth - textSize.X - 20, 10);
                spriteBatch.DrawString(font, stateText, position, Color.Orange);
            }
            if (SceneManager.CurrentState != SceneState.Combat || _currentState == CombatState.Waiting)
                return;
            if (_turnOrder != null && _turnOrder.Count > 0)
                DisplayStats(spriteBatch);

            if (_currentState == CombatState.LocationSelection)
                DrawLocationSelection(spriteBatch);

            if (_currentState == CombatState.PlayerTurn)
                DrawPlayerTurn(spriteBatch);

            DrawAllCombatMonsters(spriteBatch);
        }

        public static void UpdateInput(GameTime gameTime)
        {
            _currentMousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);
            _currentMouseHoverCell = TileManager.GetCell(_currentMousePos);

            switch (_currentState)
            {
                case CombatState.LocationSelection:
                    HandleLocationSelectionInput();
                    break;

                case CombatState.PlayerTurn:
                    HandlePlayerTurnInput();
                    break;

                    // Add more as needed (Attacking, Summoning, etc.)
            }
            if (InputManager.IsRightClick())
            {
                _playerTurnState = PlayerTurnState.PlayerWaitingInput;
            }
        }

        public static void Update(GameTime gameTime)
        {
            if (SceneManager.CurrentState == SceneState.Combat)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                UpdateDefeatedMonsters();
                UpdateInput(gameTime);
                UpdateMouseHoverCell();

                if (_currentState == CombatState.TurnStart) // Have to remove the previous turn before calling TurnStart, it'll reset everything
                {
                    UpdateMonsterCellMap();
                    _turnOrder.Peek().TurnNumber++;
                    CopyCurrentMonToStandin();

                    if (_standInMonster.isPlayerControled) // Don't use standinMonster for this turn, player controls
                    {
                        
                        SetState(CombatState.PlayerTurn);
                        _turnEnd = false;
                        _turnOrder.Peek().PlayerMovementEndPoint = null;
                        _playerTurnState = PlayerTurnState.PlayerWaitingInput;
                    }

                    else if (!_standInMonster.isPlayerControled)
                    {
                        DecideOrderOfOperations(); Add("Order Of Ops Set");
                        Add("State = ActionNAvigation");
                        CombatManager.SetState(CombatState.ActionNavigation);
                    }
                }

                if (_currentState == CombatState.ActionNavigation)
                {
                    UpdateMonsterCellMap();
                    if (_standInMonster.OrderOfActions.Count == 0)
                    {
                        Add("OrderOfAction is 00000");
                        CombatMonster mon = _turnOrder.Dequeue();
                        _turnOrder.Enqueue(mon);
                        SetState(CombatState.TurnStart);
                    }
                    else if (_standInMonster.OrderOfActions.Peek() == "moveClose")
                    {
                        Add($"mon.{_turnOrder.Peek().ID} moveCloser Begin. State = MovingAIControlled");
                        SetState(CombatState.MovingAIControlled);
                    }
                    else if (_standInMonster.OrderOfActions.Peek() == "moveFurther")
                    {

                    }
                    else if (_standInMonster.OrderOfActions.Peek() == "attack")
                    {
                        Add($"mon.{_turnOrder.Peek().ID} Attack Begin. State = AIAttacking");
                        SetState(CombatState.AIAttacking);
                    }
                    else if (_standInMonster.OrderOfActions.Count == 0)
                    {
                        SetState(CombatState.Waiting);
                    }

                }


                if (_currentState == CombatState.MovingAIControlled)
                {
                    UpdateMonsterCellMap();
                    if (!_movementComplete)
                    {
                        MoveMonster(delta);
                        return;
                    }
                    _movementComplete = false;
                    _standInMonster.OrderOfActions.Dequeue();
                    _currentState = CombatState.ActionNavigation;

                }
                if (_currentState == CombatState.AIAttacking)
                {
                    UpdateMonsterCellMap();
                    if (_standInMonster.AttackPower > 0)
                    {
                        _currentState = CombatState.ExecutingAttack;
                        _actionComplete = false;
                        AttackAIMonster(delta);
                        return;
                    }
                    Add("Dequeing Attack");
                    _standInMonster.OrderOfActions.Dequeue();
                    _currentState = CombatState.ActionNavigation;
                }
                if (_currentState == CombatState.ExecutingAttack)
                {
                    if (_actionComplete)
                    {
                        _actionComplete = false;
                        _standInMonster.OrderOfActions.Dequeue();
                        _currentState = CombatState.ActionNavigation;
                    }
                }
                if (_currentState == CombatState.PlayerTurn)
                {
                    UpdateMonsterCellMap();
                    UpdatePlayerMoveableCells();
                    if (_turnEnd)
                    {
                        _currentState = CombatState.TurnStart;
                        _turnEnd = false;
                    }

                    CombatMonster mon = _turnOrder.Peek();

                    if (mon.PlayerMovementEndPoint != null)
                    {
                        Add("GeneratedPath");
                        PopulatePath(delta, mon.PlayerMovementEndPoint);
                        mon.PlayerMovementEndPoint = null;
                    }
                    if (mon.PathGenerated)
                    { 
                        ExecuteMovementPath(delta, mon);

                    }
                }
            }
        }
        private static void HandleLocationSelectionInput()
        {
            if (_heroSpawnableCells.Contains(_currentMouseHoverCell) && InputManager.IsLeftClick())
            {
                _playerMonster.currentPos = TileManager.GetCellCords(_currentMouseHoverCell);
                SetState(CombatState.TurnStart);
                UpdateMonsterCellMap();
                Add("State = TurnStart");
            }
        }

        private static void HandlePlayerTurnInput()
        {
            CombatMonster mon = _turnOrder.Peek();
            switch (_playerTurnState)
            {
                case PlayerTurnState.PlayerWaitingInput:
                    HandleMovementClick(mon);
                    break;

                case PlayerTurnState.PlayerSummoning:
                    HandleSummonDropdownClick();
                    break;

                case PlayerTurnState.PlayerAttacking:
                    //HandleAttackTargetingClick();
                    break;

                case PlayerTurnState.PlayerExecutingAction:
                    // Optional — maybe wait for animations or effects to finish
                    break;

                case PlayerTurnState.PlayerEndingTurn:
                    // Optional — confirm dialog, visual delay, etc.
                    break;
            }
        }

        private static void HandleSummonDropdownClick()
        {
           
        }
        private static void HandleMovementClick(CombatMonster mon)
        {
            if (_standInMonster.Speed > 0)
            {
                if (_playerMoveableCells.Contains(_currentMouseHoverCell) && InputManager.IsLeftClick())
                {
                    mon.PlayerMovementEndPoint = _currentMouseHoverCell;
                    _playerTurnState = PlayerTurnState.PlayerExecutingAction;
                }
            }
                if (_summonRect.Contains(_currentMousePos) && InputManager.IsLeftClick())
                {
                    _playerTurnState = PlayerTurnState.PlayerSummoning;
                }
            
        }
        private static void DrawPlayerTurn(SpriteBatch spriteBatch)
        {
            if (_playerTurnState == PlayerTurnState.PlayerWaitingInput || _playerTurnState == PlayerTurnState.PlayerSummoning)
            {
                DrawPlayerButtonOptions(spriteBatch);
                if (_playerTurnState != PlayerTurnState.PlayerSummoning) 
                {
                    if (_standInMonster.Speed > 0)
                    {
                        foreach (var cell in _playerMoveableCells)
                        {
                            if (cell.BlockedByMonster || !cell.IsWalkable) continue;
                            DrawCellHighlight(spriteBatch, cell, Color.Black);
                        }

                        if (_currentMouseHoverCell != null && _playerMoveableCells.Contains(_currentMouseHoverCell))
                            DrawHeroPreviewOnCell(spriteBatch, _currentMouseHoverCell);
                    } 
                }
                if (_playerTurnState == PlayerTurnState.PlayerSummoning)
                {
                    DrawSummonOptions(spriteBatch);
                   
                }


            }
        }

        private static void InitilizeUIElements()
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

            _summonRect = new Rectangle(
                _attackRect.X,
                _attackRect.Y - buttonHeight - spacing,
                _endTurnRect.Width,
                buttonHeight);
        }
        private static void DrawLocationSelection(SpriteBatch spriteBatch)
        {
            DrawSpawnableTiles(spriteBatch);

            if (_currentMouseHoverCell != null && _currentMouseHoverCell.HeroSpawnable)
                DrawHeroPreviewOnCell(spriteBatch, _currentMouseHoverCell);
        }

        private static void DrawSummonOptions(SpriteBatch spriteBatch)
        {
            CombatMonster mon = _turnOrder.Peek();

            if (_player.stats.UnlockedSummons != null && _player.stats.UnlockedSummons.Count > 0)
            {
                int summonOptionHeight = 64;
                int summonOptionWidth = 64;
                int spacing = 10;

                for (int i = 0; i < _player.stats.UnlockedSummons.Count; i++)
                {
                    var summOption = _player.stats.UnlockedSummons[i];
                    Texture2D texture = summOption.IconTexture;

                    Rectangle summonIconRect = new Rectangle(
                        _summonRect.X,
                        _summonRect.Y - ((i + 1) * (summonOptionHeight + spacing)),
                        summonOptionWidth,
                        summonOptionHeight
                    );

                    spriteBatch.Draw(texture, summonIconRect, Color.White);

                    // Optional: draw border or hover highlight
                    if (summonIconRect.Contains(_currentMousePos))
                        spriteBatch.Draw(_playerCellOptions, summonIconRect, Color.Yellow * 0.4f);
                }
            }
        }
        private static void DrawAllCombatMonsters(SpriteBatch spriteBatch)
        {
            foreach (var combatMon in _turnOrder)
            {
                Rectangle destination = new Rectangle(
                    (int)(combatMon.currentPos.X),
                    (int)(combatMon.currentPos.Y),
                    32, 32);

                string textureKey = combatMon.isPlayerControled ? "Hero_Blonde" : combatMon.IconPath;
                spriteBatch.Draw(AssetManager.GetTexture(textureKey), destination, Color.White);
            }
        }
        private static void DrawCellHighlight(SpriteBatch spriteBatch, TileCell cell, Color color)
        {
            Vector2 coords = TileManager.GetCellCords(cell);
            Rectangle rect = new Rectangle((int)coords.X, (int)coords.Y, 32, 32);
            spriteBatch.Draw(_playerCellOptions, rect, color);
        }
        private static void DrawHeroPreviewOnCell(SpriteBatch spriteBatch, TileCell cell)
        {
            Vector2 coords = TileManager.GetCellCords(cell);
            Rectangle rect = new Rectangle((int)coords.X, (int)coords.Y, 32, 32);
            spriteBatch.Draw(_player.Texture, rect, Color.White);
        }
        private static void DrawSpawnableTiles(SpriteBatch spriteBatch)
        {
            foreach (var tile in _heroSpawnableCells)
                DrawCellHighlight(spriteBatch, tile, Color.White);

            foreach (var tile in _monsterSpawnableCells)
                DrawCellHighlight(spriteBatch, tile, Color.Black);
        }

        private static void DrawPlayerButtonOptions(SpriteBatch spriteBatch)
        {
            DrawButton(spriteBatch, _summonRect, "Summon");
            DrawButton(spriteBatch, _attackRect, "Attack");
            DrawButton(spriteBatch, _endTurnRect, "End Turn");
            
        }
        private static void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string label)
        {
            spriteBatch.Draw(_playerCellOptions, rect, Color.Aqua);

            Vector2 textSize = _font.MeasureString(label);
            Vector2 textPosition = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2,
                rect.Y + (rect.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, label, textPosition, Color.White);
        }

        private static void UpdatePlayerMoveableCells()
        {
            CombatMonster mon = _turnOrder.Peek();
            TileCell origin = _playerControlledMonsterMap[mon];

            List<TileCell> cells = TileManager.GetFloodFillTileWithinRange(origin, _standInMonster.Speed);

            _playerMoveableCells = cells;
        }
        private static void AttackAIMonster(float delta)
        {
            Add("Deciding what attack)");
            AIMonsterAttack(delta);

        }

        public static void AIMonsterAttack(float delta)
        {
            (SingleAttack chosenAttack, Dictionary<CombatMonster, List<TileCell>> attackCells) finalAttack;
            CombatMonster mon = _turnOrder.Peek();
            List<SingleAttack> attackOptions = new List<SingleAttack>();
            foreach (var attack in mon.Attacks.Attacks)
            {
                if (attack.Cost <= _standInMonster.AttackPower)
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
                return;
            }
            SingleAttack chosenAttack = finalAttack.chosenAttack;
            List<CombatMonster> targets = finalAttack.attackCells.Keys.ToList();
            List<TileCell> affectedCells = finalAttack.attackCells.Values.SelectMany(c => c).ToList();
            AttackManager.PerformAttack(chosenAttack, mon, targets, affectedCells);
            _standInMonster.AttackPower -= chosenAttack.Cost;
            _actionComplete = true;
        }
        private static Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>> DecideWhichAttacksInRange(List<SingleAttack> attacks)
        {
            Dictionary<SingleAttack, Dictionary<CombatMonster , List<TileCell>>> attackDic = new Dictionary<SingleAttack, Dictionary<CombatMonster, List<TileCell>>>();
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
        private static Dictionary<CombatMonster, List<TileCell>> GetPotentialAttackTargets(SingleAttack attack)
        {
            CombatMonster attacker = _turnOrder.Peek();
            TileCell origin = _aIControlledMonsterMap[attacker];

            Dictionary <SingleAttack, List<CombatMonster>> attackTargets = new();
            List<TileCell> inRangeCells = TileManager.GetCellsInRange(origin, attack.Range);

            //This return a list of the cell (or cells if AOE) that this attack will target
            Dictionary<CombatMonster, List<TileCell>> targetCells = AttackManager.GetAttackSpecificBehavior(attack.Target, "Target", inRangeCells, origin);
            return targetCells;
        }
        public static void UpdateMonsterCellMap()
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
                if (mon.isPlayerControled)
                {
                    cell.BlockedByMonster = true;
                    _playerControlledMonsterMap[mon] = cell;
                }
                else if (!mon.isPlayerControled)
                {
                    cell.BlockedByMonster = true;
                    _aIControlledMonsterMap[mon] = cell;
                }

            }
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
        private static void MoveMonster(float delta, CombatMonster mon = null)
        {
            if (mon == null) 
            { 
                mon = _turnOrder.Peek(); 
            }
            if (!mon.PathGenerated && _standInMonster.Speed >= 0)
                GenerateMovementPath(mon); ;
            
            ExecuteMovementPath(delta, mon);
        }
        public static void ExecuteMovementPath(float delta, CombatMonster mon)
        {
            // Execute movement
            if (mon.MovePath != null && mon.MovePath.Count > 0)
            {
                Vector2 target = mon.MovePath[0];
                Vector2 direction = target - mon.currentPos;
                float distance = direction.Length();
                float step = mon.MovementQuickness * delta;
                if (distance <= step || distance < 0.01f)
                {
                    mon.currentPos = target;
                    mon.MovePath.RemoveAt(0);
                }
                else
                {
                    direction.Normalize();
                    mon.currentPos += direction * step;
                }
            }
            else if (mon.MovePath.Count <= 0 || _standInMonster.Speed == 0)
            {

                _standInMonster.Speed -= (int)_numberOfCellsMoved;
                mon.PathGenerated = false;
                _movementComplete = true;
                if (mon.isPlayerControled) 
                {
                    UpdatePlayerMoveableCells();
                }
                _playerTurnState = PlayerTurnState.PlayerWaitingInput;
                _numberOfCellsMoved = 0;

            }
        }
        private static void GenerateMovementPath(CombatMonster mon, List<TileCell> tileCellPath = null)
        {
            if (tileCellPath == null)
            {
                tileCellPath = GetMovementCellPath();
            }
            if (tileCellPath.Count == 0)
            {
                mon.MovePath.Clear();
                mon.PathGenerated = true;
                Add("Monster decides not to move");
                return;
            }

            Add($"mon.{mon.ID} Found Destination");
            _numberOfCellsMoved = tileCellPath.Count;
            List<Vector2> fullVectorPath = new();
            TileCell startingCell = null;
            if (mon.isPlayerControled)
            {
                startingCell = _playerControlledMonsterMap[mon];
            }
            if (!mon.isPlayerControled)
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
            mon.PathGenerated = true;
        }
        private static void PopulatePath(float delta, TileCell end)
        {
            CombatMonster mon = _turnOrder.Peek();

            if (!mon.PathGenerated)
            {
                if (end != null)
                {
                    List<TileCell> cellPath = GetPathToPlayerSelectedCell(_playerControlledMonsterMap[mon], end);
                    GenerateMovementPath(mon, cellPath);
                }
            }
          


        }
        public static List<TileCell> GetPathToPlayerSelectedCell(TileCell start, TileCell destination)
        {
            return GridMovement.FindPath(start, destination, int.MaxValue); // or -1 if your method supports it
        }
        private static List<TileCell> GetMovementCellPath()
        {
            if (_standInMonster.OrderOfActions.Peek() == "moveClose")
            {
                CombatMonster currentMon = _turnOrder.Peek();
                TileCell currentCell = _aIControlledMonsterMap[currentMon];

                List<TileCell> playerControlledCells = _playerControlledMonsterMap
                    .Select(pair => pair.Value)
                    .Where(cell => cell != null)
                    .ToList();

                // If no targets or already adjacent, return current position
                if (TileManager.IsNeighbor(playerControlledCells, currentCell))
                    return new List<TileCell>();

                List<TileCell> listOfCellsPathToTarget = GridMovement.FindClosestTargetPath(currentCell, playerControlledCells, _standInMonster.Speed) ;
                return listOfCellsPathToTarget;
            } 

            return null;
        }
        private static void DecideOrderOfOperations()
        {
            if (_standInMonster.isPlayerControled) { return;}

            if (_standInMonster.TurnBehavior == "getCloseAsPossible")
            {
                _standInMonster.OrderOfActions = new Queue<string>(new[] { "moveClose", "attack" });
            }

        }
        private static void UpdateDefeatedMonsters()
        {
            List<CombatMonster> stillAlive = new();
            foreach (var monster in _turnOrder)
            {
                if (monster.CurrentHealth <= 0)
                {
                    _defeatedMonsters.Add(monster);
                }
                else
                {
                    stillAlive.Add(monster);
                }
            }
            _turnOrder = new Queue<CombatMonster>(stillAlive);
        }
        private static void CopyCurrentMonToStandin()
        {
            _standInMonster.TurnBehavior = _turnOrder.Peek().TurnBehavior;
            _standInMonster.CurrentMana = _turnOrder.Peek().CurrentMana;
            _standInMonster.AttackPower = _turnOrder.Peek().AttackPower;
            _standInMonster.isPlayerControled = _turnOrder.Peek().isPlayerControled;
            _standInMonster.Speed = _turnOrder.Peek().Speed;
            _standInMonster.ID = _turnOrder.Peek().ID;
        }
        private static void SetTurnOrder()
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
                _turnOrder.Enqueue(entity);
            }

        }
        public static void UpdateMouseHoverCell()
        {
            Vector2 mousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);
            _currentMouseHoverCell = TileManager.GetCell(mousePos);
        }
        private static void FindSpawnablCellsForPlayerAndMons()
        {
            foreach (var cell in _currentMapTile.TileGrid)
            {
                if (cell.MonsterSpawnable)
                {
                    _monsterSpawnableCells.Add(cell);
                }
                if (cell.HeroSpawnable)
                {
                    _heroSpawnableCells.Add(cell);
                }
            }
        }
        private static void SetCombatMonsterStartingPos()
        {
            if (_monsterSpawnableCells.Count < _playMonsters.Monsters.Count) { Debug.WriteLine($"More Monstesr than cells to spawn in"); }
            Random ran = new Random();
            List<TileCell> spawnableCells = new List<TileCell>(_monsterSpawnableCells);
            List<CombatMonster> comMon = new List<CombatMonster>(_playMonsters.Monsters);
            do
            {
                foreach(var mon in comMon)
                {
                    int index = ran.Next(spawnableCells.Count);
                    Vector2 pos = (TileManager.GetCellCords(spawnableCells[index]));
                    mon.startingPos = pos;
                    mon.currentPos = pos;
                    spawnableCells.RemoveAt(index);
                }

            } while(spawnableCells.Count < spawnableCells.Count - _playMonsters.Monsters.Count);
        }
        public static void SetState(CombatState newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;
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
        public static void DisplayStats(SpriteBatch spriteBatch)
        {
            int iconSize = 48;
            int spacingX = 150; // Horizontal space between icons
            int topY = 20; // Vertical offset from the top of the screen
            SpriteFont font = AssetManager.GetFont("mainFont");

            int count = _turnOrder.Count;
            int totalWidth = count * spacingX;

            // Center the row
            int screenWidth = 1920; // Or use GraphicsDevice.Viewport.Width if you have access
            Vector2 startingPos = new Vector2((screenWidth - totalWidth) / 2f, topY);

            int index = 0;
            foreach (var mon in _turnOrder)
            {
                Vector2 iconPos = startingPos + new Vector2(index * spacingX, 0);
                Rectangle iconRect = new Rectangle((int)iconPos.X, (int)iconPos.Y, iconSize, iconSize);

                // Draw icon
                string iconKey = mon.isPlayerControled ? "Hero_Blonde" : mon.IconPath;
                Texture2D icon = AssetManager.GetTexture(iconKey);
                spriteBatch.Draw(icon, iconRect, Color.White);

                // Draw health below
                string hpText = $"{mon.CurrentHealth} / {mon.MaxHealth}";
                Vector2 textSize = font.MeasureString(hpText);
                Vector2 textPos = new Vector2(
                    iconRect.X + (iconSize - textSize.X) / 2,
                    iconRect.Bottom + 2
                );

                spriteBatch.DrawString(font, hpText, textPos, Color.Black);

                index++;
            }
        }
        public static CombatMonster GetPlayerMonster()
        {
            return _playerMonster;
        }
        public static CombatMonster GetStandInMonster()
        {
            return _standInMonster;
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