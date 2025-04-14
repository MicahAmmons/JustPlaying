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
        private List<PlayMonsters> CurrentPlayMonsters = new List<PlayMonsters>();
        private PlayMonsters _selectedMonster = null;

        private const int IconWidth = 64;
        private const int IconHeight = 64;
        private Vector2? _selectedMonsterInfoAnchor = null;
        private Rectangle _fightButtonRect;
        private bool _isHoveringFightButton;




        public void GeneratePlayMonsters(MapTileData data)
        {
           // string path = "C:/Users/micah/OneDrive/Desktop/Repos/PlayingAround/Entities/Monster/PlayMonsters/PlayMonsterJson/PlayMonsters.json";
            string path = "C:/Users/micah/OneDrive/Desktop/Repos/JustPlaying/JustPlaying/Entities/Monster/PlayMonsters/PlayMonsterJson/PlayMonsters.json";
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
        }
        public Vector2 DeterminePlayMonsterSpawn(List<TileCellData> cells)
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
        public void AddPlayMonster(PlayMonsters mon)
        {
            CurrentPlayMonsters.Add(mon);
        }
        private void MovePlayMonsters(GameTime gameTime)
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
        private void HandleMonsterSelection()
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
                        _selectedMonsterInfoAnchor = mousePos; // Capture click position
                        return;
                    }
                }

                _selectedMonster = null;
                _selectedMonsterInfoAnchor = null;
            }
        }


        public void Update(GameTime gameTime)
        {
            HandleMonsterSelection();
            MovePlayMonsters(gameTime);

        }

        public void ClearMonsters()
        {
            CurrentPlayMonsters.Clear();
        }
        public void Draw(SpriteBatch spriteBatch)
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
        private void DrawCombatMonsterInfo(SpriteBatch spriteBatch)
        {
            if (_selectedMonster == null || _selectedMonsterInfoAnchor == null)
                return;

            var grouped = _selectedMonster.Monsters
                .GroupBy(mon => mon.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() });

            // Compute size of background
            var font = AssetManager.GetFont("mainFont");
            int lineHeight = 20;
            int boxWidth = 160;
            int boxHeight = grouped.Count() * lineHeight + 10;

            Vector2 anchor = _selectedMonsterInfoAnchor.Value;
            Rectangle backgroundBox = new Rectangle((int)anchor.X, (int)anchor.Y, boxWidth, boxHeight);

            // Draw text over it
            Vector2 textPos = anchor + new Vector2(5, 5);
            foreach (var group in grouped)
            {
                string displayName = group.Count > 1 ? $"({group.Count}) {Pluralize(group.Name)}" : group.Name;

                spriteBatch.DrawString(font, displayName, textPos, Color.Green);
                textPos.Y += lineHeight;
            }
        }

        private string Pluralize(string name)
        {
            // Very basic pluralization. You can enhance this later.
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !name.EndsWith("ey"))
                return name.Substring(0, name.Length - 1) + "ies";
            else if (name.EndsWith("s"))
                return name;
            else
                return name + "s";
        }


    }
}
