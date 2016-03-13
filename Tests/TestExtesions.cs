﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnvDTE;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public static class TestExtesions
    {
        public static class TestSolutions
        {
            private static readonly string RepoDir = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\";
            public static readonly string DevArchSln = RepoDir + "\\DevArch.sln";
            private static readonly string SampleSolutions = RepoDir + "\\Tests\\TestSolutions\\";
            public static readonly string WithSolFolders = SampleSolutions + "WithSolFolders\\WithSolFolders.sln";
            public static readonly string WithNestedFolders = SampleSolutions + "WithNestedFolders\\WithNestedFolders.sln";
        }

        public static void RemoveChild(this Node tree, string name)
        {
            var withName = tree.Childs.WithName(name);
            if (withName == null)
                throw new ChildNotFoundException(tree.ToString(),name);
            foreach (var child in tree.Childs.Where(child => child.Dependencies.Contains(withName)))
            {
                child.Dependencies.Remove(withName);
            }
            tree.RemoveChild(withName);
        }

        internal class ChildNotFoundException : Exception
        {
            public ChildNotFoundException(string parent, string child) : base($"unable to find {child} in {parent}")
            {
            }
        }

        public static string SubStrBetween(this string str, int startIndex,int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static DTE Dte => (DTE)Marshal.GetActiveObject("VisualStudio.DTE.14.0");
        public static readonly VisualStudio TestStudio = new VisualStudio(Dte);
        public static readonly DevArchSolution TestSolution = TestStudio.Solution;
        public static readonly DevArchSolution StandAloneSolution = DevArchSolution.FromPath(TestSolutions.DevArchSln);
        public static readonly string SlnDir = TestStudio.Solution.Directory;


        public static Node BuildTree(this string text)
        {
            text = Regex.Replace(text, @"\s", "");
            var root = new Node("Tree");
            root.SetChildren(_BuildTree(text));
            return root;
        }

        private static IEnumerable<Node> _BuildTree(this string text)
        {
            var nestedChildStart = 0;
            var depth = 0;
            var currentChildStartIndex = 0;
            var childEntries = new List<string>();
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '[')
                {
                    if (depth == 0)
                        nestedChildStart = i;
                    depth++;
                }
                if (c == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        childEntries.Add(text.SubStrBetween(nestedChildStart, i + 1));
                        currentChildStartIndex = i + 1;
                    }
                }
                else if (c == ',' && depth == 0)
                {
                    childEntries.Add(text.SubStrBetween(currentChildStartIndex, i));
                    currentChildStartIndex = i + 1;
                }
            }
            if(currentChildStartIndex != text.Length)
                childEntries.Add(text.SubStrBetween(currentChildStartIndex, text.Length));

            foreach (var entry in childEntries)
            {
                if (entry.StartsWith("[") && entry.EndsWith("]"))
                {
                    var node = new Node("");
                    node.SetChildren(_BuildTree(entry.SubStrBetween(1, entry.Length - 1)));
                    yield return node;
                }
                else
                {
                    yield return new Node(entry);
                }
            }

        }

        public static class TreeAssert
        {

            public static void DoesNotContainDuplicates(Node tree)
            {
                DoesNotContainDuplicates(tree.Childs);
            }

            public static void DoesNotContainDuplicates(IEnumerable<Node> tree)
            {
                var allNodes = tree.SelectMany(x => x.DescendantNodes()).Where(x => !string.IsNullOrEmpty(x.Name));

                var dups = allNodes.GroupBy(x => x)
                            .Where(x => x.Count() > 1)
                            .Select(x => x.Key)
                            .ToList();

                if(dups.Any())
                    throw new AssertFailedException($"Got {dups.Count} duplicates:" +
                                                    $"{string.Join(",",dups)}");
            }
        }
    }

}
