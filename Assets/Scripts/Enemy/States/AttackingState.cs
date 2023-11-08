namespace Poligon.Ai.EnemyStates {
    public class AttackingState : State<AiState> {
        public override AiState state { get; protected set; } = AiState.Attacking;
    }
}