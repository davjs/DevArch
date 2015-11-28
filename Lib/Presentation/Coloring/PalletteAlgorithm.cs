using System.Collections.Generic;
using System.Linq;
using Presentation.Coloring.ColoringAlgorithms;

namespace Presentation.Coloring
{
    public interface IPalletteAlgorithm
    {
        Stack<IColorData> GetDistinctColors(IColorData parentData, int slices);
        IColorData GetStartingColorData();
        IColorData GetSubColor(IColorData parentData);
    }

    public interface IColorData
    {
        AdvancedColor GetColor();
    }

    public abstract class PalletteAlgorithm<T> : IPalletteAlgorithm where T : class, IColorData
    {
        public Stack<IColorData> GetDistinctColors(IColorData parentData, int slices)
        {
            return new Stack<IColorData>(GetDistinctColorsImplementation(parentData as T,slices));
        }

        public IColorData GetStartingColorData()
        {
            return GetStartingColorDataImplementation();
        }

        public IColorData GetSubColor(IColorData parentData)
        {
            return GetSubColorImplementation(parentData as T);
        }
        protected abstract T GetSubColorImplementation(T parentData);

        protected abstract T GetStartingColorDataImplementation();

        protected abstract IEnumerable<T> GetDistinctColorsImplementation(T parentData, int slices);
    }
}