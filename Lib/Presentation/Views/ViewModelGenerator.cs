using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Presentation.ViewModels;

namespace Presentation.Views
{
    public static class ViewModelGenerator
    {
        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        public static IDiagramControl CreateViewFromViewModel(DiagramSymbolViewModel x)
        {
            if(x is LayerViewModel) return new LayerView((LayerViewModel) x);
            if (x is ArrowViewModel) return new ArrowView((ArrowViewModel) x);
            return null;
        }
    }
}