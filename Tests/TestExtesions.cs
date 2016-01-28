using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    static class TestExtesions
    {
        public static void RemoveChild(this Node tree, string name)
        {
            var withName = tree.Childs.WithName(name);
            if (withName == null)
                throw new ChildNotFoundException(name);
            foreach (var child in tree.Childs.Where(child => child.Dependencies.Contains(withName)))
            {
                child.Dependencies.Remove(withName);
            }
            tree.RemoveChild(withName);
        }
    }

    internal class ChildNotFoundException : Exception
    {
        public ChildNotFoundException(string message) : base(message)
        {
        }
    }
}
