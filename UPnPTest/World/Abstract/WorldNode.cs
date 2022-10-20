using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    enum WorldNodeType {
        None,

        PointOfInterest,
        Path,
    }

    class WorldNode {
        public Vector2 Position { get; set; }
        public bool PositionLocked { get; set; }
        public WorldNodeType Type;
        public float EffectRadius;

        public Connection ConnectionToParent { get; set; }
        public List<Connection> Connections { get; }
            = new List<Connection>();

        public bool HasChild(WorldNode node, out Connection connection) {
            IEnumerable<Connection> query = Connections.Where(x => x.B == node);

            if (query.Count() > 0) {
                connection = query.First();
                return true;
            }
            connection = null;
            return false;
        }
        public bool IsParentOfThisNode(WorldNode node) {
            return ConnectionToParent.A == node;
        }

        public void Connect(WorldNode to, bool parent = false) {
            if (parent) {
                Connection connection = new Connection(this, to);
                Connections.Add(connection);
                to.ConnectionToParent = connection;
            }
            else {
                to.Connect(this, true);
            }
        }
        public void Disconnect(WorldNode from) {
            foreach (Connection childConnection in from.Connections) {
                childConnection.A = this;
                childConnection.B.ConnectionToParent.A = this;
                Connections.Add(childConnection);
            }
            Connections.RemoveAll(x => x.B == from);
        }

        public IEnumerable<WorldNode> GetAllChildren() {
            yield return this;
            foreach (Connection connection in Connections) {
                foreach (WorldNode node in connection?.B?.GetAllChildren()) {
                    yield return node;
                }
            }
        }
        public IEnumerable<Connection> GetAllChildConections() {
            foreach (Connection connection in Connections) {
                yield return connection;
                foreach (Connection cc in connection.B?.GetAllChildConections()) {
                    yield return cc;
                }
            }
        }
    }
}
