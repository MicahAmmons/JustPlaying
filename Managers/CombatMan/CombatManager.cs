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
using PlayingAround.Managers.Movement;
using PlayingAround.Managers.Movement.CombatGrid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static TileCell _currentClickedCell;
        private static TileCell _currentMouseHoverCell;
        private static List<TileCell> _heroSpawnableCells = new List<TileCell>();
        private static List<TileCell> _monsterSpawnableCells = new List<TileCell>();
        private static List<CombatMonster> _summonedMonsters = new List<CombatMonster>();
        private static Queue<CombatMonster> _turnOrder = new Queue<CombatMonster>();
        private static Queue<string> orderOfActions = new Queue<string>();
        private static int _startingSpeed;
        private static int? _numberOfCellsMoved;
        private static CombatMonster _currentTarget;
        private static bool _isProcessingAction = false;


        private static List<string> _log = new List<string>();
        private static int _maxStrings = 15;






        public enum CombatState
        {
            Waiting,
            LocationSelection,
            RoundStart, // Once at the top of each turn 
            ActionNavigation,
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
            Add("BeginCombat");
            SceneManager.SetState(SceneManager.SceneState.Combat);
            _playMonsters = playMonsters;
            _player = player;
            _playerMonster = new CombatMonster(player);
            SetTurnOrder();
            FindSpawnablCellsForPlayerAndMons();
            SetCombatMonsterStartingPos();
            CombatManager.SetState(CombatState.LocationSelection);
            Add("State = LocationSelection");
        }
        private static void SetTurnOrder()
        {
            List<CombatMonster> allCombatants = new List<CombatMonster>();
            allCombatants.AddRange(_playMonsters.Monsters);
            allCombatants.Add(_playerMonster);
            allCombatants.AddRange(_summonedMonsters);
            allCombatants.Sort((a, b) => b.Speed.CompareTo(a.Speed));
            int idCounter = 0;
            foreach (var entity in allCombatants)
            {
                entity.ID = idCounter++;
                _turnOrder.Enqueue(entity);
            }

        }
        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            OnScreenDebug(spriteBatch);
    


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
                        (int)(combatMon.currentPos.X),
                        (int)(combatMon.currentPos.Y),
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
            UpdateEntityCells();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateMouseClickedCell();
            UpdateMouseHoverCell();
            if (_currentState == CombatState.LocationSelection)
            {
                if (_heroSpawnableCells.Contains(_currentClickedCell))
                {
                    _playerMonster.currentPos = TileManager.GetCellCords(_currentClickedCell);
                    _playerMonster.Draw = true;
                    CombatManager.SetState(CombatState.RoundStart);
                    Add("State = RoundStart");
                }
            }
            if (_currentState == CombatState.RoundStart) // Have to remove the previous turn before calling RoundStart, it'll reset everything
            {
                DecideOrderOfOperations(_turnOrder.Peek()); Add("Order Of Ops Set");
                Add($"mon.{_turnOrder.Peek().ID}_startingSpeed Set");
                _startingSpeed = _turnOrder.Peek().Speed;
                _numberOfCellsMoved = null;

                if (_turnOrder.Peek().isPlayerControled)
                {

                }
                if (!_turnOrder.Peek().isPlayerControled)
                {
                    Add("State = ActionNAvigation");
                    CombatManager.SetState(CombatState.ActionNavigation);
                } 
            }
            if (_currentState == CombatState.ActionNavigation && !_isProcessingAction) 
            {
                _isProcessingAction = true;
                if (orderOfActions == null || orderOfActions.Count == 0)
                { 
                    CombatMonster mon = _turnOrder.Dequeue();
                    _turnOrder.Enqueue(mon);
                    _currentState = CombatState.RoundStart;
                    _isProcessingAction = false;
                }
                else if (orderOfActions.Peek() == "moveClose")
                {
                    Add($"mon.{_turnOrder.Peek().ID} moveCloser Begin. State = MovingAIControlled");
                    _currentState = CombatState.MovingAIControlled;
                }
                else if (orderOfActions.Peek() == "attack")
                {
                    Add($"mon.{_turnOrder.Peek().ID} Attack Begin. State = AIAttacking");
                    _currentState = CombatState.AIAttacking;
                }
                else if (orderOfActions.Count == 0)
                {
                    _currentState = CombatState.Waiting;
                }
                
            }
            
            if (_currentState == CombatState.MovingAIControlled)
            {
 
                MoveAIMonster(delta);
                _isProcessingAction = false;

               // orderOfActions.Dequeue();


            }
            if (_currentState == CombatState.AIAttacking)
            {
                Add("Dequeing Attack");
                orderOfActions.Dequeue();
                _currentState = CombatState.ActionNavigation;
                _isProcessingAction = false;

            }

        }

        private static void MoveAIMonster(float delta)
        {
            CombatMonster mon = _turnOrder.Peek();

            // Only generate path once
            if (!mon.PathGenerated && mon.Speed > 0)
            {
                Add($"mon.{_turnOrder.Peek().ID} Movement Begin");
                TileCell dest = GetEndPoint();
                Add($"mon.{_turnOrder.Peek().ID} Found Destination");
                List<TileCell> path = GridMovement.FindPath(mon.currentPos, dest, mon.Speed);
                Add($"mon.{_turnOrder.Peek().ID} Find Path");
                _numberOfCellsMoved = path.Count();
                List<Vector2> fullVectorPath = new();
                Vector2 current = mon.currentPos;

                foreach (var endPos in path)
                {
                    List<Vector2> arc = NPCMovement.ArcMovement(TileManager.GetCellCords(endPos), current);
                    fullVectorPath.AddRange(arc);
                    current = arc.Last();
                }

                mon.MovePath = fullVectorPath;
                mon.PathGenerated = true; // ✅ prevent regen
            }

            // Execute movement
            if (mon.MovePath != null && mon.MovePath.Count > 0)
            {
                Vector2 target = mon.MovePath[0];
                Vector2 direction = target - mon.currentPos;
                float distance = direction.Length();
                float step = mon.MovementQuickness * delta;

                if (distance <= step)
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
            else if (mon.PathGenerated) // ✅ only triggers after finishing path
            {
                Add($"mon.{_turnOrder.Peek().ID} finished moving");
                _startingSpeed = (int)(_startingSpeed - _numberOfCellsMoved);
                Add($"mon.{_turnOrder.Peek().ID} movement reduced to {_startingSpeed} from {_turnOrder.Peek().Speed}");
                mon.PathGenerated = false;
                orderOfActions.Dequeue();
                Add("AIMOvement Dqueued");
                SetState(CombatState.ActionNavigation);
                Add("State set to ActionNavigation");
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

        private static TileCell GetEndPoint()
        {
            if (orderOfActions.Peek() == "moveClose")
            {
                List<TileCell> playerControlledCells = _turnOrder
                    .Where(mon => mon.isPlayerControled)
                    .Select(mon => TileManager.GetCell(mon.currentPos))
                    .Where(cell => cell != null)
                    .ToList();

                if (playerControlledCells.Count == 0)
                    return TileManager.GetCell(_turnOrder.Peek().currentPos); // fallback to current cell

                TileCell currentCell = TileManager.GetCell(_turnOrder.Peek().currentPos);

                return GridMovement.GetMonsterMovePosition(currentCell, playerControlledCells);
            }

            return null; // fallback case
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

        public static void Add(string message)
        {
            _log.Add(message);

            // Keep it from growing forever
            if (_log.Count > _maxStrings)
                 _log.RemoveAt(0);
        }

        private static void OnScreenDebug(SpriteBatch spriteBatch)
        {
            SpriteFont font = AssetManager.GetFont("mainFont");
            Vector2 startPos = new Vector2(10, 10);
            int lineHeight = 18;

            // Calculate width & height of background box
            int maxWidth = _log.Any() ? _log.Max(line => (int)font.MeasureString(line).X) : 0;
            int boxHeight = lineHeight * _log.Count + 10;
            Rectangle backgroundRect = new Rectangle((int)startPos.X - 5, (int)startPos.Y - 5, maxWidth + 10, boxHeight);

            // ✅ Draw black background
            spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), backgroundRect, Color.Black);

            for (int i = 0; i < _log.Count; i++)
            {
                Vector2 pos = startPos + new Vector2(0, i * lineHeight);
                string text = _log[i];

                // ✅ Draw "stroke" outline effect by drawing text offset in all directions
                spriteBatch.DrawString(font, text, pos + new Vector2(-1, -1), Color.Black);
                spriteBatch.DrawString(font, text, pos + new Vector2(1, -1), Color.Black);
                spriteBatch.DrawString(font, text, pos + new Vector2(-1, 1), Color.Black);
                spriteBatch.DrawString(font, text, pos + new Vector2(1, 1), Color.Black);

                // ✅ Then draw main white text
                spriteBatch.DrawString(font, text, pos, Color.White);
            }
        }

        private static void UpdateEntityCells()
        {
            foreach (var mon in _turnOrder)
            {
                TileCell cell =  TileManager.GetCell(mon.currentPos);
                TileManager.AddCombatMonsterToCell(mon, cell);
            }
        }





    }
}
