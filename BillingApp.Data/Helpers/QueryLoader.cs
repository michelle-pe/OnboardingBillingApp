using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace BillingApp.Data.Helpers
{
    public class QueryLoader
    {
        // Cache so we don't re-read XML from disk on every query call
        private static readonly Dictionary<string, Dictionary<string, string>> _cache
            = new Dictionary<string, Dictionary<string, string>>();

        private static readonly object _lock = new object();

        /// <summary>
        /// Load a named query from a XML file.
        /// fileName example: "CustomerQueries"  (no path, no .xml)
        /// </summary>
        public static string GetQuery(string fileName, string queryName)
        {
            var key = fileName;

            if (!_cache.ContainsKey(key))
            {
                lock (_lock)
                {
                    if (!_cache.ContainsKey(key))
                    {
                        _cache[key] = LoadFile(fileName);
                    }
                }
            }

            if (!_cache[key].ContainsKey(queryName))
                throw new Exception($"Query '{queryName}' not found in '{fileName}.xml'");

            return _cache[key][queryName];
        }

        private static Dictionary<string, string> LoadFile(string fileName)
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Queries",
                fileName + ".xml"
            );

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    string.Format("Query file not found: {0}", path)
                );

            var queries = new Dictionary<string, string>();
            var doc = XDocument.Load(path);

            foreach (var element in doc.Root.Elements("query"))
            {
                var name = element.Attribute("name")?.Value;
                var sql = element.Value.Trim();
                if (!string.IsNullOrEmpty(name))
                    queries[name] = sql;
            }

            return queries;
        }
    }
}