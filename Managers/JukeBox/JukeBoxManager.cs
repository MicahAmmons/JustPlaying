using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using PlayingAround.Game.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.JukeBox
{
    public static class JukeBoxManager
    {

        private static Song _previousSong;
        private static Song _nextSong;
        private static Song _currentSong;
        private static Dictionary<string, Song> _songDictionary;
        private static bool changedMusic = false;

        public static void Stop() => MediaPlayer.Stop();
        public static void Pause() => MediaPlayer.Pause();
        public static void Resume() => MediaPlayer.Resume();

        public static void InitializeJukeBox()
        {
            _songDictionary = AssetLoader.LoadAllSongs();
            SetSongTo("titleScreenBG");
            Configure();
        }
        public static void Configure(bool loop = true, float volume = 0.1f)
        {
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Volume = volume;
        }
        public static void SetSongTo(string key)
        {
            if (_songDictionary.TryGetValue(key, out var newSong))
            {
                if (_currentSong != newSong)
                {
                    _previousSong = _currentSong;
                    _currentSong = newSong;
                    changedMusic = true;
                }
            }
        }
        public static void Update(GameTime gameTime)
        {
            if (changedMusic) { UpdateSong(); changedMusic = false; }
        }
        private static void UpdateSong()
        {
            MediaPlayer.Play(_currentSong);
        }

        internal static void UpdateVolume(int currentValue)
        {
            float vol = (float)currentValue / 10f;
            MediaPlayer.Volume = MathHelper.Clamp(vol, 0f, 1f);
        }
    }
}
