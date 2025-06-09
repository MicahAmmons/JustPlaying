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
using PlayingAround.Managers.Tiles;
using System.Net;

namespace PlayingAround.Managers.Entities
{
    public static class PlayMonsterManager
    {
        private static Dictionary<string, List<PlayMonsterData>> _playMonsterData;

        private static List<PlayMonsters> _currentPlayMonsters => TileManager.CurrentMapTile.PlayMonstersList;
        public static PlayMonsters SelectedMonster = null;
        public static Vector2? SelectedMonsterInfoAnchor = null;




        public static void LoadContent()
        {
            _playMonsterData = JsonLoader.LoadPlayMonsterData();
        }
        public static List<PlayMonsters> GeneratePlayMonsters(float diffMax, float diffMin, int maxSpawn, List<TileCell> cells, List<string> monsterString)
        {
            List<PlayMonsters> monsters = new List<PlayMonsters>();
            // Difficulty of the MapTile
            float difficultyMax = diffMax;
            float difficultyMin = diffMin;
            int totalSpawns = maxSpawn;

            // Step 1: Create a list of all available CombatMonsters based on the JSON data
            List<CombatMonster> monsterOptions = CombatMonsterManager.GetCombatMonsters(monsterString);

            Random random = new Random();
            for (int i = 0; i < maxSpawn; i++)
            {

                Vector2 startPos = DeterminePlayMonsterSpawn(cells);
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
                newPlayMon.Name = comMon.Name;
                monsters.Add(newPlayMon);

            }
            return monsters;
        }
        public static Vector2 DeterminePlayMonsterSpawn(List<TileCell> cells)
        {
            List<TileCell> tileCells = new List<TileCell>(cells);

            Random random = new Random();
            TileCell selectedCell = tileCells[random.Next(tileCells.Count)];



            return selectedCell.CenterPoint;
        }

        private static void HandleMonsterSelection()
        {
            if (InputManager.IsLeftClick())
            {
                Vector2 mousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);

                foreach (var mon in _currentPlayMonsters)
                {
                    var widthHeight = CombatMonsterManager.GetMonsterWidthAndHeight(mon.Name);
                    int width = (int)widthHeight.X;
                    int height = (int)widthHeight.Y;
                    var pos = TileManager.OffSetFromCenterOfDiamond(mon.CurrentPos, width, height);
                    Rectangle dest = new Rectangle(
                        (int)pos.X,
                        (int)pos.Y,
                        width,
                        height
                    );

                    if (dest.Contains(mousePos))
                    {
                        SelectedMonster = mon;
                        SelectedMonsterInfoAnchor = mousePos; // Capture click position
                        return;
                    }
                }

                SelectedMonster = null;
                SelectedMonsterInfoAnchor = null;
            }
        }


        public static void Update(GameTime gameTime)
        {
            HandleUserInput();

            MovePlayMonsters(gameTime);
        }
        private static void HandleUserInput()
        {
            HandleMonsterSelection();
        }
        public static void MovePlayMonsters(GameTime gameTime)
        {
            if (_currentPlayMonsters.Count > 0)
            {
                NPCMovement.GetPlayMonsterMovementPath(_currentPlayMonsters, gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            DrawPlayMonsters(spriteBatch);

        }
        public static void DrawPlayMonsters(SpriteBatch spriteBatch)
        {
            if (_currentPlayMonsters == null || _currentPlayMonsters.Count == 0) return;
            foreach (var mon in _currentPlayMonsters)
            {
                {
                    var widthHeight = CombatMonsterManager.GetMonsterWidthAndHeight(mon.Name);
                    int width = (int)widthHeight.X;
                    int height = (int)widthHeight.Y;
                    var pos = TileManager.OffSetFromCenterOfDiamond(mon.CurrentPos, width, height);
                    Rectangle dest = new Rectangle(
                        (int)pos.X,
                        (int)pos.Y,
                        width,
                        height
                    );

                    // Draw icon
                    spriteBatch.Draw(mon.Icon, dest, Color.White);
                }

            }




        }
    }
}
