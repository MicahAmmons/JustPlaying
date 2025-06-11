using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.SaveData;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.JukeBox;
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

            int arrowWidth = 30;
            int arrowHeight = 30;
            int rightPadding = 15;
            int arrowVerticalSpacing = 10;
            int arrowHorizontalSpacing = 50;

            int settingIndex = 0;
            foreach (var setting in _settingData.Values)
            {
                int yOffset = settingIndex * (SettingHeight + SettingMargin);
                setting.RenderRect = new Rectangle(0, yOffset, PanelWidth, SettingHeight);
                settingIndex++;


                setting.UpArrowRect = new Rectangle(
                    setting.RenderRect.Right - arrowWidth - rightPadding,
                    setting.RenderRect.Y + arrowVerticalSpacing,
                    arrowWidth,
                    arrowHeight
                );

                setting.DownArrowRect = new Rectangle(
                    setting.RenderRect.Right - arrowWidth - rightPadding,
                    setting.RenderRect.Y + arrowHorizontalSpacing,
                    arrowWidth,
                    arrowHeight
                );

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
                if (setting.Value == null) // Value is null if there no previously saved setting, so defaults to default value
                {
                    _settingData[setting.Key].CurrentValue = _settingData[setting.Key].DefaultValue ;
                    continue;
                }
                _settingData[setting.Key].CurrentValue = setting.Value.CurrentValue;
            }
            foreach (var setting in _settingData)
            {
                NotifyNecessarySystemsOfCurrentValue(setting.Value);
            }
            
        }
        public static void Update()
        {
            if (_currentSettingState == SettingSuperState.None) { return; }
            switch (_currentSettingState)
            {

            }
            UpdateUserInput();
        }
        public static void UpdateUserInput()
        {
            var mousePoint = new Vector2(InputManager.MouseX, InputManager.MouseY);
            

            TrackScrollWheel();
            TrackUserArrowClicks(mousePoint);
            switch (_currentSettingState)
            {
                case SettingSuperState.MainPage:

                    break;
            }
        }
        public static void TrackUserArrowClicks(Vector2 mouse)
        {
            if (InputManager.IsLeftClick())
            {
                foreach (var setting in _settingData.Values)
                {
                    if (setting.Type != SettingType.Numerical) continue;

                    Rectangle up = OffsetByScroll(setting.UpArrowRect);
                    Rectangle down = OffsetByScroll(setting.DownArrowRect);



                    if (up.Contains(mouse))
                    {
                        setting.CurrentValue = Math.Min(setting.CurrentValue + 1, setting.MaxValue);
                        UpdateSettingData(setting);
                    }
                    else if (down.Contains(mouse))
                    {
                        setting.CurrentValue = Math.Max(setting.CurrentValue - 1, setting.MinValue);
                        UpdateSettingData(setting);
                    }
                }
            }

        }
        private static void UpdateSettingData(Setting setting)
        {
            _settingData[setting.Name] = setting;
            NotifyNecessarySystemsOfCurrentValue(setting);
        }
        private static void NotifyNecessarySystemsOfCurrentValue(Setting setting)
        {
            switch (setting.Name)
            {
                case "Volume":
                    JukeBoxManager.UpdateVolume(setting.CurrentValue);
                    break;
            }
        }
        private static Rectangle OffsetByScroll(Rectangle rect)
        {
            return new Rectangle(
                _panelRect.X + rect.X,
                _panelRect.Y + rect.Y - _scrollOffset,
                rect.Width,
                rect.Height
            );
        }

        public static void TrackScrollWheel()
        {
            int scrollChange = InputManager.ScrollWheelChange;
            if (scrollChange != 0)
            {
                _scrollOffset -= scrollChange / 5; // Adjust scroll sensitivity
                _scrollOffset = Math.Clamp(_scrollOffset, 0, Math.Max(0, _totalContentHeight - PanelHeight));
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_currentSettingState == SettingSuperState.None) { return; }
            // Background panel
            spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), _panelRect, ColorPalette.LightColor );

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

                // Draw setting background
                spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), screenRect, ColorPalette.DarkColor);

                SpriteFont font = AssetManager.GetFont("mainFont");

                // Draw setting name + current value
                string label = $"{setting.Name}: {setting.CurrentValue}";
                Vector2 labelPos = new Vector2(screenRect.X + 10, screenRect.Y + 10);
                spriteBatch.DrawString(font, label, labelPos, ColorPalette.LightColor);

                // Draw up arrow
                Rectangle upArrow = OffsetByScroll(setting.UpArrowRect);
                spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), upArrow, ColorPalette.LightColor);
                Vector2 upTextPos = new Vector2(
                    upArrow.X + (upArrow.Width / 2) - font.MeasureString("^").X / 2,
                    upArrow.Y + (upArrow.Height / 2) - font.MeasureString("^").Y / 2
                );
                spriteBatch.DrawString(font, "^", upTextPos, ColorPalette.LightColor);

                // Draw down arrow
                Rectangle downArrow = OffsetByScroll(setting.DownArrowRect);
                spriteBatch.Draw(AssetManager.GetTexture("fightBackground"), downArrow, ColorPalette.LightColor);
                Vector2 downTextPos = new Vector2(
                    downArrow.X + (downArrow.Width / 2) - font.MeasureString("v").X / 2,
                    downArrow.Y + (downArrow.Height / 2) - font.MeasureString("v").Y / 2
                );
                spriteBatch.DrawString(font, "v", downTextPos, ColorPalette.LightColor);
            }

        }


        internal static Dictionary<string, SettingSaveData> SaveSettingData()
        {
            Dictionary<string, SettingSaveData> saveData = new();

            foreach (var setting in _settingData)
            {
                if (setting.Value.CurrentValue == -1) continue;

                saveData.Add(setting.Key, new SettingSaveData
                {
                    Name = setting.Key,
                    CurrentValue = setting.Value.CurrentValue
                });
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