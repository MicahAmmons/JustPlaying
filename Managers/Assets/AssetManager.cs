using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlayingAround.Managers.Assets
{
    public static class AssetManager
    {
        private static ContentManager _content;
        private static Dictionary<string, Texture2D> _textures = new();
        private static Dictionary<string, SpriteFont> _fonts = new();
      
        


        // Step 1: Initialize with the Content pipeline
        public static void Initialize(ContentManager content)
        {
            _content = content;

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
        public static void LoadFont(string key, string path) =>
            _fonts[key] = _content.Load<SpriteFont>(path);

        public static SpriteFont GetFont(string key) => _fonts[key];
    }
}
