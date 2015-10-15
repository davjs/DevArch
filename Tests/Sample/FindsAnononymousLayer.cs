namespace Analysis.Tests.Sample
{
    class FindsAnononymousLayer
    {
        class A
        {
        }
    
        class B
        {
            public A UsesA;
        }

        class C
        {
            public A UsesA;
        }

    }
}
