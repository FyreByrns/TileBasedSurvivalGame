using System;
using System.Collections.Generic;
using System.IO;
using SFile = System.IO.File;

namespace FSerialization {
    public class INIFile {
        private Dictionary<string, List<(string, string)>> sections;

        private void EnsureSectionExists(string section) {
            if (!sections.ContainsKey(section)) {
                sections[section] = new List<(string, string)>();
            }
        }

        /// <summary>
        /// Whether the specified key exists.
        /// </summary>
        public bool HasValue(string key) {
            return GetValue(key, out _);
        }

        /// <summary>
        /// Search the entire file for a value.
        /// </summary>
        public bool GetValue(string key, out string result) {
            foreach (string section in sections.Keys) {
                if (GetValue(section, key, out string r)) {
                    result = r;
                    return true;
                }
            }

            result = default;
            return false;
        }
        /// <summary>
        /// Get the string value of a key from a section.
        /// </summary>
        public bool GetValue(string section, string key, out string result) {
            key = key.ToLower();

            if (sections.ContainsKey(section)) {
                foreach ((string k, string v) in sections[section]) {
                    if (k.ToLower() == key) {
                        result = v;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Get a value converted to a certain type. WARNING: does not work very well.
        /// </summary>
        public T GetTypedValue<T>(string section, string key) where T : IConvertible {
            if (GetValue(section, key, out string r)) {
                switch (Type.GetTypeCode(typeof(T))) {
                    case TypeCode.Int32: {
                            return (T)(object)Convert.ToInt32(r);
                        }
                    case TypeCode.Single: {
                            return (T)(object)Convert.ToSingle(r);
                        }
                    case TypeCode.Boolean: {
                            return (T)(object)Convert.ToBoolean(r);
                        }
                    case TypeCode.String: {
                            return (T)(object)r;
                        }
                    default:
                        return default;
                }
            }
            return default;
        }

        public void SetValue<T>(string key, T value) {
            SetValue("default", key, value);
        }
        public void SetValue<T>(string section, string key, T value) {
            EnsureSectionExists(section);
            sections[section].Add((key, value.ToString()));
        }

        /// <summary>
        /// Write the ini file to disk.
        /// </summary>
        public bool Write(string path) {
            try {
                path = $"{path}.ini";
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter stream = new StreamWriter(SFile.OpenWrite(path))) {
                    foreach (string section in sections.Keys) {
                        stream.WriteLine($"[{section}]");

                        foreach ((string key, string value) in sections[section]) {
                            if (key == ";") {
                                // handle comments
                                stream.WriteLine(value);
                                continue;
                            }

                            stream.WriteLine($"{key} = {value}");
                        }
                    }
                }
                return true;
            }
            catch {
                return false;
            }
        }
        public static bool Read(string path, out INIFile result) {
            path = $"{path}.ini";
            result = new INIFile();

            if (SFile.Exists(path)) {
                string currentSection = "default";

                foreach (string line in SFile.ReadAllLines(path)) {
                    if (string.IsNullOrEmpty(line)) {
                        continue;
                    }

                    if (line.StartsWith("[")) {
                        string sec = line.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                        result.EnsureSectionExists(sec);
                        currentSection = sec;
                        continue;
                    }

                    result.EnsureSectionExists(currentSection);
                    if (line.StartsWith(";")) {
                        // handle comments
                        result.sections[currentSection].Add((";", line));
                        continue;
                    }

                    string[] l = line.Split('=');
                    result.sections[currentSection].Add((l[0].Trim(), l[1].Trim()));
                }

                return true;
            }

            return false;
        }

        public INIFile() {
            sections = new Dictionary<string, List<(string, string)>>();
        }
    }
}
