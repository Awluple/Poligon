namespace Poligon.Ai.EnemyStates {
    public class BehindCoverState : State<AiState> {
        public override AiState state { get; protected set; } = AiState.BehindCover;
    }
}