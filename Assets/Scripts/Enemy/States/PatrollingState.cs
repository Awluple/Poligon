namespace Poligon.Ai.EnemyStates {
    public class PatrollingState : EnemyBaseState {
        public PatrollingState(EnemyController controller) : base(controller) {
        }

        public override AiState state { get; protected set; } = AiState.Patrolling;
    }
}