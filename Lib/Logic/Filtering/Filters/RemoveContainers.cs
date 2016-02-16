using System;
using System.Collections.Generic;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class RemoveContainers : Filter
    {
        public RemoveContainers(bool i) : base(i, Apply)
        {
        }

        private static IEnumerable<ClassNode> FindClasses(Node tree)
        {
            foreach (var child in tree.Childs)
            {
                if (child is ClassNode)
                    yield return child as ClassNode;
                else
                {
                    foreach (var node in FindClasses(child))
                        yield return node;
                }
            }
        }

        private new static void Apply(Node tree)
        {
            tree.SetChildren(FindClasses(tree));
        }
    }
}