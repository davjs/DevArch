using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Logic.Building.SemanticTree;

namespace Logic.Filtering
{
    public static class PatternFinder
    {
        public static IEnumerable<string> FindNamingPatterns(IEnumerable<string> names)
        {
            var splitNames = names.Select(SplitCamelCase).Where(x => x.Any()).ToList();
            var patterns = (from name in splitNames
                from name2 in splitNames
                where !Equals(name2, name)
                where name.Last().Equals(name2.Last())
                select name.Last()).ToList();

            return patterns.Distinct();
        }

        public static bool FollowsPattern(string name,string pattern)
        {
            return (SplitCamelCase(name).Last().Equals(pattern));
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
            return common.Any() ? common.First() : null;
        }
    }
}