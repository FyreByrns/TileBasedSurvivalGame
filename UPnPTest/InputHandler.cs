using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;

namespace TileBasedSurvivalGame {
    static class InputHandler {
        public delegate void InputEventHandler(string input, int ticksHeld);
        public static event InputEventHandler Input;

        public static Dictionary<string, Bind> Binds { get; }
        = new Dictionary<string, Bind>();

        // track how many ticks the mouse has been down
        public static int[] MouseButtons
        = new int[(int)Mouse.Any];
        // track how many ticks keys have been down
        public static int[] Keys
        = new int[(int)Key.Any];

        public static int MouseX { get; private set; }
        public static int MouseY { get; private set; }
        public static int MouseScroll { get; private set; }

        public static IEnumerable<string> BoundTo(Mouse button) {
            if (Bind.Bound(button)) {
                foreach (string input in Binds.Keys) {
                    Bind current = Binds[input];
                    if (current == null) continue;

                    if (current.MouseBinds.Contains(button)) {
                        yield return input;
                    }
                }
            }
        }
        public static IEnumerable<string> BoundTo(Key key) {
            if (Bind.Bound(key)) {
                foreach (string input in Binds.Keys) {
                    Bind current = Binds[input];
                    if (current == null) continue;

                    if (current.KeyBinds.Contains(key)) {
                        yield return input;
                    }
                }
            }
        }

        public static bool InputHeld(string input) {
            if (Binds.ContainsKey(input)) {
                foreach(Key key in Binds[input].KeyBinds) {
                    if (Keys[(int)key] > 0) {
                        return true;
                    }
                }
                foreach (Mouse mouse in Binds[input].MouseBinds) {
                    if (MouseButtons[(int)mouse] > 0) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void BindInput(string name, Mouse button) {
            name = name.ToLower(); // don't worry about accidental capitals

            if (!Binds.ContainsKey(name)) {
                Binds[name] = new Bind(name);
            }

            Binds[name].AddBind(button);
        }
        public static void BindInput(string name, Key key) {
            name = name.ToLower();

            if (!Binds.ContainsKey(name)) {
                Binds[name] = new Bind(name);
            }

            Binds[name].AddBind(key);
        }

        public static void UpdateMouse(int mouseX, int mouseY, int scroll) {
            MouseX = mouseX;
            MouseY = mouseY;
            MouseScroll = scroll;
        }
        public static void Update(bool[] mouseButtons, bool[] keys) {
            for (int button = 0; button < MouseButtons.Length; button++) {
                if (mouseButtons[button]) { // if the key is down
                    MouseButtons[button]++; // increment the number of frames it has been held
                }
                else {
                    MouseButtons[button] = 0;// otherwise reset the count
                }

                if (MouseButtons[button] > 0) {
                    foreach (string input in BoundTo((Mouse)button)) {
                        Input?.Invoke(input, MouseButtons[button]);
                    }
                }
            }
            for (int key = 0; key < Keys.Length; key++) {
                if (keys[key]) {
                    Keys[key]++;
                }
                else {
                    Keys[key] = 0;
                }

                if (Keys[key] > 0) {
                    foreach (string input in BoundTo((Key)key)) {
                        Input?.Invoke(input, Keys[key]);
                    }
                }
            }
        }

        public class Bind {
            static int[] boundButtons = new int[(int)Mouse.Any];
            static int[] boundKeys = new int[(int)Key.Any];
            public static bool Bound(Mouse button) {
                return boundButtons[(int)button] > 0;
            }
            public static bool Bound(Key key) {
                return boundKeys[(int)key] > 0;
            }

            public string Name { get; }
            public List<Key> KeyBinds { get; }
            = new List<Key>();
            public List<Mouse> MouseBinds { get; }
            = new List<Mouse>();

            public void AddBind(Mouse button) {
                MouseBinds.Add(button);
                boundButtons[(int)button]++;
            }
            public void AddBind(Key key) {
                KeyBinds.Add(key);
                boundKeys[(int)key]++;
            }

            public void Unbind(Mouse button) {
                MouseBinds.Remove(button);
                boundButtons[(int)button]--;
            }
            public void Unbind(Key key) {
                KeyBinds.Remove(key);
                boundKeys[(int)key]--;
            }

            public Bind(string name) {
                Name = name;
            }

            ~Bind() {
                for (int b = MouseBinds.Count - 1; b >= 0; b--) {
                    Unbind(MouseBinds[b]);
                }
                for (int k = KeyBinds.Count - 1; k >= 0; k--) {
                    Unbind(KeyBinds[k]);
                }
            }
        }
    }
}
