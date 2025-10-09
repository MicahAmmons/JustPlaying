using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.Tiles;
using PlayingAround.Triggers.ConditionFolder;
using PlayingAround.Triggers.EffectFolder;
using PlayingAround.Triggers.Notifications;
using System;
using System.Collections.Generic;


namespace PlayingAround.Triggers
{
    public class TriggerManager
    {
        private static readonly List<Trigger> _tracked = new();
        private static readonly List<FiredNode> _fired = new();

        private static Vector2 _playerPos => PlayerManager.CurrentPlayer.MovementController.CurrentPos;
        private static ConditionEvaluator _conditionEvaluator = new ConditionEvaluator();
        private static OutcomeExecutor _outcomeExecutor = new OutcomeExecutor();
        // add other evaluators…

        // Scratch partitions (reused each frame)
        private static readonly List<Trigger> _proxPartition = new();
        private static readonly List<Trigger> _nextTilePartition = new();
        public static void Initialize()
        {
            TileManager.OnMapTileChange += RebuildForCurrentTile;
            PlayMonsterManager.PlayMonsterRemoved += RebuildForCurrentTile;
        }
        public static void RebuildForCurrentTile()
        {
            _tracked.Clear();
            _fired.Clear();
            foreach (var mon in TileManager.CurrentMapTile.PlayMonstersList)
            {
                foreach (var trigger in mon.Triggers)
                {
                    _tracked.Add(trigger);
                }
            }
            foreach (var kvp in TileManager.CurrentMapTile.TriggerCells)
            {
                TileCell cell = kvp.Key;
                Trigger trig = kvp.Value;
                _tracked.Add(trig);
            }
        }

        public static void Update(float delta)
        {
            if (!SceneManager.IsState(SceneState.Play)) return;
            if (_tracked.Count == 0) return;
            var ctx = new EvalContext(
                       playerPos: _playerPos,
                       delta: delta
        );
            _fired.Clear();
            foreach (var trigger in _tracked)
                _conditionEvaluator.CheckConditions(trigger, ctx, _fired);

            for (int i = 0; i < _fired.Count; i++)
                _outcomeExecutor.Execute(_fired[i]);
        }
        public static List<Trigger> GenerateCombatTriggers(PlayMonsters mon)
        {
            List<Trigger> combatTriggers = new List<Trigger>();

            Trigger proxTrig = TriggerFactory.SingleNode((
        new[]
        {
            new Condition { Type = ConditionType.Proximity, AnchorPoint = mon },
            new Condition { Type = ConditionType.AllowedToFight, PlayMonster = mon }
        },
        new[]
        {
            new Outcome
            {
                Type = OutcomeType.NotificationText,
                NotificationTextBox = new CombatNotificationTextBox(mon)
            }
        }
    )
);
            Trigger combatStartTrig = TriggerFactory.SingleNode((new[]
            {
            new Condition { Type = ConditionType.KeyPressed, Key = Keys.E },
            new Condition { Type = ConditionType.Proximity, AnchorPoint= mon },
            new Condition { Type = ConditionType.AllowedToFight, PlayMonster = mon }

            }, 
            new[]
            {
            new Outcome { Type = OutcomeType.StartCombat, PlayMonster = mon }
            }
        )
    );

            combatTriggers.Add(proxTrig);
            combatTriggers.Add(combatStartTrig);
            return combatTriggers;
        }
        public readonly struct FiredNode
        {
            public readonly Trigger Trigger;
            public readonly int NodeIndex;
            public FiredNode(Trigger trigger, int nodeIndex)
            { 
                Trigger = trigger; 
                NodeIndex = nodeIndex; 
            }
        }
        public readonly struct EvalContext
        {
            public readonly Vector2 PlayerPos;
            public readonly float Delta;

            public EvalContext(Vector2 playerPos, float delta)
            {
                PlayerPos = playerPos;
                Delta = delta;
            }
        }
    }

}
