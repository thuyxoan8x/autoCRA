using Automation_NCD_CLI.APIControllers;
using Automation_NCD_CLI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automation_NCD_CLI.Scenarios
{
    /// <summary>
    /// Tests base for ProfileStoreRestAPI project
    /// </summary>
    public class TestsBase
    {
        private static Lazy<CDA_ContentTypeControllers> cDA_ContentTypeControllers = new Lazy<CDA_ContentTypeControllers>(() => new CDA_ContentTypeControllers());
        private static Lazy<ManifestControllers> manifestControllers = new Lazy<ManifestControllers>(() => new ManifestControllers());
        
        /// <summary>
        /// Get Content Type Controllers
        /// </summary>
        public CDA_ContentTypeControllers CDA_ContentTypeControllers => cDA_ContentTypeControllers.Value;
        /// <summary>
        /// Manifest Controller
        /// </summary>
        public ManifestControllers ManifestControllers => manifestControllers.Value;

        /// <summary>
        /// Build a filter string
        /// </summary>
        /// <typeparam name="T">Type of value, may be string, datetime, int, bool</typeparam>
        /// <param name="filters">List set of: property, operator (eq,ne...) and value</param>
        /// <param name="operatorConnections">can be and/or</param>
        /// <returns></returns>
        public string BuildFilterQuery<T>(List<Tuple<string, string, T>> filters, string operatorConnections = "and")
        {
            string filterString = "&$filter=";

            foreach (var filter in filters)
            {
                filterString += $"{filter.Item1} {filter.Item2} ";
                var value = (filter.Item3.GetType().Name == "String") ? $"'{filter.Item3}'" : filter.Item3.ToString();
                filterString += value;
                filterString += filter.Equals(filters.Last()) ? string.Empty : $" {operatorConnections} ";
            }
            return filterString;
        }

        /// <summary>
        /// /Create tuple list preparing for BuildFilterQuery
        /// </summary>
        /// <param name="property">property is used to query</param>
        /// <param name="valueList">list of value</param>
        /// <param name="propertyOperator">operator is used to query, it can be: eq, ne, lt, le, gt...</param>
        /// <returns></returns>
        public List<Tuple<string, string, string>> CreateTupleStringByPropertyList(string property, List<string> valueList, string propertyOperator = "eq")
        {
            return valueList.Select(x => Tuple.Create(property, propertyOperator, x)).ToList();
        }

       
        
    }
}