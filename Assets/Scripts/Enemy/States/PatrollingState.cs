namespace Poligon.Ai.EnemyStates {
    public class PatrollingState : State<AiState> {
        public override AiState state { get; protected set; } = AiState.Patrolling;
    }
}