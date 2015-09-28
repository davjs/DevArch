using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Analysis
{
    public static class PatternFinder
    {
        public static string FindPattern(IEnumerable<string> names)
        {
            var splitNames = names.Select(SplitCamelCase).Where(x => x.Any()).ToList();
            var patterns = new List<string>();
            foreach (var name in splitNames)
            {
                foreach (var name2 in splitNames)
                {
                    if(name2 == name)
                        continue;
                    if (name.Last().Equals(name2.Last()))
                    {
                        patterns.Add(name.Last());
                    }
                }
            }
            if (patterns.Any())
                return patterns.First();
            return null;
        }

        public static IEnumerable<string> SplitCamelCase(string input)
        {
            return Regex.Split(input, "([A-Z][a-z]+)").Where(x => x.Any()).ToList();
        }
    }
}