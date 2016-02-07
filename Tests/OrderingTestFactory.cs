using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.String;

namespace Tests
{
    internal static class OrderingTestFactory
    {
        public static HashSet<Node> CreateNodeList(string nodeCreationQuery)
        {
            nodeCreationQuery = nodeCreationQuery.Trim();
            nodeCreationQuery = Regex.Replace(nodeCreationQuery,@"[ \r]", "");
            var entries = nodeCreationQuery.Split('\n').ToList();
            var splitEntries =
                entries.Select(x => x.Split(new[] {"->"}, StringSplitOptions.RemoveEmptyEntries));

            var nodes = splitEntries.Select(x => new Node(x[0])).ToList();

            var i = 0;
            foreach (var dependencyString in splitEntries.Select(x => x.Length == 2 ? x[1] : null))
            {
                if (dependencyString != null)
                {
                    var dependencies = dependencyString.Split(',');
                    foreach (var dependency in dependencies)
                    {
                        var dependencyNode = nodes.WithName(dependency);
                        if(dependencyNode == nodes[i])
                            throw new Exception("Can not be dependant on itself");
                        if (dependencyNode == null)
                            throw new NodeNotFoundInListException(dependency);
                    }
                    var nodeDependencies = dependencies.Select(x => nodes.WithName(x));
                    nodes[i].SiblingDependencies = new HashSet<Node>(nodeDependencies);
                }
                i++;
            }
                
            return new HashSet<Node>(nodes);
        }

        public static void AssertLayout(Node expected, Node actual)
        {
                
            if(expected.Childs.Count != actual.Childs.Count)
                throw new AssertFailedException($"Expected {expected.Name ?? actual.Name} to have {expected.Childs.Count} childs got {actual.Childs.Count}\n" +
                                                "Expected:\n" +
                                                $" {expected}\n " +
                                                "--------------------\nGot:\n" +
                                                $" {actual}");
            foreach (var child in expected.Childs)
            {
                var foundChild = false;
                if (IsNullOrEmpty(child.Name))
                {
                    if (actual.Childs.Any(x => CompareNodesByLayout(child, x)))
                        foundChild = true;
                }
                else
                {
                    if (actual.Childs.WithName(child.Name) != null)
                        foundChild = true;
                }
                if(!foundChild)
                    throw new AssertFailedException($"{expected.Name ?? actual.Name} did not have child {child} got {actual.Childs}\n" +
                                                    "Expected:\n" +
                                                    $" {expected}\n " +
                                                    "--------------------\nGot:\n" +
                                                    $" {actual}");
            }
        }

        private static bool CompareNodesByLayout(Node expected, Node actual)
        {
            foreach (var child in expected.Childs)
            {
                if (IsNullOrEmpty(child.Name))
                {
                    if (!actual.Childs.Any(x => CompareNodesByLayout(child, x)))
                        return false;
                }
                else
                {
                    if (actual.Childs.WithName(child.Name) == null)
                        return false;
                }
            }
            return true;
        }
    }

    internal class NodeNotFoundInListException : Exception
    {
        public NodeNotFoundInListException(string dependency) : base(dependency) 
        {
        }
    }
}