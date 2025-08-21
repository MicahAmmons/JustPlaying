public class CombatStateMachine
{
    private CombatState _currentCombatState = CombatState.LocationSelection;

    public CombatState CurrentCombatState => _currentCombatState;

    public void SetCombatState(CombatState newState) => _currentCombatState = newState;


  
    public enum CombatState
    {
        None,
        ActionNavigation,
        ExecutingMove,
        ExecutingAttack,
        ExecutingSummon,
        ResolvingStartOfTurnEffects,
        ResolvingEndOfTurnEffects,
        EndingTurn,
        WinnerChosen,

        TopOfAction,
        WaitingPlayerInput,
        LocationSelection,
        TurnStart,
    }

   
}
