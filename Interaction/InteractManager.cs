using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PlayingAround.Managers.Proximity;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Managers.NPCHouse;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Manager;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers.Dialogue;
using static System.Net.Mime.MediaTypeNames;
using PlayingAround.ButtonsFolder;
using System.Buffers.Text;
using PlayingAround.Managers.Entities;

namespace PlayingAround.Interaction
{
    public static class InteractManager
    {
        private static List<InteractData> _interactData = new List<InteractData>();
        private static bool _layoutDirty;


        public static void Initialize()
        {
            ProximityManager.OnPlayerNearPlayMonster += HandlePlayMonsterInteract;
            ProximityManager.OnPlayerLeavePlayMonster += HandleExitPlayMonsterRange;

            ProximityManager.OnPlayerNearNPC += HandleNPCInteract;
            ProximityManager.OnPlayerLeaveNPC += HandleExitNPC;

            ProximityManager.OnPlayerNearNextTile += HandleNextTileInteract;
            ProximityManager.OnPlayerLeaveNextTile += HandleExitNextTile;


        }
        public static void Draw(SpriteBatch sb)
        {
            DrawInteractData(sb);
        }
        public static void Update()
        {
            UpdateInput();
            RelayoutPrompts();
        }
        private static void UpdateInput()
        {
            if (_interactData.Count <= 0 || _interactData == null) return;
            bool leftPressedThisFrame = InputManager.IsLeftClick();
            foreach (var data in _interactData)
            {
                if (InputManager.IsKeyPressed(data.KeyToPress))
                {
                    data.BeginInteraction();
                    ClearInteractions();
                    ProximityManager.ClearCurrentRange();
                    return;
                }
            }
        }
        private static void ClearInteractions()
        {
            _interactData.Clear();
        }
        private static void HandlePlayMonsterInteract(PlayMonsters mon)
        {
            var data = GenerateCombatInteract(mon);
            AddInteractData(data);
        }
        private static void HandleExitPlayMonsterRange(PlayMonsters mon)
        {
            var data = _interactData
                .OfType<InteractDataCombat>()
                .FirstOrDefault(d => d.PlayMon == mon);
            if (data != null)
            {
                RemoveInteractData(data);
            }
        }
        private static void HandleNPCInteract(NPC npc)
        {
            var data = GenerateDialogueInteract(npc);
            AddInteractData(data);
        }
        private static void HandleExitNPC(NPC npc)
        {
            var data = _interactData
                .OfType<InteractDataDialogue>()
                .FirstOrDefault(d => d.Npc == npc);
            if (data != null)
            {
                RemoveInteractData(data);
            }
        }
        private static void HandleNextTileInteract(NextTileData nextTile)
        {
            var data = GenerateNextTileInteract(nextTile);
            AddInteractData(data);
        }
        public static void HandleExitNextTile(NextTileData data)
        {
            var nextTileData = _interactData
                .OfType<InteractDataNextTile>()
                .FirstOrDefault(d => d.NextTile == data);
            if (nextTileData != null)
            {
                RemoveInteractData(nextTileData);
            }
        }
        private static InteractData GenerateNextTileInteract(NextTileData data)
        {
            InteractDataNextTile nextData = new InteractDataNextTile()
            {
                NextTile = data,
            };
            nextData.Button = new Button(Rectangle.Empty);
            return nextData;
        }
        private static InteractData GenerateDialogueInteract(NPC npc)
        {
            InteractDataDialogue diaData = new InteractDataDialogue()
            {
                Npc = npc,
            };
            diaData.Button = new Button(Rectangle.Empty);
            return diaData;
        }
        public static InteractData GenerateCombatInteract(PlayMonsters mon)
        {

            InteractDataCombat comData = new InteractDataCombat()
            {
                PlayMon = mon
            };
            comData.Button = new Button(Rectangle.Empty);
            return comData;
        }
        private static void RemoveInteractData(InteractData data)
        {
            _interactData.Remove(data);
            _layoutDirty = true;
        }
        private static void AddInteractData(InteractData data)
        {
            _interactData.Add(data);
            _layoutDirty = true;
        }
        private static void DrawInteractData(SpriteBatch spriteBatch)
        {
            if (_interactData == null || _interactData.Count <= 0) return;
            var font = AssetManager.GetFont("mainFont");

            foreach (var data in _interactData)
            {
                if (data.Button.DrawRectangle == Rectangle.Empty) continue;
                data.Button.Draw(spriteBatch);
                Rectangle rect = data.Button.DrawRectangle;
                Vector2 textSize = font.MeasureString(data.Text ?? string.Empty);
                float x = rect.X + (rect.Width - textSize.X) * 0.5f;
                float y = rect.Y + (rect.Height - textSize.Y) * 0.5f;
                var textPos = new Vector2(MathF.Round(x), MathF.Round(y));
                spriteBatch.DrawString(font, data.Text, textPos, ColorPalette.LightColor);
            }
        }
        private static void RelayoutPrompts()
        {
            if (_interactData == null || _interactData.Count == 0) return;
            if (!_layoutDirty) return;
            _layoutDirty = false;
            var font = AssetManager.GetFont("mainFont");

            const int marginTop = 20;
            const int marginRight = 20;
            const int pad = 8;
            const int spacing = 8;

            int screenW = ViewportManager.ScreenWidth;
            int screenH = ViewportManager.ScreenHeight;
            int maxWidth = 0;
            var sizes = new List<Point>(_interactData.Count);
            foreach (var d in _interactData)
            {
                string text = d.Text ?? string.Empty;
                var sz = font.MeasureString(text);
                int w = (int)Math.Ceiling(sz.X) + pad * 2;
                int h = (int)Math.Ceiling(sz.Y) + pad * 2;
                sizes.Add(new Point(w, h));
                if (w > maxWidth) maxWidth = w;
            }
            int x = screenW - marginRight - maxWidth;
            int y = marginTop;

            for (int i = 0; i < _interactData.Count; i++)
            {
                var d = _interactData[i];
                int h = sizes[i].Y;

                var rect = new Rectangle(x, y, maxWidth, h);

                d.Button.DrawRectangle = rect;

                y += h + spacing;

                if (y > screenH - marginTop) break;
            }
        }
    }
}
