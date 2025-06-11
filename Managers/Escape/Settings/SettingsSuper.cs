using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.SaveData;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Escape.Settings
{
    public static class SettingsSuper
    {
        private static Dictionary<string, Setting> _settingData;
        private static SettingSuperState _currentSettingState;


        private static int _scrollOffset = 0;
        private const int PanelWidth = 800;
        private const int PanelHeight = 800;
        private const int SettingHeight = 100;
        private const int SettingMargin = 10;
        private static Rectangle _panelRect;
        private static int _totalContentHeight;
        public static void LoadContent()
        {
            _settingData = JsonLoader.LoadSettingData();


            int settingIndex = 0;
            foreach (var setting in _settingData.Values)
            {
                int yOffset = settingIndex * (SettingHeight + SettingMargin);
                setting.RenderRect = new Rectangle(0, yOffset, PanelWidth, SettingHeight);
                settingIndex++;
            }

            _totalContentHeight = settingIndex * (SettingHeight + SettingMargin);
            _panelRect = new Rectangle(
                (ViewportManager.ScreenWidth - PanelWidth) / 2,
                (ViewportManager.ScreenHeight - PanelHeight) / 2,
                PanelWidth,
                PanelHeight
            );
        }
        public static void LoadSaveContent(Dictionary<string, SettingSaveData> data)
        {
            foreach (var setting in data)
            {
                if (setting.Value == null)
                {
                    _settingData[setting.Key].CurrentValue = _settingData[setting.Key].DefaultValue ;
                    continue;
                }
                _settingData[setting.Key].CurrentValue = setting.Value.CurrentValue;
            }
            
        }
        public static void Update()
        {
            switch (_currentSettingState)
            {
                case SettingSuperState.None:
                    SettingsSuper.SetSettingSuperState(SettingSuperState.MainPage);
                    break;
            }
            UpdateUserInput();
        }
        public static void UpdateUserInput()
        {
            var mousePoint = new Vector2(InputManager.MouseX, InputManager.MouseY);
            int scrollChange = InputManager.ScrollWheelChange;
            if (scrollChange != 0)
            {
                _scrollOffset -= scrollChange / 5; // Adjust scroll sensitivity
                _scrollOffset = Math.Clamp(_scrollOffset, 0, Math.Max(0, _totalContentHeight - PanelHeight));
            }
            switch (_currentSettingState)
            {
                case SettingSuperState.MainPage:

                    break;
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            // Background panel
            spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), _panelRect, Color.Black );

            // Begin clipping (manual, since SpriteBatch doesn't clip)
            foreach (var setting in _settingData.Values)
            {
                Rectangle rect = setting.RenderRect;
                Rectangle screenRect = new Rectangle(
                    _panelRect.X + rect.X,
                    _panelRect.Y + rect.Y - _scrollOffset,
                    rect.Width,
                    rect.Height
                );

                // Skip if off screen
                if (screenRect.Bottom < _panelRect.Top || screenRect.Top > _panelRect.Bottom)
                    continue;

                spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), screenRect, Color.DarkGray);
                spriteBatch.DrawString(AssetManager.GetFont("mainFont"), setting.Name, new Vector2(screenRect.X + 10, screenRect.Y + 10), Color.White);
                spriteBatch.DrawString(AssetManager.GetFont("mainFont"), $"Value: {setting.CurrentValue}", new Vector2(screenRect.X + 10, screenRect.Y + 40), Color.LightBlue);
            }
        }


        internal static Dictionary<string, SettingSaveData> SaveSettingData()
        {
            Dictionary<string, SettingSaveData> saveData = new Dictionary<string, SettingSaveData>();

            foreach (var setting in _settingData)
            {
                if (setting.Value.CurrentValue == null) { continue; }
                saveData[setting.Key].CurrentValue = _settingData[setting.Key].CurrentValue;
            }
            return saveData;
        }

        internal static void SetSettingSuperState(SettingSuperState state)
        {
            _currentSettingState = state;
        }
    }
}
public enum SettingSuperState
{
    None,
    MainPage,
}