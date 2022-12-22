using System;
using System.Collections.Generic;
using Vector2 = PixelEngine.Vector2;

namespace TileBasedSurvivalGame.World {
    /// <summary>
    /// Quadtree of any 2D positionable item.
    /// </summary>
    /// <typeparam name="T">The positionable item.</typeparam>
    /// <typeparam name="TPositioner">An interface to get the positions of the positionable items.</typeparam>
    class QuadTree<T, TPositioner>
        where TPositioner : IPositioner<T> {
        public AABB Bounds { get; set; }
        public IPositioner<T> Positioner { get; }

        // how many items are allowed in the leaf before splitting
        public int SplitThreshold { get; set; } = 4;
        public List<T> Contents { get; set; } = new List<T>();
        // only leaves may have contents
        public bool Leaf => Contents != null;

        public QuadTree<T, TPositioner> TopLeft { get; set; }
        public QuadTree<T, TPositioner> TopRight { get; set; }
        public QuadTree<T, TPositioner> BottomLeft { get; set; }
        public QuadTree<T, TPositioner> BottomRight { get; set; }
        public QuadTree<T, TPositioner>[] Children => new[] { TopLeft, TopRight, BottomLeft, BottomRight };
        void CreateChildren() {
            float halfWidth = Bounds.Width / 2;
            float halfHeight = Bounds.Height / 2;

            // TopLeft is 1ACB
            // TopRight is A2DC
            // BottomLeft is BCE3
            // BottomRight is CD4E
            // 1---A---2
            // |       |
            // B   C   D 
            // |       |
            // 3---E---4
            Vector2 topMid = Bounds.TopLeft + (halfWidth, 0);               //A
            Vector2 leftMid = Bounds.TopLeft + (0, halfHeight);             //B
            Vector2 middle = Bounds.TopLeft + (halfWidth, halfHeight);      //C
            Vector2 rightMid = Bounds.TopLeft + (Bounds.Width, halfHeight); //D
            Vector2 bottomMid = topMid + (0, Bounds.Height);                //E

            TopLeft = new QuadTree<T, TPositioner>(new AABB(Bounds.TopLeft, middle));
            TopRight = new QuadTree<T, TPositioner>(new AABB(topMid, rightMid));
            BottomLeft = new QuadTree<T, TPositioner>(new AABB(leftMid, bottomMid));
            BottomRight = new QuadTree<T, TPositioner>(new AABB(middle, Bounds.BottomRight));
        }
        void MoveContentsToChildren() {
            // copy contents
            IEnumerable<T> contents = Contents.ToArray();
            // no more contents, this tree is a branch now
            Contents = null;
            foreach (T item in contents) {
                Add(item);
            }
        }

        public bool Contains(T item, out QuadTree<T, TPositioner> container) {
            if (Leaf) {
                container = this;
                return Contents.Contains(item);
            }

            foreach (QuadTree<T, TPositioner> child in Children) {
                if (child.Contains(item, out QuadTree<T, TPositioner> _container)) {
                    container = _container;
                    return true;
                }
            }

            container = null;
            return false;
        }
        public IEnumerable<T> AllContents() {
            // if this tree is a leaf ..
            if (Leaf) {
                // .. just return all contents
                foreach (T item in Contents.ToArray()) {
                    yield return item;
                }
            }
            // otherwise ..
            else {
                // recursively find leaves and return contents
                foreach (QuadTree<T, TPositioner> child in Children) {
                    if (child != null) {
                        foreach (T item in child.AllContents()) {
                            yield return item;
                        }
                    }
                }
            }
        }
        public IEnumerable<QuadTree<T, TPositioner>> AllChildren() {
            yield return this;
            foreach (QuadTree<T, TPositioner> child in Children) {
                if (child != null) {
                    yield return child;
                    foreach (QuadTree<T, TPositioner> childChild in child.AllChildren()) {
                        yield return childChild;
                    }
                }
            }
        }

        public bool Add(T item, bool resizeToFitItem = false) {
            // if the item is outside current bounds ..
            if (!Bounds.PointWithin(Positioner.GetPosition(item))) {
                // .. and resizing is allowed,
                if (resizeToFitItem) {
                    // .. resize
                    Bounds.ResizeToFit(Positioner.GetPosition(item));
                }
                // .. otherwise, fail
                else {
                    return false;
                }
            }

            bool success = false;

            // if this tree is a leaf, simply add to contents
            if (Leaf) {
                // lock, in case I do stupid multithreading things
                lock (Contents) {
                    Contents.Add(item);
                }
                success = true;
            }
            // otherwise, find which child to add to
            else {
                foreach (QuadTree<T, TPositioner> child in Children) {
                    // if the item is within the bounds of child ..
                    if (child.Bounds.PointWithin(Positioner.GetPosition(item))) {
                        // .. add it to child
                        success = child.Add(item);
                        break;
                    }
                }
            }

            // if this tree was a leaf and is now too stuffed, split
            if (Leaf && Contents.Count > SplitThreshold) {
                CreateChildren();
                MoveContentsToChildren();
            }

            return success;
        }
        public bool Remove(T item) {
            // if this tree contains the item ..
            if (Contains(item, out QuadTree<T, TPositioner> container)) {
                // .. and if this tree is where the item is ..
                if (container == this) {
                    // .. remove the item
                    lock (Contents) {
                        Contents.Remove(item);
                    }
                    return true;
                }

                // .. otherwise remove from the tree that does contain it
                return container.Remove(item);
            }
            return false;
        }

        public IEnumerable<T> GetWithinRect(Vector2 topLeft, Vector2 bottomRight) {
            AABB rect = new AABB(topLeft, bottomRight);

            if (rect.Intersects(Bounds)) {
                if (Leaf) {
                    foreach (T item in Contents) {
                        if (rect.PointWithin(Positioner.GetPosition(item))) {
                            yield return item;
                        }
                    }
                }
                else {
                    foreach (QuadTree<T, TPositioner> child in Children) {
                        if (child != null) {
                            foreach (T item in child.GetWithinRect(topLeft, bottomRight)) {
                                yield return item;
                            }
                        }
                    }
                }
            }
        }
        public IEnumerable<T> GetWithinRadius(Vector2 point, float radius) {
            // query in the square which the circle circumscribes
            foreach(T item in GetWithinRect(point - radius, point + radius)) {
                // basic point / circle intersection
                float radiusSquared = radius * radius;
                Vector2 itemPos = Positioner.GetPosition(item);
                if((itemPos - point).Length2 <= radiusSquared) {
                    yield return item;
                }
            }
        }

        public QuadTree(AABB bounds) {
            Bounds = bounds;
            Bounds.Resize += BoundsResized;
            Positioner = Activator.CreateInstance<TPositioner>();
        }

        private void BoundsResized(AABB sender, Vector2 newTopLeft, Vector2 newBottomRight) {
            // get all contents to re-add
            IEnumerable<T> contents = AllContents();

            if (!Leaf) {
                CreateChildren();
            }
            foreach (T item in contents) {
                Add(item);
            }
        }
    }
}
