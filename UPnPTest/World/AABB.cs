using System;

namespace TileBasedSurvivalGame.World {
    class AABB {
        public delegate void ResizeEventHandler(AABB sender, Vector2 newTopLeft, Vector2 newBottomRight);
        public event ResizeEventHandler Resize;

        Vector2 _topLeft, _bottomRight;
        public Vector2 TopLeft {
            get => _topLeft;
            set {
                _topLeft = value;
                OnResize();
            }
        }
        public Vector2 BottomRight {
            get => _bottomRight;
            set {
                _bottomRight = value;
                OnResize();
            }
        }

        public float Width => BottomRight.x - TopLeft.x;
        public float Height => BottomRight.y - TopLeft.y;

        // make sure top left is above and to the left of bottom right
        void EnsureOrder() {
            _topLeft.x = Math.Min(_topLeft.x, _bottomRight.x);
            _topLeft.y = Math.Min(_topLeft.y, _bottomRight.y);
            _bottomRight.x = Math.Max(_topLeft.x, _bottomRight.x);
            _bottomRight.y = Math.Max(_topLeft.y, _bottomRight.y);
        }
        void OnResize() {
            EnsureOrder();
            Resize?.Invoke(this, TopLeft, BottomRight);
        }

        public void ResizeToFit(Vector2 point) {
            Vector2 newTopLeft = (Math.Min(point.x, TopLeft.x), Math.Min(point.y, TopLeft.y));
            Vector2 newBottomRight = (Math.Max(point.x, BottomRight.x), Math.Max(point.y, BottomRight.y));

            _topLeft = newTopLeft; // raw member set to only emit Resize event once
            BottomRight = newBottomRight;
        }

        public bool PointWithin(Vector2 point) {
            return point.x >= TopLeft.x && point.y >= TopLeft.y
                && point.x <= BottomRight.x && point.y <= BottomRight.y;
        }

        public bool Intersects(AABB other) {
            return other.TopLeft.x <= BottomRight.x && other.TopLeft.y <= BottomRight.y
                && other.BottomRight.x >= TopLeft.x && other.BottomRight.y >= TopLeft.y;
        }

        public AABB(Vector2 topLeft, Vector2 bottomRight) {
            _topLeft = topLeft;
            BottomRight = bottomRight;
        }
    }
}
