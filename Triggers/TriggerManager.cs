using Microsoft.Xna.Framework;
using PlayingAround.Managers.Proximity;
using System;
using System.Collections.Generic;

namespace PlayingAround.Triggers
{
    public class TriggerManager
    {
        private static List<Trigger> _trackedTriggers = new List<Trigger>();
        public static void Initialize()
        {
            ProximityManager.OnPlayerNearTrigger += HandleTriggerInteract;
            ProximityManager.OnPlayerLeaveTrigger += HandleExitTriggerTile;
        }

        public static void Update(float delta)
        {
            if (_trackedTriggers.Count == 0) return;


        }

        private static void HandleTriggerInteract(Trigger trigger)
        {
            _trackedTriggers.Add(trigger);
        }

        private static void HandleExitTriggerTile(Trigger trigger)
        {
            _trackedTriggers.Remove(trigger);
        }

    }
}
