namespace TileBasedSurvivalGame.World {
    public interface IPositioner<T> {
        PixelEngine.Vector2 GetPosition(T positioned);
    }
}
