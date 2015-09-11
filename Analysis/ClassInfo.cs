using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Analysis
{
    public class ClassInfo
    {
        public INamespaceSymbol NameSpace;
        public IEnumerable<ReferencedSymbol> References;
        public ISymbol Symbol;
    }
}