using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Proximity;
using PlayingAround.Managers;
using System;
using System.Collections.Generic;

namespace PlayingAround.Managers.UI
{
    public static class UIManager
    {
        private static SpriteFont _mainFont;
        private static bool _fightBox = false;
        private static PlayMonsters _playMonsters;
        private static Texture2D _fightBackground;
        private static Player _player;
        private static CombatMonster _playerMonster;
        private static CombatMonster _standInMonster;

        private static string _playerStats;

        // 🆕 Moved rectangles to static fields
        private static Rectangle _playerStatsRect = new Rectangle(1600, 880, 200, 100);
        private static Rectangle _summonRect = new Rectangle(
            1600 - 200 - 10,  // 10px gap left
            880,
            200,
            100
        );
        private static bool _summonMenuOpen = false;
        private static List<Rectangle> _summonButtons = new(); // Each summon gets its own button
        private static int _summonButtonHeight = 40; // Height per summon option
                                                     // == Summon Overlay UI ==
        private static bool _summonOverlayOpen = false;
        private static Rectangle _summonOverlayRect;
        private static Rectangle _tabAreaRect;
        private static int _overlayMarginLeft = 100;
        private static int _overlayMarginTop = 200;
        private static int _overlayMarginBottom = 200;
        private static int _overLayMarginRight = 800;



        public static void LoadContent(Player player)
        {
            _mainFont = AssetManager.GetFont("mainFont");
            _fightBackground = AssetManager.GetTexture("fightBackground");
            ProximityManager.OnPlayerNearMonster += HandleFightPrompt;
            ProximityManager.OnPlayerLeaveMonster += HandlePlayerExit;
            _player = player;
            int screenWidth = ViewportManager.ScreenWidth;
            int screenHeight = ViewportManager.ScreenHeight;

            _summonOverlayRect = new Rectangle(
                _overlayMarginLeft,
                _overlayMarginTop,
                _overLayMarginRight, // 50px margin on right
                screenHeight - _overlayMarginTop - _overlayMarginBottom
            );

            _tabAreaRect = new Rectangle(
                _summonOverlayRect.X,
                _summonOverlayRect.Y,
                _summonOverlayRect.Width,
                40 // Tab height (can change later)
            );
        }

        public static void Update(GameTime gameTime)
        {
            UpdatePlayer();
        }

        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_fightBox && SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                spriteBatch.Draw(_fightBackground, new Vector2(960, 15), Color.Black);
                if (InputManager.IsKeyPressed(Keys.Enter))
                {
                    SceneManager.SetState(SceneManager.SceneState.Combat);
                    CombatManager.BeginCombat(_playMonsters, _player);
                }
            }
            if (_summonOverlayOpen)
            {
                DrawSummonOverlay(spriteBatch);
            }

            DrawPlayerStatsUI(spriteBatch);
            DrawPlayerSummons(spriteBatch);
        }
        private static void DrawSummonOverlay(SpriteBatch spriteBatch)
        {
            // Draw window background
            spriteBatch.Draw(_fightBackground, _summonOverlayRect, Color.Black * 0.3f);

            // Draw tab area
            spriteBatch.Draw(_fightBackground, _tabAreaRect, Color.DarkSlateGray);

            // Draw "Summons" tab text
            string tabLabel = "Summons";
            Vector2 textSize = _mainFont.MeasureString(tabLabel);
            Vector2 textPos = new Vector2(
                _tabAreaRect.X + (_tabAreaRect.Width - textSize.X) / 2,
                _tabAreaRect.Y + (_tabAreaRect.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(_mainFont, tabLabel, textPos, Color.White);
            // Y offset to start drawing summons under the tab
            int startY = _tabAreaRect.Bottom + 10;
            int summonRowHeight = 40; // Each summon row height

            foreach (var summon in _player.stats.UnlockedSummons)
            {
                // 1. Draw Icon
                Rectangle iconRect = new Rectangle(
                    _summonOverlayRect.X + 10,
                    startY,
                    32,
                    32
                );
                spriteBatch.Draw(summon.IconTexture, iconRect, Color.White);

                // 2. Draw Name + Level
                string nameAndLevel = $"{summon.Name} (Lv {summon.Level})";
                Vector2 namePos = new Vector2(iconRect.Right + 10, startY);
                spriteBatch.DrawString(_mainFont, nameAndLevel, namePos, Color.White);

                // 3. Draw XP Progress Bar
                int barWidth = 100;
                int barHeight = 20;
                Rectangle xpBarBackground = new Rectangle(iconRect.Right + 200, startY + 10, barWidth, barHeight);
                Rectangle xpBarFill = new Rectangle(xpBarBackground.X, xpBarBackground.Y, (int)(barWidth * summon.XPProgressPercent), barHeight);

                spriteBatch.Draw(_fightBackground, xpBarFill, Color.PaleVioletRed);      // fill based on % XP
                spriteBatch.Draw(_fightBackground, xpBarBackground, Color.Transparent); // background


                // 4. Draw XP text (ex: 50/100)
                string xpText = $"{summon.CurrentXP} / {summon.XPNeededForNextLevel}";
                Vector2 xpTextSize = _mainFont.MeasureString(xpText);
                Vector2 xpTextPos = new Vector2(xpBarBackground.Right + 10, xpBarBackground.Y - (xpTextSize.Y / 2) + (barHeight / 2));
                spriteBatch.DrawString(_mainFont, xpText, xpTextPos, Color.White);

                // Move down for next summon
                startY += summonRowHeight;
            }

        }

        private static void DrawPlayerStatsUI(SpriteBatch spriteBatch)
        {
            if (_playerStats != null)
            {
                spriteBatch.Draw(_fightBackground, _playerStatsRect, Color.Aqua);

                Vector2 textSize = _mainFont.MeasureString(_playerStats);
                Vector2 textPosition = new Vector2(
                    _playerStatsRect.X + (_playerStatsRect.Width - textSize.X) / 2,
                    _playerStatsRect.Y + (_playerStatsRect.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(_mainFont, _playerStats, textPosition, Color.White);
            }
        }

        private static void DrawPlayerSummons(SpriteBatch spriteBatch)
        {
            if (_summonRect.Contains(InputManager.Mouse.Position) && InputManager.IsLeftClick())
            {
                _summonOverlayOpen = !_summonOverlayOpen;
            }



            spriteBatch.Draw(_fightBackground, _summonRect, Color.Aqua);

            string summonsLabel = "Summons";
            Vector2 summonsTextSize = _mainFont.MeasureString(summonsLabel);
            Vector2 summonsTextPosition = new Vector2(
                _summonRect.X + (_summonRect.Width - summonsTextSize.X) / 2,
                _summonRect.Y + (_summonRect.Height - summonsTextSize.Y) / 2
            );
            spriteBatch.DrawString(_mainFont, summonsLabel, summonsTextPosition, Color.White);

            Vector2 mousePos = new Vector2(InputManager.MouseX, InputManager.MouseY);

            if (_summonRect.Contains(mousePos))
            {
                spriteBatch.Draw(_fightBackground, _summonRect, Color.Yellow * 0.3f);

                if (InputManager.IsLeftClick())
                    _summonMenuOpen = !_summonMenuOpen; // ✅ Toggle menu open/close
            }
        }



        private static void UpdatePlayer()
        {
            if (SceneManager.CurrentState == SceneManager.SceneState.Play)
            {
                _playerStats = $"Health: {_player.stats.CurrentHealth} / {_player.stats.CurrentHealth}\n" +
                               $"Mana: {_player.stats.CurrentMana} / {_player.stats.CurrentMana}";
            }
            if (SceneManager.CurrentState == SceneManager.SceneState.Combat)
            {
                _playerMonster = CombatManager.GetPlayerMonster();
                _standInMonster = CombatManager.GetStandInMonster();
                _playerStats = $"Health: {_playerMonster.CurrentHealth} / {_playerMonster.MaxHealth}\n" +
                               $"Mana: {_playerMonster.CurrentMana} / {_playerMonster.CurrentMana}\n" +
                               $"Speed: {_standInMonster.MP}";
            }
        }

        private static void HandlePlayerExit()
        {
            _fightBox = false;
            _playMonsters = null;
        }

        private static void HandleFightPrompt(PlayMonsters mon)
        {
            _fightBox = true;
            _playMonsters = mon;
        }
    }
}