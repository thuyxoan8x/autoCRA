using System.Collections.Generic;
using System.Dynamic;

namespace EPiServer.Automation.APITestingCore
{
    /// <summary>
    /// Extention for Expando Object
    /// </summary>
    public static class ExpandoObjectExtensions
    {
        /// <summary>
        /// Add new field to expando object
        /// </summary>
        public static void Add(this ExpandoObject target, string key, object value)
        {
            dynamic d = target;
            var byName = (IDictionary<string, object>)d;
            byName[key] = value;
        }
    }
    /// <summary>
    /// Common methods for api controllers
    /// </summary>
    public abstract class ControllerBase
    {
        public RestAPIHelper restClient;

        /// <summary>
        /// API header
        /// </summary>
        public Dictionary<string, string> headers = new Dictionary<string, string>
        {
            {"Content-Type", "application/json"}
        };       
    }
}
