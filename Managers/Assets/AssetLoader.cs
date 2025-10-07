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
            AssetManager.LoadTexture("FrostOozeIcicleStabDown", "MonsterAnimations/FrostOoze/FrostOozeIcicleStabDown");
            AssetManager.LoadTexture("FrostOozeIcicleStabUp", "MonsterAnimations/FrostOoze/FrostOozeIcicleStabUp");
            AssetManager.LoadTexture("FrostOozeWalkUp", "MonsterAnimations/FrostOoze/FrostOozeWalkUp");
            AssetManager.LoadTexture("FrostOozeWalkDown", "MonsterAnimations/FrostOoze/FrostOozeWalkDown");

            AssetManager.LoadTexture("EarthOozeIdle", "MonsterAnimations/EarthOoze/EarthOozeIdle");
            AssetManager.LoadTexture("EarthOozeWalkUp", "MonsterAnimations/EarthOoze/EarthOozeWalkUp");
            AssetManager.LoadTexture("EarthOozeWalkDown", "MonsterAnimations/EarthOoze/EarthOozeWalkDown");
            AssetManager.LoadTexture("EarthOozeGraspingRoot", "MonsterAnimations/EarthOoze/EarthOozeGraspingRoot");
            AssetManager.LoadTexture("EarthOozeGraspingRootVE", "MonsterAnimations/EarthOoze/EarthOozeGraspingRootVE");

            AssetManager.LoadTexture("FireOozeIdle", "MonsterAnimations/FireOoze/FireOozeIdle");
            AssetManager.LoadTexture("FireOozeWalkUp", "MonsterAnimations/FireOoze/FireOozeWalkUp");
            AssetManager.LoadTexture("FireOozeWalkDown", "MonsterAnimations/FireOoze/FireOozeWalkDown");
            AssetManager.LoadTexture("FireOozeLavaBall", "MonsterAnimations/FireOoze/FireOozeLavaBall");
            AssetManager.LoadTexture("FireOozeLavaBallVE", "MonsterAnimations/FireOoze/FireOozeLavaBallVE");

        }
        public static void LoadPlayerSpriteSheets()
        {
            AssetManager.LoadTexture("PlayerHead", "PlayerSprites/PlayerHead");
            AssetManager.LoadTexture("PlayerBody", "PlayerSprites/PlayerBody");
            AssetManager.LoadTexture("PlayerBodyColored", "PlayerSprites/PlayerBodyColored");
            AssetManager.LoadTexture("ParticleDefault", "ParticleDefault");
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

            AssetManager.LoadTexture("-1_-1_0", "Tiles/-1_-1_0/-1_-1_0");
            AssetManager.LoadTexture("ForeDarkFog_-1_-1_0", "Tiles/-1_-1_0/ForeDarkFog_-1_-1_0");
            AssetManager.LoadTexture("LowerRocks_-1_-1_0", "Tiles/-1_-1_0/LowerRocks_-1_-1_0");

            AssetManager.LoadTexture("-1_0_0", "Tiles/-1_0_0/-1_0_0");
            AssetManager.LoadTexture("ForeDarkFog_-1_0_0", "Tiles/-1_0_0/ForeDarkFog_-1_0_0");
            AssetManager.LoadTexture("LowerRocks_-1_0_0", "Tiles/-1_0_0/LowerRocks_-1_0_0");

            AssetManager.LoadTexture("0_0_0", "Tiles/0_0_0/0_0_0");
            AssetManager.LoadTexture("ForeDarkFog_0_0_0", "Tiles/0_0_0/ForeDarkFog_0_0_0");
            AssetManager.LoadTexture("LowerRocks_0_0_0", "Tiles/0_0_0/LowerRocks_0_0_0");

            AssetManager.LoadTexture("0_0_1", "Tiles/0_0_1/0_0_1");
            AssetManager.LoadTexture("BGSmoke_0_0_1", "Tiles/0_0_1/BGSmoke_0_0_1");
            AssetManager.LoadTexture("BGCloud_0_0_1", "Tiles/0_0_1/BGCloud_0_0_1");
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
            AssetManager.LoadEffect("BodySmoke", "Shaders/BodySmoke");
        }
        public static void LoadNPCTextures()
        {
            AssetManager.LoadTexture("NormalHead", "NPC/Head/NormalHead");
            AssetManager.LoadTexture("ShortWideHead", "NPC/Head/ShortWideHead");
            AssetManager.LoadTexture("SmallHead", "NPC/Head/SmallHead");
            AssetManager.LoadTexture("THead", "NPC/Head/THead");
            AssetManager.LoadTexture("TallSkinnyHead", "NPC/Head/TallSkinnyHead");

            AssetManager.LoadTexture("LeftCircular", "NPC/Eyes/LeftCircular");
            AssetManager.LoadTexture("LeftTriangle", "NPC/Eyes/LeftTriangle");
            AssetManager.LoadTexture("LeftFlat", "NPC/Eyes/LeftFlat");
            AssetManager.LoadTexture("LeftInward", "NPC/Eyes/LeftInward");
            AssetManager.LoadTexture("LeftOutward", "NPC/Eyes/LeftOutward");

            AssetManager.LoadTexture("RightCircular", "NPC/Eyes/RightCircular");
            AssetManager.LoadTexture("RightTriangle", "NPC/Eyes/RightTriangle");
            AssetManager.LoadTexture("RightFlat", "NPC/Eyes/RightFlat");
            AssetManager.LoadTexture("RightInward", "NPC/Eyes/RightInward");
            AssetManager.LoadTexture("RightOutward", "NPC/Eyes/RightOutward");

        }
        public static void LoadActIcons()
        {
            AssetManager.LoadTexture("MoveActIcon", "ActIcons/MoveActIcon");
            AssetManager.LoadTexture("EndTurnActIcon", "ActIcons/EndTurnActIcon");
        }
    }



}
