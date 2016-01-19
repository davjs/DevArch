// ReSharper disable All
#pragma warning disable 169
using System.Collections.Generic;

namespace Tests.Sample
{
    namespace Patterns
    {
        class Domain
        {
            public class CurrencyModel
            {

            }

            public class FuelCostModel
            {

            }
        }

        class DataAccess
        {
            class Currencies
            {
                private List<Domain.CurrencyModel> _currencies;

            }
            class FuelCosts
            {
                private List<Domain.FuelCostModel> _FuelCosts;

            }
        }

        class Business
        {
            private DataAccess dataAccess;
            class CurrencyService
            {

            }
            class FuelCostService
            {

            }
            class CalculatorService
            {

            }
        }

        class Presentation
        {
            private Business _business;
            class FuelCostController
            {

            }
            class CurrencyController
            {

            }
            class CalculatorController
            {

            }

            class CalculationViewModel
            {

            }
            class CalculationView
            {

            }
            class SettingsView
            {

            }
            class SettingsViewModel
            {

            }
        }
    }
}
