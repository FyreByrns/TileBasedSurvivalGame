namespace TileBasedSurvivalGame.World {
    public interface IPositioner<T> {
        Vector2 GetPosition(T positioned);
    }
}
