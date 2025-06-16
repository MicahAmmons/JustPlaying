using Microsoft.Xna.Framework.Media;
using PlayingAround.Managers.Assets;
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
            AssetManager.LoadTexture("Hero_Blonde", "HeroArt/BlonderHero");
            AssetManager.LoadTexture("Arrow", "TileCell/arrow");
            AssetManager.LoadTexture("3Arrows", "Tilecell/3arrows");
            AssetManager.LoadTexture("MonsterIcons/AngryPlantIcon", "MonsterIcons/AngryPlantIcon");

            AssetManager.LoadTexture("fightBackground", "UI/fightBackground");

            AssetManager.LoadTexture("TitleScreenBackGround", "TitleScreen/TitleScreenBackGround");
           
            AssetManager.LoadTexture("DefaultNPC", "NPC/DefaultNPC");

            AssetManager.LoadTexture("DefaultMovementSS", "PlayerSprites/leftRightMovementTemplateSS");


            // Add fonts, sounds, etc. later
        }
        public static void LoadAttackIconTextures()
        {
            AssetManager.LoadTexture("SpitIcon", "Attacks/Spit");
        }
        public static void LoadMonsterIconTextures()
        {
            AssetManager.LoadTexture("OozeIcon", "MonsterIcons/OozeIcon");
            AssetManager.LoadTexture("TrainingDummyIcon", "MonsterIcons/TrainingDummyIcon");
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
    }
}
