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
using PlayingAround.Managers.DayManager;
using System;
using System.Collections.Generic;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Dialogue;
using System.Linq;


namespace PlayingAround.Managers.UI
{
    public static class UIManager
    {
        private static SpriteFont _mainFont;

        private static Texture2D _fightBackground;
        private static Player _currentPlayer => PlayerManager.CurrentPlayer;
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



        public static void LoadContent()
        {
            _mainFont = AssetManager.GetFont("mainFont");
            _fightBackground = AssetManager.GetTexture("fightBackground");
            ProximityManager.OnPlayerNearPlayMonster += HandlePlayMonsterInteract;
            ProximityManager.OnPlayerLeavePlayMonster += HandlePlayerExitPlayMonster;
            ProximityManager.OnPlayerNearNPC += HandleNPCInteract;
            ProximityManager.OnPlayerLeaveNPC += HandlePlayerExitNPC;
            ProximityManager.OnPlayerNearNextTile += HandlePlayerNextTileInteract;
            ProximityManager.OnPlayerLeaveNextTile += HandlePlayerExitNextTile;
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
            UpdateInput();
            if (_currentInteractState != InteractState.None && _interactMessage == null)
            {
                SetInteractMessage();
            }
        }

        public static void UpdateInput()
        {
            switch (SceneManager.CurrentState)
            {
                case SceneState.Play:
                    if (_currentInteractState != InteractState.None && InputManager.IsKeyPressed(Keys.F))
                    {
                        switch (_currentInteractState)
                        {
                            case InteractState.PlayMonster:
                                SceneManager.SetState(SceneState.Combat);
                                CombatGuard.CreateNewCombat(_currentPlayMonster);
                                break;
                            case InteractState.NPC:
                                SceneManager.SetState(SceneState.Dialogue);
                                DialogueManager.StartNewDialogue(_currentNPC);
                                break;
                            case InteractState.NextTile:
                                MapTileTransitionManager.SetNextMapTile(_currentNextTile.Value.Item2);
                                SceneManager.SetState(SceneState.MapTileTransition);
                                break;
                        }
                    }
                    break;
            }


        }
        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_summonOverlayOpen)
            {
                DrawSummonOverlay(spriteBatch);
            }

            DrawPlayerStatsUI(spriteBatch);
            DrawPlayerSummons(spriteBatch);
            DrawDayCount(spriteBatch);
            DrawEscapeState(spriteBatch);
            DrawInteractText(spriteBatch);
            DrawPlayMonsterDetails(spriteBatch);
            if (SceneManager.IsState(SceneState.Dialogue))
            {
                DrawCurrentState(spriteBatch);
            }
        }
        public static void DrawPlayMonsterDetails(SpriteBatch spriteBatch)
        {
            if (PlayMonsterManager.SelectedMonster != null)
            {
                PlayMonsters mon = PlayMonsterManager.SelectedMonster;
                var grouped = mon.Monsters
                 .GroupBy(mon => mon.NamePlusLevel)
                 .Select(g => new { NamePlusLevel = g.Key, Count = g.Count() });

                // Compute size of background
                var font = AssetManager.GetFont("mainFont");
                int lineHeight = 20;
                int boxWidth = 160;
                int boxHeight = grouped.Count() * lineHeight + 10;

                Vector2 anchor = PlayMonsterManager.SelectedMonsterInfoAnchor.Value;
                Rectangle backgroundBox = new Rectangle((int)anchor.X, (int)anchor.Y, boxWidth, boxHeight);

                // Draw text over it
                Vector2 textPos = anchor + new Vector2(5, 5);

                foreach (var group in grouped)
                {
                    string displayName = group.Count > 1
                        ? $"({group.Count}) {Pluralize(group.NamePlusLevel)}"
                        : group.NamePlusLevel;

                    spriteBatch.DrawString(font, displayName, textPos, Color.Green);
                    textPos.Y += lineHeight;
                }



            }
        }
        public static void DrawCurrentState(SpriteBatch spriteBatch)
        {
            string state = $"{SceneManager.CurrentState}";
            spriteBatch.DrawString(_mainFont, state, new Vector2(900, 50), ColorPalette.DarkColor);
        }
        public static void DrawDayCount(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_mainFont, $"Day {DayManager.DayCycleManager.FetchDays()}", new Vector2(1700, 50 ), ColorPalette.DarkColor);
        }
        public static void DrawEscapeState(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_mainFont, $"{EscapeOverseer.EscapeOverseer.CurrentEscapeState}", new Vector2(1700, 100), ColorPalette.DarkColor);
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

            foreach (var summon in _currentPlayer.stats.UnlockedSummons)
            {
                // 1. Draw Icon
                Rectangle iconRect = new Rectangle(
                    _summonOverlayRect.X + 10,
                    startY,
                    32,
                    32
                );
                spriteBatch.Draw(AssetManager.GetTexture(summon.IconTextureString), iconRect, Color.White);

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
            if (SceneManager.IsState(SceneState.Play))
            {

                _playerStats = $"Health: {_currentPlayer.stats.CurrentHealth} / {_currentPlayer.stats.CurrentHealth}\n" +
                               $"Mana: {_currentPlayer.stats.CurrentMana} / {_currentPlayer.stats.CurrentMana}";
            }
            if (SceneManager.IsState(SceneState.Combat))
            {
                CombatMonster mon = CombatGuard.CurrentCombat.CurrentMonster;
                _playerMonster = CombatGuard.CurrentCombat.GetPlayerMonster();
                _standInMonster = mon;
                _playerStats = $"Health: {_playerMonster.CurrentHealth} / {_playerMonster.BaseHealth}\n" +
                               $"Speed: {_playerMonster.CurrentMP} / {_playerMonster.MP}";
            }
        }



        // Section devoted to Interaction popups
        //
        //
        private static InteractState _currentInteractState = InteractState.None;
        private static PlayMonsters _currentPlayMonster;
        private static NPC _currentNPC;
        private static (Vector2, NextTileData)? _currentNextTile;
        private static Rectangle _interactRectangle;
        private static string? _interactMessage;
        private static void DrawInteractText(SpriteBatch spriteBatch)
        {
            if (!SceneManager.IsState( SceneState.Play)) return;
            if (_interactMessage != null)
            {
                int padding = 6;
                spriteBatch.Draw(_fightBackground, _interactRectangle, ColorPalette.DarkColor * .5f);

                Vector2 textPosition = new Vector2(_interactRectangle.X + padding, _interactRectangle.Y + padding);
                spriteBatch.DrawString(_mainFont, _interactMessage, textPosition, ColorPalette.LightColor);
            }
        }
        private static void HandlePlayerExitPlayMonster()
        {
            _currentPlayMonster = null;
            if (_currentInteractState == InteractState.PlayMonster)
            {
                _currentInteractState = InteractState.None;
                _interactMessage = null;
                
            }
        }
        private static void HandlePlayMonsterInteract(PlayMonsters mon)
        {
            _currentInteractState = InteractState.PlayMonster;            
            _currentPlayMonster = mon;
        }
        private static void HandleNPCInteract(NPC npc)
        {
            _currentInteractState = InteractState.NPC;
            _currentNPC = npc;
        }
        private static void HandlePlayerExitNPC()
        {
            _currentNPC = null;
            if (_currentInteractState == InteractState.NPC)
            {
                _currentInteractState = InteractState.None;
                _interactMessage = null;
            }
        }
        private static void HandlePlayerNextTileInteract(Vector2 center, NextTileData nextTileData)
        {
            _currentInteractState = InteractState.NextTile;
            _currentNextTile = (center, nextTileData);
        }
        private static void HandlePlayerExitNextTile()
        {
            _currentNextTile = null;
            if (_currentInteractState == InteractState.NextTile)
            {
                _currentInteractState = InteractState.None;
                _interactMessage = null;
            }
        }
        private static void SetInteractMessage()
        {
            string message = "ERROR IN UI, NO MESSAGE SET";
            Vector2 drawPoint = new Vector2(50, 50);

            switch (_currentInteractState)
            {
                case InteractState.PlayMonster:
                    message = "Press F to Fight";
                    drawPoint = _currentPlayMonster.CurrentPos;
                    break;

                case InteractState.NPC:
                    message = "Press F to Talk";
                    drawPoint = _currentNPC.currentPos;
                    break;
                case InteractState.NextTile:
                    message = "Press F to travel";
                    drawPoint = _currentNextTile.Value.Item1;
                        break;
            }

            InteractMessage(message, drawPoint);
        }
        private static void InteractMessage(string message, Vector2 drawPoint)
        {
            int padding = 6;

            Vector2 textSize = _mainFont.MeasureString(message);
            int totalWidth = (int)textSize.X + padding * 2;
            int totalHeight = (int)textSize.Y + padding * 2;

            _interactRectangle = new Rectangle((int)drawPoint.X, (int)drawPoint.Y, totalWidth, totalHeight);
            _interactMessage = message;
        }

        private static string Pluralize(string name)
        {
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !name.EndsWith("ey"))
                return name.Substring(0, name.Length - 1) + "ies";
            else if (name.EndsWith("s"))
                return name;
            else
                return name + "s";
        }

    }

}
public enum InteractState
{
    None,
    PlayMonster,
    NPC,
    NextTile
}