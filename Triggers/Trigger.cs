using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Tiles;
using PlayingAround.Triggers.ConditionFolder;
using PlayingAround.Triggers.EffectFolder;
using PlayingAround.Triggers.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers
{
    public class Trigger
    {

        public List<TriggerNodes> TriggerNodes {  get; set; }

    }
    public class TriggerNodes
    {
        public List<Condition> Conditions { get; set; } = new();
        public List<Outcome> Outcomes { get; set; } = new();

    }
    public class CombatTrigger : Trigger
    {
        public PlayMonsters Mon { get; set; }
        public CombatTrigger(PlayMonsters mon)
        {
            Mon = mon;
            var node = new TriggerNodes();
            var engageFight = new Condition
            {
                Type = ConditionType.KeyPressed,
                Key = Keys.E,
                PlayMonster = mon,
            };

            var startCombat = new Outcome
            {
                Type = OutcomeType.StartCombat
            };

            node.Conditions.Add(engageFight);
            node.Outcomes.Add(startCombat);

            TriggerNodes = new List<TriggerNodes> { node };
        }
        
    }
    public class ProximityTrigger : Trigger
    {

        public ProximityTrigger(IProximityTracked obj, PlayMonsters mon)
        {
            var node = new TriggerNodes();

            var proxCondition = new Condition
            {
                Type = ConditionType.Proximity,
                AnchorPoint = obj

            };

            var allowedToFight = new Condition
            {
                Type = ConditionType.AllowedToFight,
                PlayMonster = mon
            };

            var notify = new Outcome
            {
                Type = OutcomeType.NotificationText,
                NotificationTextBox = new CombatNotificationTextBox(obj),
            };

            node.Conditions.Add(proxCondition);
            node.Conditions.Add(allowedToFight);
            node.Outcomes.Add(notify);

            TriggerNodes = new List<TriggerNodes> { node };
        }
    }
}