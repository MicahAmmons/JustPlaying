using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan;
using PlayingAround.Managers;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using PlayingAround.Managers.Tiles;

public class CombatantInfoUI
{
    public readonly ICombatant combatant;
    const int iconSize = 64;
    const int spacingX = 150;
    const int topY = 20;

    public bool isCurrentCombatant = false;

    public Rectangle IconRectangle;
    public Rectangle HealthTextRectangle;
    public Rectangle MpTextRectangle;
    public Rectangle ApTextRectangle;
    public List<Rectangle> AspectRectangles = new();

    public bool CurrentHovered = false;
    public Texture2D BackGroundTexture;
    public SpriteFont Font;

    private Vector2 iconPos;

    public CombatantInfoUI(ICombatant comb)
    {
        combatant = comb;
        BackGroundTexture = AssetManager.GetTexture("fightBackground");
        Font = AssetManager.GetFont("mainFont");
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!SceneManager.IsState(SceneState.Combat) || CombatGuard.CurrentCombat.TurnOrder.Count <= 0) return;
        SetIconPosition();
        DrawCombatantIcon(spriteBatch);
        DrawCombatantHealth(spriteBatch);
        DrawCombatantStats(spriteBatch);
        DrawCombatantAspects(spriteBatch);
        // This stops the highlight from being drawn during location selection
        if (!combatant.MovementController.AllowedToBeDrawn) return;
        DrawHighlightedCell(spriteBatch);
    }
    private void DrawHighlightedCell(SpriteBatch spriteBatch)
    {
        if (CurrentHovered)
            TileManager.GetCell(combatant.MovementController.CurrentPos).DrawCellHighlight(spriteBatch, ColorPalette.Water);
    }
    private void SetIconPosition()
    {
        int totalWidth = CombatGuard.CurrentCombat.TotalCombatants * spacingX;
        int screenWidth = ViewportManager.ScreenWidth;
        iconPos = new Vector2((screenWidth - totalWidth) / 2f + combatant.PositionInOrder * spacingX, topY);
        IconRectangle = new Rectangle((int)iconPos.X, (int)iconPos.Y, iconSize, iconSize);
    }
    private void DrawCombatantIcon(SpriteBatch spriteBatch)
    {
        Color col = combatant.Is switch
        {
            CombatMonsterType.Summoned => Color.White * 0.8f,
            _ => Color.White
        };

        if (combatant.isDead)
            col = Color.Gray * 0.4f;

        if (isCurrentCombatant)
            spriteBatch.Draw(BackGroundTexture, IconRectangle, col);

        spriteBatch.Draw(combatant.Icon, IconRectangle, col);
    }
    private void DrawCombatantHealth(SpriteBatch spriteBatch)
    {
        float currentHealth = MathF.Max(0, combatant.CurrentStats.Health);
        string hpText = $"{currentHealth} / {combatant.BaseStats.Health}";
        Vector2 textSize = Font.MeasureString(hpText);
        Vector2 textPos = new Vector2(IconRectangle.X + (iconSize - textSize.X) / 2, IconRectangle.Bottom + 2);

        HealthTextRectangle = new Rectangle((int)textPos.X, (int)textPos.Y, (int)textSize.X, (int)textSize.Y);
        spriteBatch.DrawString(Font, hpText, textPos, ColorPalette.LightColor);
    }
    private void DrawCombatantStats(SpriteBatch spriteBatch)
    {
        // MP
        string mpText = $"MP: {combatant.CurrentStats.MP} / {combatant.BaseStats.MP}";
        Vector2 mpSize = Font.MeasureString(mpText);
        Vector2 mpPos = new Vector2(IconRectangle.X + (iconSize - mpSize.X) / 2, HealthTextRectangle.Bottom + 2);
        MpTextRectangle = new Rectangle((int)mpPos.X, (int)mpPos.Y, (int)mpSize.X, (int)mpSize.Y);
        spriteBatch.DrawString(Font, mpText, mpPos, Color.Blue);

        // AP
        string apText = $"AP: {combatant.CurrentStats.AP} / {combatant.BaseStats.AP}";
        Vector2 apSize = Font.MeasureString(apText);
        Vector2 apPos = new Vector2(IconRectangle.X + (iconSize - apSize.X) / 2, MpTextRectangle.Bottom + 2);
        ApTextRectangle = new Rectangle((int)apPos.X, (int)apPos.Y, (int)apSize.X, (int)apSize.Y);
        spriteBatch.DrawString(Font, apText, apPos, Color.Orange);
    }
    private void DrawCombatantAspects(SpriteBatch spriteBatch)
    {
        AspectRectangles.Clear();
        int aspectSize = 24;
        int aspectSpacing = 4;

        for (int i = 0; i < combatant.Aspects.Count; i++)
        {
            var aspect = combatant.Aspects[i];
            Vector2 aspectPos = new Vector2(
                IconRectangle.X + i * (aspectSize + aspectSpacing),
                IconRectangle.Bottom + 25
            );
            Rectangle aspectRect = new Rectangle((int)aspectPos.X, (int)aspectPos.Y, aspectSize, aspectSize);
            spriteBatch.Draw(aspect.Icon, aspectRect, Color.White);

            string turnsLeft = MathF.Ceiling(aspect.Duration).ToString();
            Vector2 numSize = Font.MeasureString(turnsLeft);
            Vector2 numPos = new Vector2(
                aspectRect.Center.X - numSize.X / 2,
                aspectRect.Center.Y - numSize.Y / 2
            );
            spriteBatch.DrawString(Font, turnsLeft, numPos, Color.Yellow);

            AspectRectangles.Add(aspectRect);
        }
    }
    public void ToggleMouseHover(Vector2 mouse)
    {
        if (IconRectangle.Contains(mouse))
        {
            CurrentHovered = true;
        }
        else CurrentHovered = false;

    }
}
