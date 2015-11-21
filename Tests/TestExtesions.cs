using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Building.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    static class TestExtesions
    {
        public static void RemoveChild(this Tree tree, string name)
        {
            var withName = tree.Childs.WithName(name);
            if (withName == null)
                throw new ChildNotFoundException(name);
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
