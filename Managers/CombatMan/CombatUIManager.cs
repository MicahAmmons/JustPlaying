using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CombatStateMachine;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatUIManager
    {
        private CombatStateMachine _stateMachine;
        private readonly Queue<CombatMonster> _allCombatants;
        private readonly Func<List<TileCell>> _getMoveableCells;
        private readonly List<CombatMonster> _referenceList;
        public PlayerTurnState StatePlayerTurn => _stateMachine.CurrentPlayerTurnState;
        public CombatState StateCombat => _stateMachine.CurrentCombatState;
        public SummonedTurnState StateSummonedTurn => _stateMachine.CurrentSummonedTurnState;
        public AITurnState StateAITurn => _stateMachine.CurrentAITurnState;




        public CombatUIManager(CombatStateMachine stateMachine, Queue<CombatMonster> allCombatants, List<CombatMonster> referenceList)
        {
            _stateMachine = stateMachine;
            _allCombatants = allCombatants;
            _referenceList = referenceList;
            

        }
        public void Update()
        {
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (StateCombat != CombatState.None){return;}
            
            
            DrawDisplayStats(spriteBatch);


        }



        public void DrawDisplayStats(SpriteBatch spriteBatch)
        {
            int iconSize = 64;
            int spacingX = 150; // Horizontal space between icons
            int topY = 20; // Vertical offset from the top of the screen
            SpriteFont font = AssetManager.GetFont("mainFont");

            int count = _referenceList.Count;
            int totalWidth = count * spacingX;

            // Center the row
            int screenWidth = ViewportManager.ScreenWidth; // Or use GraphicsDevice.Viewport.Width if you have access
            Vector2 startingPos = new Vector2((screenWidth - totalWidth) / 2f, topY);

            int index = 0;
            foreach (var mon in _referenceList)
            {
                Vector2 iconPos = startingPos + new Vector2(index * spacingX, 0);
                Rectangle iconRect = new Rectangle((int)iconPos.X, (int)iconPos.Y, iconSize, iconSize);

                // Determine texture key
                string textureKey = mon.IsSummon ? mon.IconTextureKey : (mon.isPlayer || mon.isSummoned ? "Hero_Blonde" : mon.IconTextureKey);
                Texture2D icon = AssetManager.GetTexture(textureKey);

                // Set color depending on isDead
                Color col = mon.IsSummon ? new Color(Color.Blue, 0.3f) : Color.White;
                if (mon.isDead)
                    col = Color.Gray * 0.5f;

                // Draw monster icon
                spriteBatch.Draw(icon, iconRect, col);



                // Draw health below
                float currentHealth = MathF.Max(0, mon.CurrentHealth);
                string hpText = $"{currentHealth} / {mon.MaxHealth}";
                Vector2 textSize = font.MeasureString(hpText);
                Vector2 textPos = new Vector2(
                    iconRect.X + (iconSize - textSize.X) / 2,
                    iconRect.Bottom + 2
                );

                // Draw aspects below icon
                int aspectSize = 24;
                int aspectSpacing = 4;
                for (int i = 0; i < mon.Aspects.Count; i++)
                {
                    var aspect = mon.Aspects[i];
                    Vector2 aspectPos = new Vector2(
                        iconRect.X + i * (aspectSize + aspectSpacing),
                        iconRect.Bottom + 25
                    );
                    Rectangle aspectRect = new Rectangle((int)aspectPos.X, (int)aspectPos.Y, aspectSize, aspectSize);

                    spriteBatch.Draw(aspect.Icon, aspectRect, Color.White);

                    // Overlay duration
                    string turnsLeft = MathF.Ceiling(aspect.Duration).ToString();
                    Vector2 numSize = font.MeasureString(turnsLeft);
                    Vector2 numPos = new Vector2(
                        aspectRect.Center.X - numSize.X / 2,
                        aspectRect.Center.Y - numSize.Y / 2
                    );

                    spriteBatch.DrawString(font, turnsLeft, numPos, Color.Yellow);
                }

                spriteBatch.DrawString(font, hpText, textPos, Color.Black);
                index++;
            }
        }




    }
}
