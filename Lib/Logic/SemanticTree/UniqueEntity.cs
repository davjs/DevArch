using System.Linq;

namespace Logic.SemanticTree
{
    public class UniqueEntity
    {
        public readonly int Id;
        private static int _lastId;

        protected UniqueEntity()
        {
            Id = _lastId;
            _lastId++;
        }
    }
}