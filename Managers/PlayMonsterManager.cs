using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Assets;
using PlayingAround.Game.Map;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers
{
    public class PlayMonsterManager
    {
        private static List<PlayMonsters> CurrentPlayMonsters = new List<PlayMonsters>();
        private static PlayMonsters _selectedMonster = null;

        private const int IconWidth = 64;
        private const int IconHeight = 64;
        public static List<PlayMonsters> GeneratePlayMonsters(MapTileData data)
        {
            string path = "C:/Users/micah/OneDrive/Desktop/Repos/PlayingAround/Entities/Monster/PlayMonsters/PlayMonsterJson/PlayMonsters.json";
            Dictionary<string, List<PlayMonsterData>> jsonData = JsonLoader.LoadPlayMonsterData(path);

            // Difficulty of the MapTile
            float totalDifficulty = data.Difficulty;
            int totalSpawns = data.TotalMonsterSpawns;

            // Step 1: Create a list of all available CombatMonsters based on the JSON data
            

            Random random = new Random();
            for (int i = 0; i < data.TotalMonsterSpawns; i++)
            {
                List<CombatMonster> allCombatMonsters = new List<CombatMonster>();
                totalDifficulty = data.Difficulty;
                float moveSpeed = 0;
                Rectangle pacingBound = new Rectangle(00, 0, 0, 0);
                string movementPatter = null;
                string iconPath = null;
                float pauseDur = 0;

                while (totalDifficulty > 0)
                {

                    string randomMonsterName = data.MonsterStrings[random.Next(data.MonsterStrings.Count)];

                    if (jsonData.TryGetValue(randomMonsterName, out var monsterDataList) && monsterDataList.Count > 0)
                    {
                        var monsterData = monsterDataList[random.Next(monsterDataList.Count)];
                        CombatMonster combatMonster = new CombatMonster
                        {
                            IconPath = monsterData.IconPath,
                            Difficulty = monsterData.Difficulty,
                            Name = randomMonsterName
                        };
                        totalDifficulty -= monsterData.Difficulty;
                        allCombatMonsters.Add(combatMonster);
                        moveSpeed = monsterData.MovementSpeed;
                        pacingBound = monsterData.PacingBoundaryRect;
                        pauseDur = monsterData.PauseDuration;
                        movementPatter = monsterData.MovementPattern;
                        iconPath = monsterData.IconPath;

                    }

                }
                Vector2 startPos = DeterminePlayMonsterSpawn(data.Cells);
                PlayMonsters newPlayMon = new PlayMonsters()
                {
                    Monsters = allCombatMonsters,
                    MovementSpeed = moveSpeed,
                    PacingBoundary = pacingBound,
                    MovementPattern = movementPatter,
                    IconPath = iconPath,
                    SpawnPosition = startPos,
                    Icon = AssetManager.GetTexture(iconPath),
                    CurrentPos = startPos,
                    PauseDuration = pauseDur,
                };
                CurrentPlayMonsters.Add(newPlayMon);
            }
            return CurrentPlayMonsters;
        }
        public static Vector2 DeterminePlayMonsterSpawn(List<TileCellData> cells)
        {
            List<TileCellData> tileCells = new List<TileCellData>();
            foreach (var cell in cells)
            {
                if (cell.CanSpawn) { tileCells.Add(cell); }
            }
            Random random = new Random();
            TileCellData selectedCell = tileCells[random.Next(tileCells.Count)];

            int x = selectedCell.X * MapTile.TileWidth;
            int y = selectedCell.Y * MapTile.TileHeight;
            return new Vector2(x, y);
        }
        public static void AddPlayMonster(PlayMonsters mon)
        {
            CurrentPlayMonsters.Add(mon);
        }
        private static void MovePlayMonsters(GameTime gameTime)
        {
            foreach (var mon in CurrentPlayMonsters)
            {
                if (mon.MovementPattern == "arc" || mon.MovementPattern == "idle")
                {
                    if (Movement.NPCMovement.HandlePause(mon, gameTime))
                        continue;

                    Movement.NPCMovement.MoveTowardsNextPathPoint(mon, gameTime);
                }
            }
        }
        private static void HandleMonsterSelection()
        {
            if (InputManager.IsLeftClick())
            {
                Vector2 mousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);

                foreach (var mon in CurrentPlayMonsters)
                {
                    Rectangle dest = new Rectangle(
                        (int)(mon.CurrentPos.X - IconWidth / 2f),
                        (int)(mon.CurrentPos.Y - IconHeight),
                        IconWidth,
                        IconHeight
                    );

                    if (dest.Contains(mousePos))
                    {
                        _selectedMonster = mon;
                        return;
                    }
                }
                _selectedMonster = null;
            }
        }

        public static void Update(GameTime gameTime)
        {
            HandleMonsterSelection();
            MovePlayMonsters(gameTime);
        }
        public static void UpdateCurrentPlayMonsters(List<PlayMonsters> mons)
        {

            CurrentPlayMonsters = mons;
        }
        public static void ClearMonsters()
        {
            CurrentPlayMonsters.Clear();
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var monster in CurrentPlayMonsters)
            {
                if (monster.Icon != null)
                {
                    Rectangle dest = new Rectangle(
                        (int)(monster.CurrentPos.X - IconWidth / 2f),
                        (int)(monster.CurrentPos.Y - IconHeight),
                        IconWidth,
                        IconHeight
                    );

                    // Draw icon
                    spriteBatch.Draw(monster.Icon, dest, Color.White);
                }
            }
            if (_selectedMonster != null)
            {
                DrawCombatMonsterInfo(spriteBatch);
            }
        }
        private static void DrawCombatMonsterInfo(SpriteBatch spriteBatch)
        {
            Vector2 uiPos = _selectedMonster.CurrentPos;
            foreach (var combatMon in _selectedMonster.Monsters)
            {
                spriteBatch.DrawString(
                    AssetManager.GetFont("mainFont"), // Make sure you load a SpriteFont
                    combatMon.Name,
                    uiPos,
                    Color.Black
                );

                uiPos.Y += 20; // Move down for the next line
            }
        }
    }}
