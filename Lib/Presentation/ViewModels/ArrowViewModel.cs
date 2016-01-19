using System;

namespace Presentation.ViewModels
{
    public class ArrowViewModel : DiagramSymbolViewModel
    {
        public enum Direction
        {
            Left,Right,Up,Down
        }

        public int Angle;

        public ArrowViewModel(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    Angle = 180;
                    break;
                case Direction.Right:
                    Angle = 0;
                    break;
                case Direction.Up:
                    Angle = 270;
                    break;
                case Direction.Down:
                    Angle = 90;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}