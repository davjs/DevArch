using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
