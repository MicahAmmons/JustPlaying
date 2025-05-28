public class CombatStateMachine
{
    // Backing fields
    private PlayerTurnState _currentPlayerTurnState = PlayerTurnState.None;
    private SummonedTurnState _currentSummonedTurnState = SummonedTurnState.None;
    private CombatState _currentCombatState = CombatState.LocationSelection;
    private AITurnState _currentAITurnState = AITurnState.None;

    // Public accessors
    public PlayerTurnState CurrentPlayerTurnState => _currentPlayerTurnState;
    public SummonedTurnState CurrentSummonedTurnState => _currentSummonedTurnState;
    public CombatState CurrentCombatState => _currentCombatState;
    public AITurnState CurrentAITurnState => _currentAITurnState;

    // Public setters
    public void SetPlayerTurnState(PlayerTurnState newState) => _currentPlayerTurnState = newState;
    public void SetSummonedTurnState(SummonedTurnState newState) => _currentSummonedTurnState = newState;
    public void SetCombatState(CombatState newState) => _currentCombatState = newState;
    public void SetAITurnState(AITurnState newState) => _currentAITurnState = newState;

    // Enums
    public enum PlayerTurnState
    {
        None,
        PlayerChoosingSummoned,
        PlayerClickedMoveButton,
        PlayerExecutingMove,

        PlayerClickedAttacking,
        PlayerWaitingInput,
        PlayerExecutingAction,


        PlayerSummoning,
        PlayerExecutingSummoning,
        PlayerAttacking,
        PlayerTargeting,
        PlayerExecutingAttack,
        PlayerEndingTurn,
    }

    public enum SummonedTurnState
    {
        None,
        SummonedWaitingInput,
        SummonClickedAttack,
        SummonTargetingAttack,
        SummonExecutingAttack,
        SummonMoving,
        SummonAttacking,
        SummonEndingTurn
    }

    public enum CombatState
    {
        None,
        LocationSelection,
        TurnStart,
        PlayerTurn,
        AITurn,
        SummonedTurn,
        Debug,
        ResolvingStartOfTurnEffects,
        ResolvingEndOfTurnEffects,
        EndingTurn,
        CombatOver
    }

    public enum AITurnState
    {
        None,
        ActionNavigation,
        MovingAIControlled,
        ExecutingMove,
        AIAttacking,
        ExecutingAttack,


    }
}
