using Automation_NCD_CLI.Models;
using Automation_NCD_CLI.TestResources;
using EPiServer.Automation.APITestingCore;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Automation_NCD_CLI.APIControllers
{
    public class APIControllerBase : ControllerBase
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        public APIControllerBase()
        {
            restClient = new RestAPIHelper(ConfigurationResource.ManagementSiteUrl);
            GetToken();
        }

        /// <summary>
        /// Call API to get token before running test scripts
        /// </summary>
        /// <returns></returns>
        public void GetToken()
        {
            headers["Content-Type"] = "application/x-www-form-urlencoded";
            ClientConfig clientConfig = new ClientConfig();
            var responsePost = restClient.Post<Token>(ConfigurationResource.TokenEndPoint, headers, clientConfig, RestSharp.DataFormat.None);
            Token = responsePost.StatusCode.Equals(HttpStatusCode.OK) ? "Bearer " + responsePost.Data.Access_token : string.Empty;
            headers["Content-Type"] = "application/json";
        }

        /// <summary>
        /// Update header by jointing the default header with additionalHeader
        /// </summary>
        /// <param name="additionalHeaders">Additional Headers</param>
        /// <returns>New headers dictionary</returns>
        public Dictionary<string, string> UpdateHeader(Dictionary<string, string> additionalHeaders)
        {
            if (additionalHeaders == null)
                additionalHeaders = headers;
            else
            {
                foreach (string key in headers.Keys)
                {
                    if (!additionalHeaders.ContainsKey(key))
                        additionalHeaders.Add(key, headers[key]);
                }
            }
            return additionalHeaders;
        }

        /// <summary>
        /// Update the request url includes parameters
        /// </summary>
        /// <param name="requestUrl">Request url</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Updated request url</returns>
        public string UpdateRequestUrl(string requestUrl, Dictionary<string, string> parameters)
        {
            if (parameters != null)
            {
                requestUrl += "?";
                foreach (string key in parameters.Keys)
                {
                    requestUrl += key + "=" + parameters[key] + "&";
                }
                requestUrl = requestUrl.Substring(0, requestUrl.Length - 1);
            }
            return requestUrl;
        }
    }
}
