using EPiServer.Automation.APITestingCore;
using EPiServer.Automation.commercehapi.Models;
using Automation_NCD_CLI.Models;
using Automation_NCD_CLI.TestResources;
using Newtonsoft.Json;
using System.Net;
using RestSharp;

namespace Automation_NCD_CLI.APIControllers
{
    public class CDA_ContentTypeControllers : APIControllerBase
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        public CDA_ContentTypeControllers()
        {
            restClient = new RestAPIHelper(ConfigurationResource.ManagementSiteUrl);
            if (!string.IsNullOrEmpty(Token))
                headers.Add("Authorization", Token);
        }


        /// <summary>
        /// Delete order by Content type
        /// </summary>
        /// <param name="guid">Content type guid value</param>
        /// <returns>Status code</returns>
        public string DeleteContentType(string guid)
        {
            var responseDelete = restClient.Delete(ConfigurationResource.CDA_ContentTypeEndpoint + $"{guid}", headers);
            return responseDelete.StatusCode.ToString();
        }

        /// <summary>
        /// Get content types
        /// </summary>
        /// <returns>request response</returns>
        public IRestResponse GetContentTypes()
        {
            return restClient.Get(ConfigurationResource.CDA_ContentTypeEndpoint, headers);
        }
    }
}
