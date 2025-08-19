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
        private static Dictionary<string, PlayMonsterData> _playMonsterData;

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
            List<TileCell> startingCellOptions = new List<TileCell>(cells);
            for (int i = 0; i < maxSpawn; i++)
            { 
                // Step 1: Create a list of all available CombatMonsters based on the JSON data
                List<CombatMonster> monsterOptions = CombatMonsterManager.GetCombatMonsters(monsterString);
                CombatMonster firstMon = monsterOptions[0];
                string firstMonName = firstMon.Name;
                var dataCopy = DeepCopyHelper.DeepCopy(_playMonsterData[firstMonName]);
                TileCell startingCell = PickStartingCell(startingCellOptions);
                startingCellOptions.Remove(startingCell);
                PlayMonsters newPlayMon = new PlayMonsters(_playMonsterData[firstMonName], monsterOptions[0])
                {
                    Monsters = monsterOptions,
                };
                newPlayMon.SetCurrentPauseDuration();
                newPlayMon.SetPlayMonsterStartingPos(startingCell.CenterPoint);
                
                monsters.Add(newPlayMon);

            }
            return monsters;
        }

        private static TileCell PickStartingCell(List<TileCell> startingCellOptions)
        {
            TileCell selectedCell = startingCellOptions[RandomHut.rng.Next(startingCellOptions.Count)];
            return selectedCell;
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
                    var pos = TileManager.OffSetFromCenterOfDiamond(mon.MovementController.CurrentPos, width, height);
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
            if (SceneManager.CurrentState == SceneState.Play || SceneManager.CurrentState == SceneState.Dialogue)
            {
                foreach (var mon in _currentPlayMonsters)
                {
                    mon.Update(gameTime);
                }
            }    
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var mon in _currentPlayMonsters)
            {
                mon?.Draw(spriteBatch);
            }
        }
        public static void RemovePlayMonster(PlayMonsters playMonsters)
        {
            _currentPlayMonsters.Remove(playMonsters);
        }
    }
}
