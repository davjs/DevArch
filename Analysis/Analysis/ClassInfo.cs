using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Logic.Analysis
{
    public class ClassInfo
    {
        public INamespaceSymbol NameSpace;
        public IEnumerable<ReferencedSymbol> References;
        public ISymbol Symbol;
        public IEnumerable<TypeSyntax> BaseTypes { get; set; }
    }
}