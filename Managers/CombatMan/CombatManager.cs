using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Movement.CombatGrid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static PlayingAround.Managers.SceneManager;

namespace PlayingAround.Managers.CombatMan
{
    public static class CombatManager
    {
        private static PlayMonsters _playMonsters;
        private static Player _player;
        private static CombatMonster _playerMonster;
        private static MapTile _currentMapTile;
        private static Texture2D _playerCellOptions;
        private static TileCell _currentClickedCell;
        private static TileCell _currentMouseHoverCell;
        private static List<TileCell> _heroSpawnableCells = new List<TileCell>();
        private static List<TileCell> _monsterSpawnableCells = new List<TileCell>();
        private static List<CombatMonster> _summonedMonsters = new List<CombatMonster>();
        private static Queue<CombatMonster> _turnOrder = new Queue<CombatMonster>();
        private static Queue<string> orderOfActions = new Queue<string>();
        private static int _startingSpeed;




        public enum CombatState
        {
            Waiting,
            LocationSelection,
            RoundStart, // add new class here?
            AwaitingInput,
            MovingPlayerControlled,
            MovingAIControlled,
            AIAttacking,

            ExecutingAction,
            ResolvingEffects,
            EndingTurn,
            CombatOver

        }

        private static CombatState _currentState = CombatState.Waiting;
        public static CombatState CurrentState => _currentState;

        public static void Initialize()
        {
            _currentMapTile = TileManager.CurrentMapTile;
            _playerCellOptions = AssetManager.GetTexture("fightBackground");
        }

        public static void BeginCombat(PlayMonsters playMonsters, Player player)
        {
            SceneManager.SetState(SceneManager.SceneState.Combat);
            _playMonsters = playMonsters;
            _player = player;
            _playerMonster = new CombatMonster(player);
            SetTurnOrder();
            FindSpawnablCellsForPlayerAndMons();
            SetCombatMonsterStartingPos();
            CombatManager.SetState(CombatState.LocationSelection);
        }
        private static void SetTurnOrder()
        {
            List<CombatMonster> allCombatants = new List<CombatMonster>();
            allCombatants.AddRange(_playMonsters.Monsters);
            allCombatants.Add(_playerMonster);
            allCombatants.AddRange(_summonedMonsters);
            allCombatants.Sort((a, b) => b.Speed.CompareTo(a.Speed));
            foreach (var entity in allCombatants)
            {
                _turnOrder.Enqueue(entity);
            }

        }
        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_currentState == CombatState.LocationSelection)
            {
                foreach (var tile in _heroSpawnableCells)
                {
                    Vector2 cords = TileManager.GetCellCords(tile);
                    Rectangle destination = new Rectangle(
                    (int)(cords.X),
                    (int)cords.Y,
                    32,
                    32
                     );
                    spriteBatch.Draw(_playerCellOptions, destination, Color.White);
                }
                foreach (var cell in _monsterSpawnableCells)
                {
                    Vector2 monCords = TileManager.GetCellCords(cell);
                    Rectangle destination = new Rectangle(
                    (int)(monCords.X),
                    (int)monCords.Y,
                    32,
                    32
                     );
                    spriteBatch.Draw(_playerCellOptions, destination, Color.Black);
                }
                if (_currentMouseHoverCell.HeroSpawnable)
                {
                    Vector2 cellCord = TileManager.GetCellCords(_currentMouseHoverCell);
                    Rectangle destination = new Rectangle(
                    (int)(cellCord.X),
                    (int)cellCord.Y,
                    32,
                    32
                     );
                    spriteBatch.Draw(_player.Texture, destination, Color.White);
                }
            }


            foreach (var combatMon in _turnOrder)
            {
                if (!combatMon.isPlayerControled)
                {
                    Rectangle destination = new Rectangle(
                        (int)(combatMon.startingPos.X),
                        (int)(combatMon.startingPos.Y),
                        32,
                        32);
                    spriteBatch.Draw(AssetManager.GetTexture($"{combatMon.IconPath}"), destination, Color.White);
                }
                if (_playerMonster.Draw)
                {
                    Rectangle destination = new Rectangle(
                        (int)(_playerMonster.currentPos.X),
                        (int)(_playerMonster.currentPos.Y),
                        32,
                        32);
                    spriteBatch.Draw(AssetManager.GetTexture("Hero_Blonde"), destination, Color.White);
                }
            }
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
        public static void Update(GameTime gameTime)
        {
            UpdateMouseClickedCell();
            UpdateMouseHoverCell();
            if (_currentState == CombatState.LocationSelection)
            {
                if (_heroSpawnableCells.Contains(_currentClickedCell))
                {
                    _playerMonster.currentPos = TileManager.GetCellCords(_currentClickedCell);
                    _playerMonster.Draw = true;
                    CombatManager.SetState(CombatState.RoundStart);
                }
            }
            if (_currentState == CombatState.RoundStart)
            {
                var currentCombatant = _turnOrder.Peek();

                if (currentCombatant.isPlayerControled)
                {

                }
                if (!currentCombatant.isPlayerControled)
                {
                    _startingSpeed = _turnOrder.Peek().Speed;
                    DecideOrderOfOperations(currentCombatant);
                    if (orderOfActions.Peek() == "moveClose")
                    {
                        _currentState = CombatState.MovingAIControlled;
                    }
                    if (orderOfActions.Peek() == "attack")
                    {
                        _currentState = CombatState.AIAttacking;
                    }
                    if (orderOfActions.Count == 0)
                    {
                        _currentState = CombatState.Waiting;
                    }
                }
            }
            if (_currentState == CombatState.MovingAIControlled)
            {
                if (_turnOrder.Peek().Speed > 0)
                {
                    Vector2 dest = GetDestination();
                    List<Vector2> path = GridMovement.GetPath(_turnOrder.Peek().currentPos, dest, _turnOrder.Peek().Speed);
                    ExecutePath(_turnOrder.Peek(), path);
                }
            }

        }

        private static void ExecutePath(CombatMonster mon, List<Vector2> path)
        {
            if (path == null || path.Count == 0)
                return;

            mon.currentPos = path.Last();
            mon.Speed -= path.Count - 1;

            orderOfActions.Dequeue(); // Finished this action
            SetState(CombatState.RoundStart); // Continue to next action or state
        }

        private static Vector2 GetDestination() 
        {
            if (orderOfActions.Peek() == "moveCloser")
            {
                List<Vector2> playerControlledPositions = _turnOrder
                 .Where(mon => mon.isPlayerControled)
                 .Select(mon => mon.currentPos)
                 .ToList();
                if (playerControlledPositions.Count == 0)
                    return _turnOrder.Peek().currentPos;

                return GridMovement.GetMonsterMovePosition(_turnOrder.Peek().currentPos, playerControlledPositions, _turnOrder.Peek().Speed);
            }
            return new Vector2(0, 0);
        }
        private static void DecideOrderOfOperations(CombatMonster mon)
        {

            if (mon.TurnBehavior == "getCloseAsPossible")
            {
                orderOfActions = new Queue<string>(new[] { "moveClose", "attack" });
            }

        }
        public static void UpdateMouseClickedCell()
        {
            if (InputManager.IsLeftClick())
            {
                Vector2 mousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);
                _currentClickedCell = TileManager.GetCell(mousePos);
            }
        }
        public static void UpdateMouseHoverCell()
        {
            Vector2 mousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);
            _currentMouseHoverCell = TileManager.GetCell(mousePos);
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

        



    }
}
