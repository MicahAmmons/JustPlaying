using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using PlayingAround.Game.Map;
using PlayingAround.Managers;
using PlayingAround.Managers.Entities;
using PlayingAround.Managers.NPCHouse;
using PlayingAround.Managers.Tiles;
using PlayingAround.Triggers;
using PlayingAround.Triggers.ConditionFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Triggers.Proximity
{
    public class ProximityEvaluator
    {
        internal bool WithinRange(Condition con, TriggerManager.EvalContext ctx)
        {
            if (con == null) return false;
            TileCell playerCell = TileManager.GetCell(ctx.PlayerPos);
            TileCell conCell = TileManager.GetCell(con.AnchorPoint.ProximityTrackingPoint);
            return playerCell == conCell;
            Vector2 anchor = con.AnchorPoint.ProximityTrackingPoint; 

            //float distSq = Vector2.DistanceSquared(ctx.PlayerFeet, anchor);
            //float rangeSq = con.ProximityDistance * con.ProximityDistance;

            //return distSq <= rangeSq;
        }
    }

}
