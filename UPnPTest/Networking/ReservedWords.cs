using System.Collections.Generic;
using System.Reflection;

//// = documentation
// = per-step working comments


namespace TileBasedSurvivalGame.Networking {
    //// to keep track of all reserved / disallowed words
    static class ReservedWords {
        public static string Unset { get; } = "[unset]";

        //// whether a word is allowed
        public static bool WordIsAllowed(string word) {
            return !_reservedWords.Contains(word);
        }

        #region functionality
        static List<string> _reservedWords = new List<string>();

        public static bool IsWordReserved(string word) {
            return _reservedWords.Contains(word);
        }

        static ReservedWords() {
            // automagically get all string properties of this static class
            // .. and add them to the reserved list
            foreach (PropertyInfo propertyInfo in typeof(ReservedWords).GetProperties()) {
                if (propertyInfo.PropertyType == typeof(string)) {
                    _reservedWords.Add((string)propertyInfo.GetValue(null));
                }
            }
        }
        #endregion functionality
    }
}
