using System;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Tests
{
    public class DisposableResult : IDisposable
    {
        public AdhocWorkspace Workspace;
        public ProjectNode result;
        public void Dispose()
        {
            Workspace.Dispose();
        }
    }
}