using Microsoft.Xna.Framework.Graphics;
using PlayingAround.ButtonsFolder;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.ActFolder
{
    public class ActController
    {
        public List<Act> ActsOrder {  get; set; } = new List<Act>();
        public ActController(ActData data)
        {
            foreach (var act in data.ActionOrder)
            {
                switch (act.Type)
                {
                    case ActType.Attack:
                        ActsOrder.Add(new AttackAct(act));
                    break;

                    case ActType.Move:
                        ActsOrder.Add(new MoveAct(act));
                        break;
                }
            }
        }
        public void DrawButtons(SpriteBatch sb)
        {
            foreach (var act in ActsOrder)
            {
                act.ActButton.Draw(sb); 
            }
        }
    }
    public class Act
    {
        public ActionTarget Target { get; set; }
        public Button ActButton { get; set; }
    }
    public class AttackAct : Act
    {
        public SingleAttack Attack {  get; set; } 
        public AttackAct(SpecificActData data)
        {
            Attack = AttackManager.GetAttack(data.AttackName, data.ElementType);
            Target = data.ActionTarget;
        }

    }
    public class MoveAct : Act
    {
        public MovementAmount MovementAmount { get; set; }

        public MoveAct(SpecificActData data)
        {
            MovementAmount = data.MovementAmount;
            Target = data.ActionTarget;
        }
    }
   
}
