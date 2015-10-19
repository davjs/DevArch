#pragma warning disable 169
namespace Analysis.Tests.Sample
{
    namespace FindsAnononymousLayer
    {
        class A
        {
        }
    
        class B
        {
            A _usesA;
        }

        class C
        {
            A _usesA;
        }

    }
}
