using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using PlayingAround.Game.Assets;
using PlayingAround.Utils;

namespace PlayingAround.Managers.Assets
{
    public static class AssetManager
    {
        private static ContentManager _content;
        private static Dictionary<string, Texture2D> _textures = new();
        private static Dictionary<string, SpriteFont> _fonts = new();
        private static Dictionary<string, Song> _songs = new();
        private static Dictionary<string, Effect> _effects = new();



        internal static void LoadAllAssets()
        {
            AssetLoader.LoadElementIconTextures();
            AssetLoader.LoadAllTextures();
            AssetLoader.LoadAllFonts();
            AssetLoader.LoadAttackIconTextures();
            AssetLoader.LoadMonsterIconTextures();
            AssetLoader.LoadAllSpriteSheets();
            AssetLoader.LoadPlayerIconTextures();
            AssetLoader.LoadTileSpecificAssets();
            AssetLoader.LoadShaders();
            AssetLoader.LoadPlayerSpriteSheets();
            LoadCustomAssets();
        }
        public static void LoadCustomAssets()
        {
            Texture2D text =DrawDiamondTexture.GetDiamond(128, 64, Color.White * 0.5f);
            _textures["CellDiamond"] = text;
        }
        // Step 1: Initialize with the Content pipeline
        public static void Initialize(ContentManager content)
        {
            _content = content;
            LoadAllAssets();
        }
      

        // Step 2: Load a texture and store it in the dictionary
        public static void LoadTexture(string key, string path)
        {
            Texture2D texture = _content.Load<Texture2D>(path);
            _textures[key] = texture;
        }

        // Step 3: Get a previously loaded texture
        public static Texture2D GetTexture(string key)
        {
            return _textures[key];
        }
        public static bool TextureExists(string key)
        {
            return _textures.ContainsKey(key);
        }
        public static Song GetSong(string key)
        {
            return _songs[key];
        }
        public static Song LoadSong(string path)
        {
            return _content.Load<Song>(path);
        }
        public static void LoadFont(string key, string path) =>
            _fonts[key] = _content.Load<SpriteFont>(path);
        public static SpriteFont GetFont(string key) => _fonts[key];
        public static Texture2D GetIconWithElementColored(Texture2D icon, ElementType element)
        {

            Color tint = element switch
            {
                ElementType.Fire => Color.Red,
                ElementType.Ice => Color.White,
                ElementType.Earth => new Color(139, 69, 19),     // brown
                ElementType.Wind => Color.White,
                ElementType.Acid => new Color(144, 238, 144),    // light green
                ElementType.Metal => Color.Silver,
                ElementType.Electricity => Color.Yellow,
                ElementType.Light => Color.Pink,
                ElementType.Dark => Color.Black,
                ElementType.Normal => Color.LightGray,
                _ => Color.White
            };

            return TintTexture(icon, tint);
        }
        private static Texture2D TintTexture(Texture2D original, Color tint)
        {
            Texture2D tinted = new Texture2D(original.GraphicsDevice, original.Width, original.Height);
            Color[] data = new Color[original.Width * original.Height];
            original.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                Color originalColor = data[i];
                // Preserve alpha, but apply tint to RGB
                data[i] = new Color(
                    (originalColor.R * tint.R) / 255,
                    (originalColor.G * tint.G) / 255,
                    (originalColor.B * tint.B) / 255,
                    originalColor.A
                );
            }

            tinted.SetData(data);
            return tinted;
        }

        public static void LoadEffect(string key, string path)
        {
            Effect effect = _content.Load<Effect>(path);
            _effects[key] = effect;
        }

        internal static Effect GetEffect(string v)
        {
            return _effects[v];
        }
    }
}
