using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Managers.CombatMan.CombatAttacks;
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
using System.IO;
using PlayingAround.Managers.Movement;

namespace PlayingAround.Managers
{
    public class PlayMonsterManager
    {
        public  List<PlayMonsters> CurrentPlayMonsters => TileManager.CurrentMapTile.PlayMonstersList;
        public PlayMonsters _selectedMonster = null;

        private const int IconWidth = 64;
        private const int IconHeight = 64;
        private Vector2? _selectedMonsterInfoAnchor = null;
        private static Dictionary<string, List<PlayMonsterData>> _playMonsterData;



        public static void LoadContent()
        {
            _playMonsterData = JsonLoader.LoadPlayMonsterData();
        }
        public List<PlayMonsters> GeneratePlayMonsters(MapTileData data)
        {
            List<PlayMonsters> monsters = new List<PlayMonsters>();
            // Difficulty of the MapTile
            float difficultyMax = data.DifficultyMax;
            float difficultyMin = data.DifficultyMin;   
            int totalSpawns = data.TotalMonsterSpawns;

            // Step 1: Create a list of all available CombatMonsters based on the JSON data
            List<CombatMonster> monsterOptions = CombatMonsterManager.GetCombatMonsters(data.MonsterStrings);

            Random random = new Random();
            for (int i = 0; i < data.TotalMonsterSpawns; i++)
            {

                Vector2 startPos = DeterminePlayMonsterSpawn(data.Cells);
                PlayMonsters newPlayMon = new PlayMonsters()
                {
                    Monsters = CombatMonsterManager.BalanceCombatMonsters(monsterOptions, difficultyMax, difficultyMin),
                    SpawnPosition = startPos,
                    CurrentPos = startPos,

                };
                CombatMonster comMon = newPlayMon.Monsters[0];
                string name = comMon.Name;
                newPlayMon.Icon = AssetManager.GetTexture(comMon.IconTextureKey);
                newPlayMon.MovementPattern = comMon.MovementPattern;
                newPlayMon.MovementSpeed = _playMonsterData[name][0].MovementSpeed;
                newPlayMon.PacingBoundary = _playMonsterData[name][0].PacingBoundaryRect;
                monsters.Add(newPlayMon);

            }
            return monsters;
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

            UpdateTileCells();
        }

        public void MovePlayMonsters(GameTime gameTime)
        {
            if (CurrentPlayMonsters.Count > 0)
            {
                NPCMovement.GetPlayMonsterMovementPath(CurrentPlayMonsters, gameTime);
            }
        }
        public void UpdateTileCells()
        {
            foreach (var mon in CurrentPlayMonsters)
            {
                mon.CurrentCell = TileManager.GetCell(mon.CurrentPos);
            }
        }

        public void ClearMonsters()
        {
            CurrentPlayMonsters.Clear();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                DrawPlayMonsters(spriteBatch);
              
                if (_selectedMonster != null)
                {
                    DrawCombatMonsterInfo(spriteBatch);
                }
            }
        }
        public void DrawPlayMonsters(SpriteBatch spriteBatch)
        {
            if (CurrentPlayMonsters == null || CurrentPlayMonsters.Count == 0) return;
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
        }
        private void DrawCombatMonsterInfo(SpriteBatch spriteBatch)
        {
            if (_selectedMonster == null || _selectedMonsterInfoAnchor == null)
                return;

            var grouped = _selectedMonster.Monsters
    .GroupBy(mon => mon.NamePlusLevel)
    .Select(g => new { NamePlusLevel = g.Key, Count = g.Count() });



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
                string displayName = group.Count > 1
                    ? $"({group.Count}) {Pluralize(group.NamePlusLevel)}"
                    : group.NamePlusLevel;

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
