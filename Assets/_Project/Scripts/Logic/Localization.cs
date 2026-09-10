// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;
using System.Collections.Generic;

namespace Starfall.Logic
{
    public enum Language
    {
        English = 0,
        Portuguese = 1,
    }

    /// <summary>
    /// Source-string localization: the English text used in code and data is the key, and each language provides a
    /// table from that text to its translation. Missing entries fall back to the English source, so partial tables
    /// never break the UI. Dynamic strings use <see cref="F"/> with numbered placeholders ("STAGE {0}").
    /// </summary>
    public static class Loc
    {
        private static readonly Dictionary<Language, IReadOnlyDictionary<string, string>> Tables = new Dictionary<Language, IReadOnlyDictionary<string, string>>();
        private static IReadOnlyDictionary<string, string> _current;

        public static Language Current { get; private set; } = Language.English;
        public static event Action Changed;

        public static void Register(Language language, IReadOnlyDictionary<string, string> table)
        {
            Tables[language] = table;
            if (language == Current) _current = table;
        }

        public static void Set(Language language)
        {
            if (Current == language && _current != null) return;
            Current = language;
            Tables.TryGetValue(language, out _current);
            Changed?.Invoke();
        }

        /// <summary>Language code of the device ("pt", "en", ...) → best supported language.</summary>
        public static Language FromSystem(string languageName)
        {
            if (string.IsNullOrEmpty(languageName)) return Language.English;
            string l = languageName.ToLowerInvariant();
            return l.StartsWith("pt") || l.StartsWith("portug") ? Language.Portuguese : Language.English;
        }

        public static Language Next(Language language) => language == Language.English ? Language.Portuguese : Language.English;

        public static string DisplayName(Language language) => language == Language.Portuguese ? "PORTUGUÊS (BR)" : "ENGLISH";

        /// <summary>Translates a source string; returns the source when there is no entry.</summary>
        public static string T(string source)
        {
            if (string.IsNullOrEmpty(source) || _current == null) return source;
            return _current.TryGetValue(source, out var translated) ? translated : source;
        }

        /// <summary>Translates a format string and fills numbered placeholders.</summary>
        public static string F(string source, params object[] args)
        {
            string format = T(source);
            try { return string.Format(format, args); }
            catch (FormatException) { return string.Format(source, args); }
        }

        public static bool Has(string source) => _current != null && !string.IsNullOrEmpty(source) && _current.ContainsKey(source);

        /// <summary>Upper-case helper that is safe for accented characters.</summary>
        public static string Upper(string s) => string.IsNullOrEmpty(s) ? s : s.ToUpperInvariant();
    }
}
