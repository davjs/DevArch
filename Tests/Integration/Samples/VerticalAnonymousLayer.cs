// ReSharper disable All
#pragma warning disable 169

namespace Tests.Integration.Samples
{
    namespace VerticalAnonymousLayer
    {
            class TopLeft
            {

            }
            class BottomLeft
            {
                private TopLeft top;
            }

            class TopRight
            {

            }
            class MidRight
            {
                private TopRight _topRight;
            }
            class BottomRight
            {
                private MidRight _midRight;
            }


    }
}
