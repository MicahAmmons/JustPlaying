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
        PlayerClickedSpecificSummoned,
        PlayerClickedMoveButton,
        PlayerExecutingMove,

        PlayerClickedAttacking,
        PlayerWaitingInput,
        PlayerExecutingAction,


        PlayerClickedSummonButton,
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
        SummonedClickedAttackButton,
        SummonedClickedMoveButton,
        SummonedClickedSpecificAttackButton,
        SummonedChoosingTarget,
        SummonedExecutingAttack,
        SummonedExecutingMove,
        SummonedEndingTurn,
        SummonedTopOfAction
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
        WinnerChosen,
    }

    public enum AITurnState
    {
        None,
        ActionNavigation,
        ExecutingMove,
        AIAttacking,
        ExecutingAttack,
        EndOfActionPause

    }
   
}
