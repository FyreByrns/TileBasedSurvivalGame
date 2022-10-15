using PixelEngine;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame {
    static class ImmediateModeGui {
        public static Pixel BorderColour { get; set; } = Pixel.Presets.Snow;
        public static Pixel ContentColour { get; set; } = Pixel.Presets.White;
        public static Pixel BackgroundColour { get; set; } = Pixel.Presets.Navy;

        public static bool Button(Engine context, int x, int y, string text, int width = 0, int height = 0, int margin = 2, int scale = 1) {
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
    }
}
