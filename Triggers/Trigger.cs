using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.AnimationFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Tiles;
using PlayingAround.Triggers;
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

        public List<TriggerNodes> TriggerNodes {  get; set; } = new List<TriggerNodes>();

    }
    public class TriggerNodes
    {
        public List<Condition> Conditions { get; set; } = new();
        public List<Outcome> Outcomes { get; set; } = new();
        public int MaxNodesAccepted { get; set; }

    }
}
public static class TriggerFactory
{
    public static Trigger SingleNode(params (Condition[] conds, Outcome[] outs)[] nodes)
    {
        var t = new Trigger();
        foreach (var (conds, outs) in nodes)
        {
            var n = new TriggerNodes();
            n.Conditions.AddRange(conds);
            n.Outcomes.AddRange(outs);
            t.TriggerNodes.Add(n);
        }
        return t;
    }
}
