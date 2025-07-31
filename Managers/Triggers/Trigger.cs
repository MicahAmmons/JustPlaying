using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Triggers
{
    public class Trigger
    {
        public List<TriggerStages> Stages { get; set; }
    }
    public class TriggerStages
    {
        public string Id {  get; set; }
        public List<TriggerCondition> Condition { get; set; }
        public List<TriggerEffect> Effect { get; set; }
    }
    public class TriggerCondition
    {
        public TriggerConditionType Type { get; set; }
        public string? ItemName {  get; set; }
    }
    public class TriggerEffect
    {
        public TriggerEffectType Type { get; set; }
        public string? Text { get; set; }
    }
}
public enum TriggerConditionType
{
    ItemNotHeld,
    ItemHeld
}
public enum TriggerEffectType
{
    NotificationText
}
