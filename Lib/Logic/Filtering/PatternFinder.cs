using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Logic.Building.SemanticTree;

namespace Logic.Filtering
{
    public static class PatternFinder
    {
        public static string FindNamingPattern(IEnumerable<string> names)
        {
            var splitNames = names.Select(SplitCamelCase).Where(x => x.Any()).ToList();
            var patterns = (from name in splitNames
                from name2 in splitNames
                where !Equals(name2, name)
                where name.Last().Equals(name2.Last())
                select name.Last()).ToList();

            return patterns.Any() ? patterns.First() : null;
        }

        public static IEnumerable<string> SplitCamelCase(string input)
        {
            return Regex.Split(input, "([A-Z][a-z]+)").Where(x => x.Any()).ToList();
        }

        public static string FindBaseClassPattern(IReadOnlyList<Node> nodes)
        {
            var classes = nodes.Where(x => x is ClassNode);
            var lists = classes.Select(x => (x as ClassNode)?.BaseClasses.Select(y => y.ToString())).ToList();

            var common = lists.Aggregate((a, b) => a.Intersect(b)).ToList();
            if (common.Any())
                return common.First();
            return null;
        }
    }
}