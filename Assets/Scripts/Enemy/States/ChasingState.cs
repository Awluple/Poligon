namespace Poligon.Ai.EnemyStates {
    public class ChasingState : State<AiState> {
        public override AiState state { get; protected set; } = AiState.Chasing;
    }
}