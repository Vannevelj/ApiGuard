using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies
{
    internal class BestGuessEndpointMatchingStrategy : IEndpointMatchingStrategy
    {
        public EndpointResult GetEndpoint(List<Endpoint> existingEndpoints, Endpoint otherEndpoint)
        {
            var differencesWithTargetPoint = new Dictionary<Endpoint, int>();

            foreach (var endpoint in existingEndpoints)
            {
                var differences = 0;
                var locations = new List<string>();
                Compare(endpoint, otherEndpoint, ref differences, locations);
                differencesWithTargetPoint.Add(endpoint, differences);
            }

            var minimalDifference = differencesWithTargetPoint.OrderBy(x => x.Value).FirstOrDefault();
            return new EndpointResult
            {
                Endpoint = minimalDifference.Key,
                IsExactMatch = minimalDifference.Value == 0
            };
        }

        private void Compare(Endpoint endpoint, Endpoint otherEndpoint, ref int counter, List<string> locations)
        {
            Compare(endpoint.MethodName, otherEndpoint.MethodName, ref counter, locations);
            Compare(endpoint.ReturnType, otherEndpoint.ReturnType, ref counter, locations);
            Compare(endpoint.Parameters, otherEndpoint.Parameters, ref counter, locations);
            Compare(endpoint.Attributes, otherEndpoint.Attributes, ref counter, locations);
        }

        private bool Compare<T>(T obj1, T obj2, ref int counter, List<string> locations)
        {
            if (!EqualityComparer<T>.Default.Equals(obj1, obj2))
            {
                counter++;
                return false;
            }

            return true;
        }

        private void Compare(List<MyType> types1, List<MyType> types2, ref int counter, List<string> locations)
        {
            if (!Compare(types1.Count, types2.Count, ref counter, locations))
            {
                return;
            }

            for (int i = 0; i < types1.Count; i++)
            {
                var type1 = types1[i];
                var type2 = types2[i];
                Compare(type1, type2, ref counter, locations);
            }
        }

        private void Compare(MyType type1, MyType type2, ref int counter, List<string> locations)
        {
            Compare(type1.Typename, type2.Typename, ref counter, locations);
            Compare(type1.NestedElements, type2.NestedElements, ref counter, locations);
        }

        private void Compare(List<IElement> type1, List<IElement> type2, ref int counter, List<string> locations)
        {
            if (!Compare(type1.Count, type2.Count, ref counter, locations))
            {
                return;
            }

            for (int i = 0; i < type1.Count; i++)
            {
                var element1 = type1[i];
                var element2 = type2[i];
                Compare(element1, element2, ref counter, locations);
            }
        }

        private void Compare(MyProperty property1, MyProperty property2, ref int counter, List<string> locations)
        {
            Compare(property1.Name, property2.Name, ref counter, locations);
            Compare(property1.Type, property2.Type, ref counter, locations);
        }

        private void Compare(MyMethod method1, MyMethod method2, ref int counter, List<string> locations)
        {
            Compare(method1.Name, method2.Name, ref counter, locations);
            Compare(method1.ReturnType, method2.ReturnType, ref counter, locations);
            Compare(method1.Parameters, method2.Parameters, ref counter, locations);
        }

        private void Compare(List<MyParameter> params1, List<MyParameter> params2, ref int counter, List<string> locations)
        {
            if (!Compare(params1.Count, params2.Count, ref counter, locations))
            {
                return;
            }

            for (int i = 0; i < params1.Count; i++)
            {
                var param1 = params1[i];
                var param2 = params2[i];
                Compare(param1, param2, ref counter, locations);
            }
        }

        private void Compare(MyParameter param1, MyParameter param2, ref int counter, List<string> locations)
        {
            Compare(param1.Ordinal, param2.Ordinal, ref counter, locations);
            Compare(param1.Type, param2.Type, ref counter, locations);
        }
    }
}
