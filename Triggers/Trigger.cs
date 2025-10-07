using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.AnimationFolder;
using PlayingAround.ConditionsAndEffects.ConditionFolder;
using PlayingAround.ConditionsAndEffects.EffectFolder;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Interfaces;
using PlayingAround.Managers.Tiles;
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
        public List<List<Condition>> Conditions { get; set; } = new();
        public List<List<Outcome>> Outcomes { get; set; } = new();
    }
}