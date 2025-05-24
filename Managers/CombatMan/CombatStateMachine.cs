public class CombatStateMachine
{
    // Backing fields
    private PlayerTurnState _currentPlayerTurnState = PlayerTurnState.None;
    private SummonedTurnState _currentSummonedTurnState = SummonedTurnState.None;
    private CombatState _currentCombatState = CombatState.Waiting;
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
        PlayerWaitingInput,
        PlayerExecutingAction,
        PlayerExecutingMove,
        PlayerMoving,
        PlayerSummoning,
        PlayerAttacking,
        PlayerTargeting,
        PlayerExecutingAttack,
        PlayerEndingTurn,
    }

    public enum SummonedTurnState
    {
        None,
        SummonWaitingInput,
        SummonMoving,
        SummonAttacking,
        SummonEndingTurn
    }

    public enum CombatState
    {
        Waiting,
        LocationSelection,
        TurnStart,
        ActionNavigation,
        MovingPlayerControlled,
        MovingAIControlled,
        AIAttacking,
        ExecutingAttack,
        ExecutingMove,
        AwaitingPlayerInput,
        PlayerTurn,
        ResolvingStartOfTurnEffects,
        ResolvingEndOfTurnEffects,
        ResolvingEffects,
        EndingTurn,
        CombatOver
    }

    public enum AITurnState
    {
        None,

    }
}
