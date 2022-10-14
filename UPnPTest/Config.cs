using System;
using System.IO;
using System.Collections.Generic;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame {
    //// contains various information about configuration
    static class Config {
        public static bool Log { get; set; }
            = false;
        public static int ScreenWidth { get; set; }
            = 400;
        public static int ScreenHeight { get; set; }
            = 225;
        public static int PixelSize { get; set; }
            = 2;

        static Dictionary<string, Action<string>> LoadMethods = new Dictionary<string, Action<string>>() {
            { "Log", s          => Log = s.ToLower().Trim() == "true" },
            { "ScreenWidth", s  => ScreenWidth = int.Parse(s) },
            { "ScreenHeight", s => ScreenHeight = int.Parse(s) },
            { "PixelSize", s    => PixelSize = int.Parse(s) },
        };

        public static void Save() {
            List<string> values = new List<string>();
            void addComment(string comment) {
                values.Add($"#{comment}");
            }
            void addValue(string name, object value) {
                values.Add($"{name}: {value}");
            }

            addComment("whether to show logs");
            addValue("Log", Log);
            addComment("screen width / height");
            addValue("ScreenWidth", ScreenWidth);
            addValue("ScreenHeight", ScreenHeight);
            addComment("pixel size (1 = same as monitor, 2 = 2x2 monitor pixels, etc..)");
            addValue("PixelSize", PixelSize);

            if (File.Exists("config.txt")) {
                File.Delete("config.txt");
            }
            File.WriteAllLines("config.txt", values.ToArray());
        }
        public static void Load() {
            if (File.Exists("config.txt")) {
                foreach (string line in File.ReadAllLines("config.txt")) {
                    if (line.StartsWith("#")) continue; // ignore comments
                    // split the line into key and value
                    string[] lineParts = line.Split(new[] { ": " }, StringSplitOptions.None);
                    string key = lineParts[0];
                    string value = lineParts[1];
                    // attempt to set config value by key
                    try {
                        LoadMethods[key](value);
                    }
                    catch (Exception e) {
                        Logger.Log($"error while attempting to parse config value {key}:\n\t{e.Message}");
                    }
                }
            }
            else {
                Logger.Log("config not found");
            }
        }
    }
}
