using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Game.Map;
using PlayingAround.Managers.Assets;
using PlayingAround.Managers.CombatMan.CombatAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CombatStateMachine;

namespace PlayingAround.Managers.CombatMan
{
    public class CombatUIManager
    {
        private CombatStateMachine _stateMachine;
        private readonly Queue<CombatMonster> _allCombatants;
        private readonly Func<List<TileCell>> _getMoveableCells;
        private readonly List<CombatMonster> _referenceList;
        public PlayerTurnState StatePlayerTurn => _stateMachine.CurrentPlayerTurnState;
        public CombatState StateCombat => _stateMachine.CurrentCombatState;
        public SummonedTurnState StateSummonedTurn => _stateMachine.CurrentSummonedTurnState;
        public AITurnState StateAITurn => _stateMachine.CurrentAITurnState;




        public CombatUIManager(CombatStateMachine stateMachine, Queue<CombatMonster> allCombatants, List<CombatMonster> referenceList)
        {
            _stateMachine = stateMachine;
            _allCombatants = allCombatants;
            _referenceList = referenceList;
            

        }
        public void Update()
        {
        }
        public void Draw(SpriteBatch spriteBatch)
        {



        }



      




    }
}
