using PlayingAround.Managers.Dialogue;
using PlayingAround.Managers.Proximity;
using PlayingAround.Managers.Quests;
using PlayingAround.Triggers;
using PlayingAround.Triggers.Proximity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayingAround.Triggers.TriggerManager;

namespace PlayingAround.Triggers.ConditionFolder
{
    public class ConditionEvaluator
    {
        private ProximityEvaluator _proximityEvaluator = new ProximityEvaluator();
        public void CheckConditions(Trigger trigger, in EvalContext ctx, List<FiredNode> firedOut)
        {
            var nodes = trigger.TriggerNodes;
            if (nodes == null || nodes.Count == 0) return;

            for (int n = 0; n < nodes.Count; n++)
            {
                var node = nodes[n];
                if (ConditionsAreMet(node.Conditions, trigger, ctx))
                    firedOut.Add(new FiredNode(trigger, n));
            }
        }
        public bool ConditionsAreMet(List<Condition> conditions, Trigger trigger, in EvalContext ctx)
        {
            if (conditions == null || conditions.Count == 0) return true;
            foreach (var c in conditions)
                if (!Evaluate(c, trigger, ctx)) return false; 
            return true;
        }
        private bool Evaluate(Condition cond, Trigger trig, in EvalContext ctx)
        {
            switch (cond.Type)
            {
                case ConditionType.QuestStage:
                    return QuestManager.GetStage(cond.QuestId) == cond.QuestStage;

                case ConditionType.ObjectiveProgress:
                    return QuestManager.ObjectiveProgressIs(cond.QuestId, cond.ObjectiveId, cond.ProgressionStateId);
                case ConditionType.Proximity:
                    return _proximityEvaluator.WithinRange(cond, ctx);
                // add KeyPressed / Proximity, etc., later
                default:
                    return false;
            }
        }
    }
}