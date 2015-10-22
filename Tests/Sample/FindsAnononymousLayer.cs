#pragma warning disable 169
namespace Tests.Sample
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
