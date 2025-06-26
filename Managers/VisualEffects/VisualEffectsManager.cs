using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Managers.Assets;
using PlayingAround.Visuals;
using System.Collections.Generic;

namespace PlayingAround.Managers.CombatMan
{
    public  class VisualEffectManager
    {
        private List<VisualEffect> _activeEffects = new();
        private SpriteFont _font = AssetManager.GetFont("mainFont");
        public bool HasActiveEffects { get; set; } = false;
        public void AddEffect(VisualEffect effect)
        {
            _activeEffects.Add(effect);
        }

        public void Update(float delta)
        {
            if (_activeEffects.Count == 0) {HasActiveEffects =  false; return; } 
            else HasActiveEffects = true;
                for (int i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    _activeEffects[i].Update(delta);
                    if (_activeEffects[i].IsFinished)
                    {
                        _activeEffects.RemoveAt(i);
                    }
                }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var effect in _activeEffects)
            {
                effect.Draw(spriteBatch, _font);
            }
        }
    }
}
