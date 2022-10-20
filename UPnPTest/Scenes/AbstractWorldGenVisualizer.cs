//// = documentation
// = per-step working comments

using PixelEngine;
using System.Linq;
using TileBasedSurvivalGame.World.Abstract;
using gui = TileBasedSurvivalGame.ImmediateModeGui;

namespace TileBasedSurvivalGame.Scenes {
    class AbstractWorldGenVisualizer : Scene {
        public override string Name => "Abstract World Gen Visualizer";
        public override Scene Next { get; protected set; }

        public AbstractWorld AbstractWorld { get; set; }
        WorldNode _selectedNode = null;

        float _lastElapsed;

        Vector2 _cameraLocation;
        Vector2 _newCameraLocation;
        float _cameraZoom = 1;
        float _cameraSpeed = 200;
        Vector2 ScreenToWorld(Vector2 screenLocation) {
            return (screenLocation / _cameraZoom) + _cameraLocation;
        }
        Vector2 WorldToScreen(Vector2 worldLocation) {
            return (worldLocation - _cameraLocation) * _cameraZoom;
        }

        void CenterCameraOn(WorldNode node, bool snap = false) {
            _newCameraLocation = node.Position - (Config.ScreenWidth / 2f / _cameraZoom, Config.ScreenHeight / 2f / _cameraZoom);
            if (snap) {
                _cameraLocation = _newCameraLocation;
            }
        }

        public override void Begin(Engine instance) {
            _cameraLocation = new Vector2(instance.ScreenWidth / 2, instance.ScreenHeight / 2) * -1;
            _newCameraLocation = _cameraLocation;
        }

        public override void Update(Engine instance, float elapsed) {
            _lastElapsed = elapsed;
            Next = this;

            // handle zooming
            if (InputHandler.MouseScroll != 0) {
                Vector2 oldMouseWorldLoc = ScreenToWorld((instance.MouseX, instance.MouseY));

                if (InputHandler.MouseScroll > 0) {
                    _cameraZoom *= 1.01f;
                }
                if (InputHandler.MouseScroll < 0) {
                    _cameraZoom *= 0.99f;
                }

                Vector2 newMouseWorldLoc = ScreenToWorld((instance.MouseX, instance.MouseY));
                Vector2 difference = oldMouseWorldLoc - newMouseWorldLoc;

                _newCameraLocation = _cameraLocation = _cameraLocation + difference;
            }

            // handle camera movement
            _cameraLocation.x = instance.Lerp(_cameraLocation.x, _newCameraLocation.x, elapsed);
            _cameraLocation.y = instance.Lerp(_cameraLocation.y, _newCameraLocation.y, elapsed);
        }
        public override void Tick(Engine instance) { }

        bool _typeDropdownHovered = false;
        override public void Render(Engine instance) {
            instance.Clear(Pixel.Empty);
            // recursively draw nodes and connections
            foreach (WorldNode node in AbstractWorld.Origin.GetAllChildren()) {
                instance.DrawCircle(WorldToScreen(node.Position), 1, _selectedNode == node ? Pixel.Presets.Green : Pixel.Presets.Grey);
                instance.DrawText(WorldToScreen(node.Position) + (4, 4), new string(node.Type.ToString().Take(2).ToArray()), Pixel.Presets.Grey);
                instance.DrawCircle(WorldToScreen(node.Position), (int)(node.EffectRadius * _cameraZoom), Pixel.Presets.Lime);
                if (node.PositionLocked) {
                    instance.DrawCircle(WorldToScreen(node.Position), 2, Pixel.Presets.DarkGrey);
                }

                foreach (Connection connection in node.Connections) {
                    if (connection.B == null) continue;

                    instance.DrawLine(WorldToScreen(connection.A.Position), WorldToScreen(connection.B.Position), Pixel.Presets.Lime);
                }
            }

            // if there's a node selected, allow editing
            if (_selectedNode != null) {
                // current position
                string positionString = $"{_selectedNode.Position.x:000.00},{_selectedNode.Position.y:000.00}";
                instance.DrawText(new Vector2(0, instance.ScreenHeight - gui.TextSize(positionString).y), positionString, Pixel.Presets.White);

                // distance
                instance.DrawLine(WorldToScreen(_selectedNode.Position), new Point(instance.MouseX, instance.MouseY), Pixel.Presets.DarkGrey);
                float distance = (_selectedNode.Position - ScreenToWorld((instance.MouseX, instance.MouseY))).Length;
                instance.DrawText(new Point(instance.MouseX, instance.MouseY), $"{distance:000.00}", Pixel.Presets.DarkGrey);

                // radius
                gui.Slider(instance, 180, 0, 100, 10, 0, 100, ref _selectedNode.EffectRadius);
                instance.DrawText(WorldToScreen(_selectedNode.Position + (_selectedNode.EffectRadius, 0)), $"{_selectedNode.EffectRadius:000.00}", Pixel.Presets.Lime);

                gui.EnumDropdown(instance, 0, 0, ref _selectedNode.Type, ref _typeDropdownHovered);
                if (gui.Button(instance, instance.ScreenWidth - (int)gui.TextSize("X").x, 0, "X")) { _selectedNode = null; }

                // lock / unlock position
                if (gui.Button(instance, instance.ScreenWidth - (int)gui.TextSize("L").x - (int)gui.TextSize("X").x, 0, "L")) { _selectedNode.PositionLocked = true; }
                if (gui.Button(instance, instance.ScreenWidth - (int)gui.TextSize("U").x - (int)gui.TextSize("L").x - (int)gui.TextSize("X").x, 0, "U")) { _selectedNode.PositionLocked = false; }
            }

            instance.Draw(WorldToScreen(_cameraLocation), Pixel.Presets.Red);
            instance.Draw(WorldToScreen(_newCameraLocation), Pixel.Presets.Pink);
        }

        public AbstractWorldGenVisualizer() {
            AbstractWorld = new AbstractWorld();
            AbstractWorld.Origin.PositionLocked = true;

            InputHandler.Input += InputHandler_Input;
            InputHandler.BindInput("cam_up", Key.W);
            InputHandler.BindInput("cam_down", Key.S);
            InputHandler.BindInput("cam_left", Key.A);
            InputHandler.BindInput("cam_right", Key.D);

            InputHandler.BindInput("delete_node", Key.Delete);
            InputHandler.BindInput("chain_add", Key.Shift);
            InputHandler.BindInput("reparent_add", Key.Control);
            InputHandler.BindInput("retransform", Key.Tab);

            InputHandler.BindInput("mouse_left", Mouse.Left);
            InputHandler.BindInput("mouse_right", Mouse.Right);
        }

        private void InputHandler_Input(string input, int ticksHeld) {
            if (input == "cam_up") _newCameraLocation.y -= _cameraSpeed * _lastElapsed;
            if (input == "cam_down") _newCameraLocation.y += _cameraSpeed * _lastElapsed;
            if (input == "cam_left") _newCameraLocation.x -= _cameraSpeed * _lastElapsed;
            if (input == "cam_right") _newCameraLocation.x += _cameraSpeed * _lastElapsed;

            if (input == "mouse_left") {
                foreach (WorldNode node in AbstractWorld.Origin.GetAllChildren()) {
                    if ((WorldToScreen(node.Position) - (InputHandler.MouseX, InputHandler.MouseY)).Length < 4) {
                        _selectedNode = node;
                        break;
                    }
                }

                if (ticksHeld > 1) {
                    if (!_selectedNode?.PositionLocked ?? false) {
                        if ((WorldToScreen(_selectedNode.Position) - (InputHandler.MouseX, InputHandler.MouseY)).Length < 32) {
                            Vector2 oldPosition = _selectedNode.Position;
                            _selectedNode.Position = ScreenToWorld((InputHandler.MouseX, InputHandler.MouseY));

                            if (InputHandler.InputHeld("retransform")) {
                                foreach (Connection connection in _selectedNode.GetAllChildConections()) {
                                    connection.B.Position -= (oldPosition - _selectedNode.Position);
                                }
                            }
                        }
                    }
                }
            }
            if (input == "mouse_right") {
                if (ticksHeld == 1) {
                    if (_selectedNode != null) {
                        WorldNode newNode = new WorldNode() {
                            Position = ScreenToWorld((InputHandler.MouseX, InputHandler.MouseY))
                        };

                        _selectedNode.Connect(newNode, true);

                        if (InputHandler.InputHeld("reparent_add")) {
                            newNode.Connections.AddRange(_selectedNode.Connections.Where(x => x.B != newNode));
                            foreach (Connection connection in newNode.Connections) {
                                if (connection.B != newNode) {
                                    _selectedNode.Connections.Remove(connection);
                                    connection.A = newNode;
                                    connection.B.ConnectionToParent.A = newNode;
                                    connection.B.ConnectionToParent.B = connection.B;
                                }
                            }
                            if (InputHandler.InputHeld("retransform")) {
                                foreach (Connection connection in newNode.GetAllChildConections()) {
                                    connection.B.Position += (newNode.Position - _selectedNode.Position);
                                }
                            }
                        }
                        if (InputHandler.InputHeld("chain_add")) {
                            newNode.Type = _selectedNode.Type;
                            _selectedNode = newNode;
                            CenterCameraOn(newNode);
                        }
                    }
                }
            }
            if (input == "delete_node") {
                if (ticksHeld == 1) {
                    if (_selectedNode != null) {
                        WorldNode parent = _selectedNode.ConnectionToParent?.A;
                        if (parent != null) {
                            parent.Disconnect(_selectedNode);
                            _selectedNode = parent;
                            CenterCameraOn(parent);
                        }
                    }
                }
            }
        }
    }
}
