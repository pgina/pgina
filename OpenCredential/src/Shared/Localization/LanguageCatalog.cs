using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace OpenCredential.Shared.Localization
{
    // Text only: never load assemblies, serialized objects, external entities or executable content.
    public sealed class LanguageCatalog
    {
        private const int MaximumBytes = 524288;
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<LanguageCatalog> parents = new List<LanguageCatalog>();
        public string Culture { get; private set; }
        public string NativeName { get; private set; }
        public int EntryCount { get { return texts.Count; } }

        private LanguageCatalog(string culture, string name) { Culture = culture; NativeName = name; }
        public static LanguageCatalog English { get { return new LanguageCatalog("en", "English"); } }
        public override string ToString() { return NativeName + " (" + Culture + ")"; }

        public string Get(string module, string key, string english)
        {
            string value;
            return TryGet(module, key, english, out value) ? value : english;
        }

        private bool TryGet(string module, string key, string english, out string value)
        {
            if (texts.TryGetValue(module + "/" + key, out value) && Compatible(english, value)) return true;
            foreach (var parent in parents)
            {
                if (parent.TryGet(module, key, english, out value)) return true;
            }
            value = english;
            return false;
        }

        private static bool Compatible(string original, string translated)
        {
            // Preserve complete format tokens (including format specifiers) and application substitutions.
            const string pattern = @"\{[0-9]+(?:,[^{}]+)?(?::[^{}]+)?\}|%[a-z]";
            var a = Regex.Matches(original, pattern).Cast<Match>().Select(m => m.Value).OrderBy(s => s, StringComparer.Ordinal);
            var b = Regex.Matches(translated, pattern).Cast<Match>().Select(m => m.Value).OrderBy(s => s, StringComparer.Ordinal);
            return a.SequenceEqual(b) && BalancedFormat(translated);
        }

        private static bool BalancedFormat(string value)
        {
            // Validate syntax without rendering attacker-controlled alignment/format widths.
            for (int position = 0; position < value.Length; position++)
            {
                char current = value[position];
                if (current != '{' && current != '}') continue;
                if (position + 1 < value.Length && value[position + 1] == current) { position++; continue; }
                if (current == '}') return false;
                var match = Regex.Match(value.Substring(position), @"\A\{([0-9]+)(?:,\s*(-?[0-9]+)\s*)?(?::([^{}]*))?\}");
                int index, alignment;
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out index) || index > 31 ||
                    (match.Groups[2].Success && (!int.TryParse(match.Groups[2].Value, out alignment) || alignment < -256 || alignment > 256)) ||
                    match.Groups[3].Value.Length > 64) return false;
                position += match.Length - 1;
            }
            return true;
        }

        public static LanguageCatalog Read(Stream stream)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null,
                MaxCharactersInDocument = MaximumBytes, MaxCharactersFromEntities = 0 };
            XDocument document;
            using (var reader = XmlReader.Create(stream, settings)) document = XDocument.Load(reader);
            var root = document.Root;
            if (root == null || root.Name != "language" || (string)root.Attribute("schemaVersion") != "1")
                throw new InvalidDataException("Unsupported language package schema.");
            string culture = (string)root.Attribute("culture");
            string name = (string)root.Attribute("nativeName");
            var info = CultureInfo.GetCultureInfo(culture ?? "");
            if (info.Name.Length == 0 || !string.Equals(info.Name, culture, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(name) || name.Length > 80 || name.Any(char.IsControl))
                throw new InvalidDataException("Invalid language identity.");
            var catalog = new LanguageCatalog(info.Name, name);
            foreach (var module in root.Elements())
            {
                string id = (string)module.Attribute("id");
                if (module.Name != "module" || !ValidKey(id)) throw new InvalidDataException("Invalid language module.");
                foreach (var text in module.Elements())
                {
                    string key = (string)text.Attribute("key");
                    string value = text.Value;
                    if (text.Name != "text" || text.HasElements || !ValidKey(key) || string.IsNullOrWhiteSpace(value) ||
                        value.Length > 8192 || !BalancedFormat(value) || catalog.texts.Count >= 4096)
                        throw new InvalidDataException("Invalid language entry.");
                    string fullKey = id + "/" + key;
                    if (catalog.texts.ContainsKey(fullKey)) throw new InvalidDataException("Duplicate language key.");
                    catalog.texts.Add(fullKey, value);
                }
            }
            return catalog;
        }

        private static bool ValidKey(string value)
        {
            return value != null && value.Length <= 160 && Regex.IsMatch(value, @"\A[A-Za-z0-9_.-]+\z");
        }

        public static LanguageCatalog ReadFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > MaximumBytes) throw new InvalidDataException("Language package is too large.");
                return Read(stream);
            }
        }

        public static LanguageCatalog Load(string directory, string culture)
        {
            var info = CultureInfo.GetCultureInfo(culture);
            var result = new LanguageCatalog(info.Name, info.NativeName);
            while (info.Name.Length != 0)
            {
                string path = Path.Combine(directory, info.Name + ".xml");
                try
                {
                    if (File.Exists(path))
                    {
                        var external = ReadFile(path);
                        if (string.Equals(external.Culture, info.Name, StringComparison.OrdinalIgnoreCase)) result.parents.Add(external);
                    }
                }
                catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is XmlException || e is ArgumentException) { }
                if (info.Name == "es")
                    using (var stream = typeof(LanguageCatalog).Assembly.GetManifestResourceStream("OpenCredential.Shared.Localization.es.xml"))
                        result.parents.Add(Read(stream));
                info = info.Parent;
            }
            return result;
        }

        public static IList<LanguageCatalog> Available(string directory)
        {
            var choices = new Dictionary<string, LanguageCatalog>(StringComparer.OrdinalIgnoreCase);
            choices.Add("en", new LanguageCatalog("en", "English"));
            choices.Add("es", new LanguageCatalog("es", "Español"));
            try
            {
                if (Directory.Exists(directory))
                foreach (var file in Directory.EnumerateFiles(directory, "*.xml").Take(128))
                {
                    try
                    {
                        var item = ReadFile(file);
                        if (string.Equals(Path.GetFileNameWithoutExtension(file), item.Culture, StringComparison.OrdinalIgnoreCase))
                            choices[item.Culture] = item;
                    }
                    catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is XmlException || e is ArgumentException) { }
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { }
            return choices.Values.OrderBy(c => c.NativeName, StringComparer.Ordinal).ToList();
        }

        public static LanguageCatalog Import(string source, string directory)
        {
            // Capture and validate the same bytes that will be installed (no validate/copy race).
            byte[] bytes;
            using (var stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > MaximumBytes) throw new InvalidDataException("Language package is too large.");
                using (var memory = new MemoryStream()) { stream.CopyTo(memory); bytes = memory.ToArray(); }
            }
            LanguageCatalog catalog;
            using (var stream = new MemoryStream(bytes)) catalog = Read(stream);
            Directory.CreateDirectory(directory);
            string temporary = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                File.WriteAllBytes(temporary, bytes);
                // Do not silently overwrite an existing translation.
                File.Move(temporary, Path.Combine(directory, catalog.Culture + ".xml"));
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return catalog;
        }
    }

    public static class UiLanguage
    {
        private static LanguageCatalog current = LanguageCatalog.English;
        public static LanguageCatalog Current
        {
            get { return current; }
            set { current = value ?? throw new ArgumentNullException("value"); }
        }
        public static string Text(string module, string key, string english) { return Current.Get(module, key, english); }
    }
}
