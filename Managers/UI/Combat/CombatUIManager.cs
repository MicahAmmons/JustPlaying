using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Interfaces;
using PlayingAround.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.UI.Combat
{
    public class CombatUIManager
    {
        private List<CombatantInfoUI> _combatantInfo = new List<CombatantInfoUI>();

        public CombatUIManager() { }
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawCombatantUIInfo(spriteBatch);
        }
        public void Update()
        {
            UpdateMouseHoverIcon();
        }
        public void UpdateMouseHoverIcon()
        {
            Vector2 mouse = new Vector2(InputManager.MouseX, InputManager.MouseY);
            foreach (var info in  _combatantInfo)
            {
                info.ToggleMouseHover(mouse);
            }
        }
        public void DrawCombatantUIInfo(SpriteBatch spriteBatch)
        {
            if (_combatantInfo.Count <= 0) return;
            foreach (var ui in _combatantInfo)
            {
                ui.Draw(spriteBatch);
            }
        }
        public void ClearCombatantUIInfo()
        {
            _combatantInfo.Clear();
        }
        public void AddCombatantUIInfo(CombatantInfoUI combtUI)
        {
            _combatantInfo.Add(combtUI);
        }
        public void RemoveCombatantUI(ICombatant mon)
        {
            foreach (var ui in _combatantInfo)
            {
                if (ui.combatant == mon)
                {
                    _combatantInfo.Remove(ui);
                    break;
                }
            }
        }
    }
}
