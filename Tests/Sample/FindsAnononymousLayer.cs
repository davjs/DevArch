#pragma warning disable 169
namespace Analysis.Tests.Sample
{
    class FindsAnononymousLayer
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
