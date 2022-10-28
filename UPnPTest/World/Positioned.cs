namespace TileBasedSurvivalGame.World {
    public class Positioned {
        public Vector2 Position { get; set; }
        public object Value { get; set; }

        public bool Is<T>() {
            return typeof(T) == Value.GetType();
        }
        public Positioned<T> As<T>() {
            if(Is<T>()) {
                return (Positioned<T>)this;
            }
            return null;
        }
        public T ValueAs<T>() {
            if (Is<T>()){
                return As<T>().GetValue();
            }
            return default(T);
        }
    }

    public class Positioned<T> : Positioned {
        public T GetValue() { return (T)Value; }
        public void SetValue(T value) { Value = value; }

        public Positioned(T value, Vector2 position) {
            Value = value;
            Position = position;
        }
    }
}
