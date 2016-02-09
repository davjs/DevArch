using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering
{
    public class IndirectSiblingBuilderAsync
    {
        readonly Dictionary<Node, Task<HashSet<Node>>> _indirectSiblingDeps = new Dictionary<Node, Task<HashSet<Node>>>();

        public IndirectSiblingBuilderAsync(IEnumerable<Node> toBuildDepsFor)
        {
            foreach (var node in toBuildDepsFor)
            {
                _indirectSiblingDeps[node] = BuildDepsForAsync(node);
            }
        }
        
        private async Task<HashSet<Node>> BuildDepsForAsync(Node n, Node exclude = null)
        {
            var sibDeps = n.SiblingDependencies.Where(dependency => dependency != exclude).ToHashSet();
            foreach (var dependency in sibDeps.ToHashSet())
            {
                if (!_indirectSiblingDeps.ContainsKey(dependency))
                    sibDeps.UnionWith(await BuildDepsForAsync(dependency, n));
                else
                    sibDeps.UnionWith((await _indirectSiblingDeps[dependency]).Where(x => x != n));
            }
            return sibDeps;
        }
        public void Build()
        {
            foreach (var pair in _indirectSiblingDeps)
            {
                //pair.Key.IndirectSiblingDependencies = pair.Value.Result;
            }
        }
    }


    public class IndirectSiblingBuilder
    {
        public static HashSet<Node> BuildDepsFor(Node n, ISet<Node> exclude = null)
        {
            if (exclude == null)
                exclude = new HashSet<Node> { n };
            else
                exclude.Add(n);
            var sibDeps = n.SiblingDependencies.Except(exclude).ToHashSet();
            foreach (var dependency in sibDeps.ToHashSet())
            {
                sibDeps.UnionWith(BuildDepsFor(dependency, exclude));
            }
            return sibDeps;
        }

        public static void BuildDepsFor(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                //node.IndirectSiblingDependencies = BuildDepsFor(node);
            }
        }

        /*public static void FindIndirectSiblingDependencies(Node node)
            {
                //var builder = new IndirectSiblingBuilder(node.DescendantNodes().ToHashSet());
                //builder.Build();
                foreach (var descendantNode in node.DescendantNodes())
                {
                    descendantNode.IndirectSiblingDependencies = BuildDepsFor(descendantNode);
                }
                /*var willBuildDepsFor = node.DescendantNodes().ToHashSet();
                while (willBuildDepsFor.Any())
                {
                    var curr = willBuildDepsFor.First();
                    var alldeps = new HashSet<Node>();
                    var toBeAdded = curr.SiblingDependencies;
                    while (toBeAdded.Any())
                    {
                        alldeps.UnionWith(toBeAdded);
                        toBeAdded = toBeAdded.SelectMany(x => x.SiblingDependencies).ToHashSet();
                    }
                    curr.IndirectSiblingDependencies = alldeps;
                    willBuildDepsFor.Remove(curr);
                }/
            }*/

        /*public static async Task<HashSet<Node>> SetupIndirectSiblingDeps(ISet<Node> siblingDependencies)
            {
                var allDeps = siblingDependencies.ToHashSet();
                foreach (var node in siblingDependencies)
                {
                    if(node.IndirectSiblingDependencies == null)
                        node.IndirectSiblingDependencies = await SetupIndirectSiblingDeps(node.SiblingDependencies);
                    allDeps.UnionWith(await node.IndirectSiblingDependencies);
                }
                return allDeps;
            }*/
    }
}