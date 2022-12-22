using PixelEngine;
using System;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame {
    static class ImmediateModeGui {
        public static Pixel BorderColour { get; set; } = Pixel.Presets.Snow;
        public static Pixel ContentColour { get; set; } = Pixel.Presets.White;
        public static Pixel BackgroundColour { get; set; } = Pixel.Presets.Navy;

        public static Vector2 TextSize(string text, int margin = 2, int scale = 1) {
            int textPixelSize = 8 * scale;
            int newlinesInText = 1;
            foreach (char c in text) {
                if (c == '\n')
                    newlinesInText++;
            }

            return ((text.Length * textPixelSize) + margin * 2, newlinesInText * (textPixelSize + (margin * 2)));
        }

        public static bool Button(Game context, int x, int y, string text, int width = 0, int height = 0, int margin = 2, int scale = 1) {
            int textPixelSize = 8 * scale;

            // if dimensions are zero, autogenerate them from text
            if (width == 0) {
                width = text.Length * textPixelSize + margin * 2;
            }
            if (height == 0) {
                int newlinesInText = 1;
                foreach (char c in text) {
                    if (c == '\n')
                        newlinesInText++;
                }
                height = newlinesInText * textPixelSize + margin * 2;
            }

            // draw button
            Point topLeft = new Point(x, y);
            Point bottomRight = new Point(x + width, y + height);
            Point textLocation = new Point(x + margin, y + margin);
            context.FillRect(topLeft, bottomRight, BackgroundColour);
            context.DrawText(textLocation, text, ContentColour, scale);
            context.DrawRect(topLeft, bottomRight, BorderColour);

            // detect press
            if (context.MouseX > x && context.MouseY > y && context.MouseX < x + width && context.MouseY < y + height) {
                if (context.GetMouse(Mouse.Left).Down) {
                    return true;
                }
            }
            return false;
        }

        public static bool EnumDropdown<T>(Game context, int x, int y, ref T selection, ref bool hovered, int margin = 2, int scale = 1) {
            int textPixelSize = 8 * scale;
            int width = 0;
            int height = textPixelSize + margin * 2;

            T[] possibilities = (T[])Enum.GetValues(typeof(T));
            string[] names = new string[possibilities.Length];
            for (int i = 0; i < possibilities.Length; i++) {
                names[i] = possibilities[i].ToString();

                width = Math.Max(width, names[i].Length * textPixelSize + margin * 2);
            }

            // if the mouse is hovering over the dropdown, show all possibilities
            hovered |= context.MouseX > x && context.MouseY > y && context.MouseX < x + width && context.MouseY < y + height;
            if (hovered) {
                height = (names.Length + 1) * (textPixelSize + margin * 2);
                hovered = context.MouseX > x && context.MouseY > y && context.MouseX < x + width && context.MouseY < y + height;
                for (int i = 0; i < names.Length; i++) {
                    if (
                    Button(context, x, y + (textPixelSize + margin * 2) * (i + 1), names[i], width, 0, margin, scale)) {
                        selection = possibilities[i];
                        return true;
                    }
                }
            }
            // otherwise, just show the current selection
            Button(context, x, y, names[Array.IndexOf(possibilities, selection)], width, 0, margin, scale);
            return false;
        }

        public static void Slider(Game context, int x, int y, int width, int height, float minValue, float maxValue, ref float value, int handleSize = 3, int margin = 2) {
            Vector2 topLeft = (x, y);
            Vector2 bottomRight = (x + width, y + height);

            Vector2 sliderLineStart = (x + margin, y + height / 2);
            Vector2 sliderLineEnd = (bottomRight.x - margin, y + height / 2);

            context.FillRect(topLeft, bottomRight, BackgroundColour);
            context.DrawRect(topLeft, bottomRight, BorderColour);
            context.DrawLine(sliderLineStart, sliderLineEnd, BorderColour);

            float percentageAlong = (value - minValue) / (maxValue - minValue);
            float loc = context.Lerp(sliderLineStart.x, sliderLineEnd.x, percentageAlong);
            Vector2 handleLoc = (loc, sliderLineStart.y);

            if (context.GetMouse(Mouse.Left).Down &&
                context.MouseX > x && context.MouseY > y && context.MouseX < bottomRight.x && context.MouseY < bottomRight.y) {
                handleLoc.x = context.MouseX;
                value = (maxValue - minValue) * (handleLoc.x - sliderLineStart.x) / (sliderLineEnd.x - sliderLineStart.x); // range mapping my behated
            }

            context.FillCircle(handleLoc, handleSize, ContentColour);
        }

        public static bool InputBox(Game context, int x, int y, string query, ref string state, int margin = 2, int scale = 1, Key enterKey = Key.Enter) {
            Vector2 topLeft = (x, y);
            Vector2 queryLocation = topLeft + (margin, margin);
            Vector2 entryLocation = topLeft + (margin, TextSize(query, margin, scale).y);
            Vector2 bottomRight = topLeft + TextSize(query.Length > state.Length ? query : state, margin, scale) * (1, 2); // twice as tall

            context.FillRect(topLeft, bottomRight, BackgroundColour);
            context.DrawText(queryLocation, query, ContentColour);
            context.DrawText(entryLocation, state, ContentColour);
            context.DrawRect(topLeft, bottomRight, BorderColour);

            if (context.GetKey(Key.Any).Pressed) {
                for(Key key = Key.A; key <= Key.K9; key++) {
                    if (context.GetKey(key).Pressed) {
                        state += context.GetChar(key);
                    }
                }
                for (Key key = Key.OEM_1; key <= Key.OEM_PERIOD; key++) {
                    if (context.GetKey(key).Pressed) {
                        state += context.GetChar(key);
                    }
                }

                if (context.GetKey(Key.Space).Pressed) {
                    state += " ";
                }

                if (context.GetKey(Key.Back).Pressed) {
                    if (state.Length > 0) {
                        state = state.Substring(0, state.Length - 1);
                    }
                }
            }
            
            return context.GetKey(enterKey).Pressed;
        }
    }
}
