namespace Poligon.Ai.EnemyStates {
    public class HidingState : State<AiState> {
        public override AiState state { get; protected set; } = AiState.Hiding;
    }
}