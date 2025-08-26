using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using PlayingAround.Managers.Assets;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;

namespace PlayingAround.Game.Assets
{
    public static class AssetLoader
    {
        public static void LoadAllFonts()
        {
            AssetManager.LoadFont("mainFont", "mainFont");
            AssetManager.LoadFont("titleScreenButtonFont", "TitleScreenButtonFont");
        }
        public static void LoadAllTextures()
        {
            AssetManager.LoadTexture("Hero_Idle", "HeroArt/BlonderHero");

            AssetManager.LoadTexture("Arrow", "TileCell/arrow");
            AssetManager.LoadTexture("3Arrows", "Tilecell/3arrows");
            AssetManager.LoadTexture("MonsterIcons/AngryPlantIcon", "MonsterIcons/AngryPlantIcon");

            AssetManager.LoadTexture("fightBackground", "UI/fightBackground");

            AssetManager.LoadTexture("TitleScreenBackGround", "TitleScreen/TitleScreenBackGround");
           
            AssetManager.LoadTexture("DefaultNPC", "NPC/DefaultNPC");



            // Add fonts, sounds, etc. later
        }
        public static void LoadAllSpriteSheets()
        {
            AssetManager.LoadTexture("FrostOozeIdle", "MonsterAnimations/FrostOoze/FrostOozeIdle");
           AssetManager.LoadTexture("FrostOozeSlamDown", "MonsterAnimations/FrostOoze/FrostOozeSlamDown");
            AssetManager.LoadTexture("FrostOozeSlamUp", "MonsterAnimations/FrostOoze/FrostOozeSlamUp");
            AssetManager.LoadTexture("FrostOozeWalk", "MonsterAnimations/FrostOoze/FrostOozeWalk");
        }
        public static void LoadPlayerSpriteSheets()
        {
            AssetManager.LoadTexture("PlayerHead", "PlayerSprites/PlayerHead");
            AssetManager.LoadTexture("PlayerBody", "PlayerSprites/PlayerBody");
            AssetManager.LoadTexture("PlayerBodyColored", "PlayerSprites/PlayerBodyColored");
        }
        public static void LoadAttackIconTextures()
        {
            AssetManager.LoadTexture("Spit", "Attacks/Spit");
        }
        public static void LoadMonsterIconTextures()
        {
            AssetManager.LoadTexture("OozeIcon", "MonsterIcons/OozeIcon");
        }
        public static void LoadPlayerIconTextures()
        {
            AssetManager.LoadTexture("Hero_Blonde", "HeroArt/BlonderHero");
        }
        public static void LoadElementIconTextures()
        {
            AssetManager.LoadTexture("AcidIcon", "Aspects/AcidIcon");
        }

        public static Dictionary<string, Song> LoadAllSongs()
        {
            Dictionary<string, Song> songs = new Dictionary<string, Song>()
            {
                ["newGameIntroCinBackground"] = AssetManager.LoadSong("Songs/newGameIntroCinBackground"),
                ["titleScreenBG"] = AssetManager.LoadSong("Songs/titleScreenBG"),
            };
            return songs;
        }

        public static void LoadTileSpecificAssets()
        {
            AssetManager.LoadTexture("-1_0_0", "Tiles/-1_0_0/-1_0_0");

            AssetManager.LoadTexture("0_-1_0", "Tiles/0_-1_0/0_-1_0");
            AssetManager.LoadTexture("GlowMound1_0_-1_0", "Tiles/0_-1_0/GlowMound1_0_-1_0");
            AssetManager.LoadTexture("GlowMound2_0_-1_0", "Tiles/0_-1_0/GlowMound2_0_-1_0");
            AssetManager.LoadTexture("GlowMound3_0_-1_0", "Tiles/0_-1_0/GlowMound3_0_-1_0");
            AssetManager.LoadTexture("ForeDarkFog_0_-1_0", "Tiles/0_-1_0/ForeDarkFog_0_-1_0");
            AssetManager.LoadTexture("LowerRocks_0_-1_0", "Tiles/0_-1_0/LowerRocks_0_-1_0");
        }
        public static void LoadMiscMapTileAssets()
        {
            AssetManager.LoadTexture("NoiseA", CreateNoise.GenerateTileableFBM(256, octaves: 5, lacunarity: 2f, gain: 0.5f, seed: 42));
            AssetManager.LoadTexture("NoiseB", CreateNoise.GenerateTileableFBM(256, octaves: 5, lacunarity: 2.2f, gain: 0.52f, seed: 1337));
            AssetManager.LoadTexture("BackgroundSmoke", "Tiles/BackgroundSmoke");
        }

        public static void LoadShaders()
        {
            AssetManager.LoadEffect("ColorReplace", "Shaders/ColorReplace");
            AssetManager.LoadEffect("ColorColumnPulse", "Shaders/ColorColumnPulse");
            AssetManager.LoadEffect("Smoke", "Shaders/Smoke");
        }

        public static void LoadActIcons()
        {
            AssetManager.LoadTexture("MoveActIcon", "ActIcons/MoveActIcon");
        }
    }



}
