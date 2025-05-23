﻿using PlayingAround.Managers.Assets;

namespace PlayingAround.Game.Assets
{
    public static class AssetLoader
    {
        public static void LoadAllAssets()
        {
            AssetManager.LoadTexture("Hero_Idle", "HeroArt/BlonderHero");
            AssetManager.LoadTexture("Hero_Blonde", "HeroArt/BlonderHero");
            AssetManager.LoadTexture("Arrow", "TileCell/arrow");
            AssetManager.LoadTexture("3Arrows", "Tilecell/3arrows");
            AssetManager.LoadTexture("MonsterIcons/OozeIcon", "MonsterIcons/OozeIcon");
            AssetManager.LoadTexture("MonsterIcons/AngryPlantIcon", "MonsterIcons/AngryPlantIcon");

            AssetManager.LoadTexture("fightBackground", "UI/fightBackground");
            AssetManager.LoadTexture("AcidSpitIcon", "Attacks/acidSpit");
            AssetManager.LoadTexture("acidIcon", "Aspects/acidIcon");
            AssetManager.LoadTexture("TitleScreenBackGround", "TitleScreen/TitleScreenBackGround");


            AssetManager.LoadFont("mainFont", "mainFont");
            AssetManager.LoadFont("titleScreenButtonFont", "TitleScreenButtonFont");


            // Add fonts, sounds, etc. later
        }
    }
}
