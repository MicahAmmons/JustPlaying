using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder.GlowTex;
using PlayingAround.AnimationFolder.SmokeText;
using PlayingAround.Entities.Monster;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.World.MapTiles.CellHighlights;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace PlayingAround.Game.Map
{
    public class MapTile
    {
        public int x;
        public int y;
        public int z; 
        public string Id { get; }
        public List<Texture2D> BackgroundBuidldOrder { get; private set; } = new List<Texture2D>();
        public List<Rectangle> Obstacles { get; } = new();
        public List<TileCell> AllValidCells { get; private set; } = new();
        public Dictionary<(int, int), bool> WalkableMap { get; private set; } = new();
        public Dictionary<Vector2, NextTileData> NextTileMap { get; private set; } = new();
        public List<string> OptionsOfMonsters { get; private set; } = new();
        public List<TileCell> CellsWithStaticBackGround {  get; private set; } = new(); 
        public List<TileCell> MonsterSpawnableCells { get; private set; } = new List<TileCell> { };
        public List<TileCell> PlayerSpawnableCells { get; private set; } = new List<TileCell> { };
        public List<TileCell> PlayMonsterSpawnableCells { get; private set; } = new List<TileCell> { };
        public List<GlowTexture> BehindGlowTextures { get; private set; } = new List<GlowTexture> { };
        public SmokeTexture BackgroundSmokeTexture { get; private set; } 
        public TileCellHighlights CellHighlights { get; private set; }
        public List<NPC> NPCs { get; private set; } = new();
        public Dictionary<TileCell, NPC> NPCCells { get; private set; } = new Dictionary< TileCell, NPC> { };
        public float DifficultyMax { get; }
        public float DifficultyMin { get; }
        public int TotalMonsterSpawns { get; }
        public List<PlayMonsters> PlayMonstersList { get; } = new List<PlayMonsters>();



        public const int GridWidth = 31;   // example number of cells per screen
        public const int GridHeight = 35;
        public const int TileWidth = 64;
        public const int TileHeight = 32;


        public MapTile(MapTileData data)
        {
            Id = $"{data.GridX}_{data.GridY}_{data.GridZ}";
            x = data.GridX ; y = data.GridY ; z= data.GridZ ;
            foreach (var str in data.BackgroundBuildOrder)
            {
                BackgroundBuidldOrder.Add(AssetManager.GetTexture(str));    
            }
            BackgroundSmokeTexture = data.BackgroundSmokeTexture;
            DifficultyMax = data.DifficultyMax;
            DifficultyMin = data.DifficultyMin;
            TotalMonsterSpawns = data.TotalMonsterSpawns;
            CellHighlights = new TileCellHighlights(data.TileHighlightData);
  
            if (data.GlowTexture != null)
            {
                foreach (var glowText in data.GlowTexture)
                {
                    BehindGlowTextures.Add(new GlowTexture(glowText));
                }
            }
            MonsterSpawnableCells = new List<TileCell>();
            PlayerSpawnableCells = new List<TileCell>();
            OptionsOfMonsters = data.MonsterStrings;
            AllValidCells = new List<TileCell>();

            foreach (var cellData in data.Cells)
            { 
                if (cellData.X < 0 || cellData.X >= GridWidth) continue;
                if (cellData.Y < 0 || cellData.Y >= GridHeight) continue;

                var tile = new TileCell(cellData);
              
                if (!IsDiamondAligned(tile.X, tile.Y)) continue;
                    if (tile.NextTile != null)
                    {
                        NextTileMap[tile.CenterPoint] = tile.NextTile;
                    }
                    AllValidCells.Add(tile);
                    if (tile.IsWalkable) WalkableMap[(tile.X, tile.Y)] = true;
                    if (cellData.MonsterSpawnable)
                        MonsterSpawnableCells.Add(tile);
                    if (cellData.PlayerSpawnable)
                        PlayerSpawnableCells.Add(tile);
                    if (cellData.PlayMonsterSpawnable)
                       PlayMonsterSpawnableCells.Add(tile);
                if (cellData.StaticBackGroundTexture != null)
                {
                    tile.BackGroundTexture = AssetManager.GetTexture(cellData.StaticBackGroundTexture);
                    CellsWithStaticBackGround.Add(tile);
                }
                if (cellData.NPCName != null)
                {
                    NPC npc = NPCManager.GenerateNPC(tile.NPCName, tile);
                    NPCCells[tile] = npc;
                    NPCs.Add(npc);
                }
                if (cellData.Trigger != null)
                {

                }

            }

            PlayMonstersList = PlayMonsterManager.GeneratePlayMonsters(DifficultyMax, DifficultyMin, TotalMonsterSpawns, PlayMonsterSpawnableCells, OptionsOfMonsters );
        }


        public static bool IsDiamondAligned(int x, int y) => (x % 2) == (y % 2);


    }
}
