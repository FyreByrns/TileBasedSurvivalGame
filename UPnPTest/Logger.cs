using System;

//// = documentation
// = per-step working comments


namespace TileBasedSurvivalGame {
    static class Logger {
        public static bool ShowLogs { get; set; } = false;

        public static void Log(object message) {
            if (ShowLogs) {
                Console.WriteLine(message);
            }
        }
    }
}
