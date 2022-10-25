using System.Collections.Generic;

namespace TileBasedSurvivalGame.World {
    class QuadTree {
        public AABB Bounds { get; set; }

        // how many items are allowed in the leaf before splitting
        public int SplitThreshold { get; set; } = 10;
        public List<Positioned> Contents { get; set; } = null;
        // only leaves may have contents
        public bool Leaf => Contents != null;

        public QuadTree TopLeft { get; set; }
        public QuadTree TopRight { get; set; }
        public QuadTree BottomLeft { get; set; }
        public QuadTree BottomRight { get; set; }
        public QuadTree[] Children => new[] { TopLeft, TopRight, BottomLeft, BottomRight };
        void CreateChildren() {
            float halfWidth = Bounds.Width / 2;
            float halfHeight = Bounds.Height / 2;

            // TopLeft is 1ACB
            // TopRight is A2DC
            // BottomLeft is BCE3
            // BottomRight is CDE4
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

            TopLeft = new QuadTree(new AABB(Bounds.TopLeft, middle));
            TopRight = new QuadTree(new AABB(topMid, rightMid));
            BottomLeft = new QuadTree(new AABB(leftMid, bottomMid));
            BottomRight = new QuadTree(new AABB(middle, Bounds.BottomRight));
        }
        void MoveContentsToChildren() {
            // copy contents
            IEnumerable<Positioned> contents = Contents.ToArray();
            // no more contents, this tree is a branch now
            Contents = null;
            foreach (Positioned item in contents) {
                Add(item);
            }
        }

        public bool Contains(Positioned item, out QuadTree container) {
            if (Leaf) {
                container = this;
                return Contents.Contains(item);
            }

            foreach (QuadTree child in Children) {
                if (child.Contains(item, out QuadTree _container)) {
                    container = _container;
                    return true;
                }
            }

            container = null;
            return false;
        }
        public IEnumerable<Positioned> AllContents() {
            // if this tree is a leaf ..
            if (Leaf) {
                // .. just return all contents
                foreach (Positioned item in Contents) {
                    yield return item;
                }
            }
            // otherwise ..
            else {
                // recursively find leaves and return contents
                foreach (QuadTree child in Children) {
                    foreach (Positioned item in child.AllContents()) {
                        yield return item;
                    }
                }
            }
        }

        public bool Add(Positioned item, bool resizeToFitItem = false) {
            // lock, in case I do stupid multithreading things
            lock (Contents) {
                // if the item is outside current bounds ..
                if (!Bounds.PointWithin(item.Position)) {
                    // .. and resizing is allowed,
                    if (resizeToFitItem) {
                        // .. resize
                        Bounds.ResizeToFit(item.Position);
                    }
                    // .. otherwise, fail
                    else {
                        return false;
                    }
                }

                bool success = false;

                // if this tree is a leaf, simply add to contents
                if (Leaf) {
                    Contents.Add(item);
                    success = true;
                }
                // otherwise, find which child to add to
                else {
                    foreach (QuadTree child in Children) {
                        // if the item is within the bounds of child ..
                        if (child.Bounds.PointWithin(item.Position)) {
                            // .. add it to child
                            success = child.Add(item);
                            break;
                        }
                    }
                }

                // if this tree was a leaf and is now too stuffed, split
                if (Contents.Count > SplitThreshold) {
                    CreateChildren();
                    MoveContentsToChildren();
                }

                return success;
            }
        }
        public bool Remove(Positioned item) {
            // if this tree contains the item ..
            if (Contains(item, out QuadTree container)) {
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

        public QuadTree(AABB bounds) {
            Bounds = bounds;
            Bounds.Resize += BoundsResized;
        }

        private void BoundsResized(AABB sender, Vector2 newTopLeft, Vector2 newBottomRight) {
            // get all contents to re-add
            IEnumerable<Positioned> contents = AllContents();

            if (!Leaf) {
                CreateChildren();
            }
            foreach (Positioned item in contents) {
                Add(item);
            }
        }
    }
}
