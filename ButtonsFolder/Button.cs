using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Manager;
using PlayingAround.Managers.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CombatStateMachine;
using System.Security;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework.Input;
using static PlayingAround.ButtonsFolder.CombatButtonManager;

namespace PlayingAround.ButtonsFolder
{
    public class CombatButtonManager
    {
        private List<Button> _buttons = new List<Button>() { };
        public MoveButton MoveButton { get; }
        public AttackButton AttackButton { get; }
        public EndTurnButton EndTurnButton { get; }
        public SummonButton SummonButton { get; }

        public void UpdateInput()
        {
            var mousePoint = new Point(InputManager.MouseX, InputManager.MouseY);
            bool leftPressedThisFrame = InputManager.IsLeftClick();
            foreach (var b in _buttons) b.ResetInputState();
            foreach (var b in _buttons)
            {
                if (!b.AllowedToInputTrack) continue;

                b.UpdateInput(mousePoint, leftPressedThisFrame);

                // If you want exclusive input (topmost wins), break on first hit
                if (b.MouseHovered && leftPressedThisFrame)
                    break;
            }
        }
        public void ResetAllFlags()
        {
            foreach (var b in _buttons)
            {
                b.ResetPermissions();
            }
        }
        public void ApplyPermissions(CombatState state)
        {
            foreach (var b in _buttons)
            {
                b.ResetPermissions();
                if (b.TryGetPermission(state, out var p))
                {
                    b.AllowedToBeDrawn = p.Draw;
                    b.AllowedToInputTrack = p.Input;
                }
            }
        }
        public CombatButtonManager()
        {
            MoveButton = new MoveButton();
            AttackButton = new AttackButton();
            EndTurnButton = new EndTurnButton();
            SummonButton = new SummonButton();

            _buttons.AddRange(new Button[]
            {
                    MoveButton,
                    AttackButton,
                    EndTurnButton,
                    SummonButton
            });
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var b in _buttons)
            {
                if (!b.AllowedToBeDrawn) continue;
                b.Draw(spriteBatch);
            }
        } }
        public class MoveButton : Button
        {
            private readonly Dictionary<CombatState, ButtonPermission> _permissions;

            public MoveButton()
            {
                DrawRectangle = new Rectangle(50, 700, 200, 100);
                _permissions = new Dictionary<CombatState, ButtonPermission>
                {
                    [CombatState.ActionNavigation] = new ButtonPermission(true, false),
                    [CombatState.ExecutingMove] = new ButtonPermission(true, false),
                    [CombatState.ExecutingAttack] = new ButtonPermission(true, false),
                    [CombatState.ExecutingSummon] = new ButtonPermission(true, false),
                    [CombatState.ResolvingEndOfTurnEffects] = new ButtonPermission(true, false),
                    [CombatState.ResolvingStartOfTurnEffects] = new ButtonPermission(true, false),
                    [CombatState.EndingTurn] = new ButtonPermission(true, false),
                    [CombatState.WinnerChosen] = new ButtonPermission(false, false),
                    [CombatState.TopOfAction] = new ButtonPermission(true, false),
                    [CombatState.WaitingPlayerInput] = new ButtonPermission(true, true),
                    [CombatState.LocationSelection] = new ButtonPermission(true, false),
                    [CombatState.TurnStart] = new ButtonPermission(true, false),
                };
            }
            public override bool TryGetPermission(CombatState state, out ButtonPermission perm)
        => _permissions.TryGetValue(state, out perm);
        }
        public class AttackButton : Button
    {
        private readonly Dictionary<CombatState, ButtonPermission> _permissions;
        public AttackButton()
        {
            DrawRectangle = new Rectangle(50, 800, 200, 100);
            _permissions = new Dictionary<CombatState, ButtonPermission>
            {
                [CombatState.ActionNavigation] = new ButtonPermission(true, false),
                [CombatState.ExecutingMove] = new ButtonPermission(true, false),
                [CombatState.ExecutingAttack] = new ButtonPermission(true, false),
                [CombatState.ExecutingSummon] = new ButtonPermission(true, false),
                [CombatState.ResolvingEndOfTurnEffects] = new ButtonPermission(true, false),
                [CombatState.ResolvingStartOfTurnEffects] = new ButtonPermission(true, false),
                [CombatState.EndingTurn] = new ButtonPermission(true, false),
                [CombatState.WinnerChosen] = new ButtonPermission(false, false),
                [CombatState.TopOfAction] = new ButtonPermission(true, false),
                [CombatState.WaitingPlayerInput] = new ButtonPermission(true, true),
                [CombatState.LocationSelection] = new ButtonPermission(true, false),
                [CombatState.TurnStart] = new ButtonPermission(true, false),
            };
        }
        public override bool TryGetPermission(CombatState state, out ButtonPermission perm)
    => _permissions.TryGetValue(state, out perm);
    }
        public class EndTurnButton : Button
    {
        private readonly Dictionary<CombatState, ButtonPermission> _permissions;
        public EndTurnButton()
        {
            DrawRectangle = new Rectangle(50, 900, 200, 100);
            _permissions = new Dictionary<CombatState, ButtonPermission>
            {
                [CombatState.ActionNavigation] = new ButtonPermission(true, false),
                [CombatState.ExecutingMove] = new ButtonPermission(true, false),
                [CombatState.ExecutingAttack] = new ButtonPermission(true, false),
                [CombatState.ExecutingSummon] = new ButtonPermission(true, false),
                [CombatState.ResolvingEndOfTurnEffects] = new ButtonPermission(true, false),
                [CombatState.ResolvingStartOfTurnEffects] = new ButtonPermission(true, false),
                [CombatState.EndingTurn] = new ButtonPermission(true, false),
                [CombatState.WinnerChosen] = new ButtonPermission(false, false),
                [CombatState.TopOfAction] = new ButtonPermission(true, false),
                [CombatState.WaitingPlayerInput] = new ButtonPermission(true, true),
                [CombatState.LocationSelection] = new ButtonPermission(true, false),
                [CombatState.TurnStart] = new ButtonPermission(true, false),
            };
        }
        public override bool TryGetPermission(CombatState state, out ButtonPermission perm)
    => _permissions.TryGetValue(state, out perm);
    }
        public class SummonButton : Button
    {
        private readonly Dictionary<CombatState, ButtonPermission> _permissions;
        public SummonButton()
        {
            DrawRectangle = new Rectangle(50, 900, 200, 100);
            _permissions = new Dictionary<CombatState, ButtonPermission>
            {
                [CombatState.ActionNavigation] = new ButtonPermission(true, false),
                [CombatState.ExecutingMove] = new ButtonPermission(true, false),
                [CombatState.ExecutingAttack] = new ButtonPermission(true, false),
                [CombatState.ExecutingSummon] = new ButtonPermission(true, false),
                [CombatState.ResolvingEndOfTurnEffects] = new ButtonPermission(true, false),
                [CombatState.ResolvingStartOfTurnEffects] = new ButtonPermission(true, false),
                [CombatState.EndingTurn] = new ButtonPermission(true, false),
                [CombatState.WinnerChosen] = new ButtonPermission(false, false),
                [CombatState.TopOfAction] = new ButtonPermission(true, false),
                [CombatState.WaitingPlayerInput] = new ButtonPermission(true, true),
                [CombatState.LocationSelection] = new ButtonPermission(true, false),
                [CombatState.TurnStart] = new ButtonPermission(true, false),
            };
        }
        public override bool TryGetPermission(CombatState state, out ButtonPermission perm)
    => _permissions.TryGetValue(state, out perm);
    }
        public struct ButtonPermission
    {
        public bool Draw;
        public bool Input;
        public ButtonPermission(bool draw, bool input) { Draw = draw; Input = input; }
    }
        public class Button
    {

        public bool AllowedToBeDrawn = false;
        public bool AllowedToInputTrack = false;
        public Texture2D Texture = AssetManager.GetTexture("fightBackground");
        public Rectangle DrawRectangle;
        public bool MouseHovered = false;
        public bool MouseClicked = false;
        public event Action Clicked;

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DrawRectangle, Color.White);
        }
        public void UpdateInput(Point mousePoint, bool leftPressedThisFrame)
        {
            MouseHovered = DrawRectangle.Contains(mousePoint);
            if (MouseHovered && leftPressedThisFrame) Clicked?.Invoke();
        }
        public virtual bool TryGetPermission(CombatState state, out ButtonPermission perm)
        {
            perm = default;
            return false;
        }
        public void ResetInputState()
        {
            MouseHovered = false;
            MouseClicked = false;
        }
        public void ResetPermissions()
        {
            AllowedToBeDrawn = false;
            AllowedToInputTrack = false;
        }

    }