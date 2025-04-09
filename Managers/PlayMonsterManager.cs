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
        private const int IconWidth = 128;
        private const int IconHeight = 128;
        public static void GeneratePlayMonsters(MapTileData data)
        {
            string path = "C:/Users/micah/OneDrive/Desktop/Repos/PlayingAround/Entities/Monster/PlayMonsters/PlayMonsterJson/PlayMonsters.json";
            Dictionary<string, List<PlayMonsterData>> jsonData = JsonLoader.LoadPlayMonsterData(path);



            // Difficulty of the MapTile
            float totalDifficulty = data.Difficulty;
            int totalSpawns = data.TotalMonsterSpawns;

            // Step 1: Create a list of all available CombatMonsters based on the JSON data
            List<CombatMonster> allCombatMonsters = new List<CombatMonster>();

            Random random = new Random();
            for (int i = 0; i < data.TotalMonsterSpawns; i++)
            {

                totalDifficulty = data.Difficulty;
                float moveSpeed = 0;
                Rectangle pacingBound = new Rectangle(0, 0, 0, 0);
                string movementPatter = null;
                string iconPath = null;

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
                        pacingBound = monsterData.PacingBoundary;
                        movementPatter = monsterData.MovementPattern;
                        iconPath = monsterData.IconPath;

                    }

                }
                PlayMonsters newPlayMon = new PlayMonsters()
                {
                    Monsters = allCombatMonsters,
                    MovementSpeed = moveSpeed,
                    PacingBoundary = pacingBound,
                    MovementPattern = movementPatter,
                    IconPath = iconPath,
                    SpawnPosition = DeterminePlayMonsterSpawn(data.Cells),
                    Icon = AssetManager.GetTexture(iconPath)
                };
                CurrentPlayMonsters.Add(newPlayMon);
            }
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
        public static void MovePlayMonsters(GameTime gameTime)
        {
            foreach (var monster in CurrentPlayMonsters)
            {
                if (monster.MovementPattern == "arc")
                {
                    // Increment time since last jump
                    monster.TimeSinceLastJump += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Check if it's time to jump (after the cooldown)
                    if (monster.TimeSinceLastJump >= monster.JumpCooldown)
                    {
                        // Select a random landing point within the PacingBoundary
                        Random random = new Random();

                        // Ensure that the new landing point is at least 1/5th of the boundary width away from the origin
                        float minX = monster.PacingBoundary.X + monster.PacingBoundary.Width / 5f;
                        float maxX = monster.PacingBoundary.X + monster.PacingBoundary.Width * 0.8f;

                        float minY = monster.PacingBoundary.Y + monster.PacingBoundary.Height / 5f;
                        float maxY = monster.PacingBoundary.Y + monster.PacingBoundary.Height * 0.8f;

                        // Randomly select a landing point within these bounds
                        monster.LandingPoint = new Vector2(
                            random.NextFloat(minX, maxX),
                            random.NextFloat(minY, maxY)
                        );

                        // Store the start position of the jump
                        monster.JumpStartPosition = monster.SpawnPosition;

                        // Reset the jump timer
                        monster.TimeSinceLastJump = 0f;
                    }

                    // Calculate movement towards the landing point
                    Vector2 direction = monster.LandingPoint - monster.SpawnPosition;

                    // Normalize the direction vector to get the unit vector
                    float distance = direction.Length();
                    direction.Normalize();

                    // Define the arc's curve: Use a sine function for vertical movement
                    float arcHeight = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * monster.MovementSpeed) * 50f;

                    // Calculate the new position based on the direction and arc
                    float progress = (float)(gameTime.TotalGameTime.TotalSeconds % 1); // Loop the movement after 1 second
                    float horizontalMovement = progress * distance; // Smooth horizontal movement

                    // Interpolating between the start and end positions
                    Vector2 currentPosition = Vector2.Lerp(monster.JumpStartPosition, monster.LandingPoint, progress);

                    // Apply the arc height as vertical movement
                    currentPosition.Y += arcHeight;

                    // Set the new position, ensuring it stays within the pacing boundary
                    monster.SpawnPosition = new Vector2(
                        MathHelper.Clamp(currentPosition.X, monster.PacingBoundary.X, monster.PacingBoundary.X + monster.PacingBoundary.Width),
                        MathHelper.Clamp(currentPosition.Y, monster.PacingBoundary.Y, monster.PacingBoundary.Y + monster.PacingBoundary.Height)
                    );
                }
                //else if (monster.MovementPattern == "idle")
                //{
                //    // Idle movement: slight left-right or staying in place
                //    float idleMovement = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5f) * 10f; // Small horizontal movement
                //    monster.SpawnPosition = new Vector2(monster.SpawnPosition.X + idleMovement, monster.SpawnPosition.Y);
                //}
            }
        }
    


        public static void Update(GameTime gameTime)
        {
           MovePlayMonsters(gameTime);
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var monster in CurrentPlayMonsters)
            {
                if (monster.Icon != null)
                {
                    Vector2 origin = new Vector2(IconWidth / 2f, IconHeight / 2f);

                    // Now draw the texture with the new size
                    spriteBatch.Draw(monster.Icon, monster.SpawnPosition, null, Color.White, 0f, origin, new Vector2(IconWidth / (float)monster.Icon.Width, IconHeight / (float)monster.Icon.Height), SpriteEffects.None, 0f);


                }
            }
        }
    }

}
