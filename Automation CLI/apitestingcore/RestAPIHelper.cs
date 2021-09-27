using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Automation.APITestingCore
{
    /// <summary>
    /// Helper for Rest api
    /// </summary>
    public class RestAPIHelper
    {
        private readonly IRestClient _restClient;

        public RestAPIHelper(string url)
        {
            _restClient = new RestClient(url);
        }

        /// <summary>
        /// Create a REST API request
        /// </summary>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <param name="method">request method as GET, POST, DELETE...</param>
        /// <param name="body">request body</param>
        /// <param name="dataFormat">format of request body</param>
        /// <returns></returns>
        private IRestRequest CreateRequest(string endPoint, Dictionary<string, string> headers, Method method, object body = null, DataFormat dataFormat = DataFormat.None)
        {
            IRestRequest restRequest = new RestRequest()
            {
                Method = method,
                Resource = endPoint
            };

            if (headers != null)
            {
                headers.Keys.ToList().ForEach(key => restRequest.AddHeader(key, headers[key]));
            }

            if (body != null)
            {
                switch (dataFormat)
                {
                    case DataFormat.Json:
                        restRequest.AddJsonBody(body);
                        break;

                    case DataFormat.Xml:
                        restRequest.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        restRequest.AddParameter("xmlData", body.GetType().Equals(typeof(string)) ? body :
                        restRequest.XmlSerializer.Serialize(body), ParameterType.RequestBody);
                        break;

                    case DataFormat.None:
                        // process for application/x-www-form-urlencoded
                        restRequest.AddObject(body);
                        break;
                }
            }
            return restRequest;
        }

        /// <summary>
        /// Send a REST API request to server
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="restRequest">api request</param>
        /// <returns></returns>
        private IRestResponse<T> SendRequest<T>(IRestRequest restRequest) where T : new()
        {
            IRestResponse<T> restResponse = _restClient.Execute<T>(restRequest);

            if (restResponse.ContentType.Equals("application/xml"))
            {
                var deserializer = new RestSharp.Deserializers.DotNetXmlDeserializer();
                restResponse.Data = deserializer.Deserialize<T>(restResponse);
            }
            else if (restResponse.ContentType.Contains("application/json"))
            {
                restResponse.Data = JsonConvert.DeserializeObject<T>(restResponse.Content);
            }
            else 
            { 
                restResponse.Data = JsonConvert.DeserializeObject<T>(restResponse.Content);
            }
            return restResponse;
        }       
        
        /// <summary>
        /// The GET request that was made to get response
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <returns></returns>
        public IRestResponse<T> Get<T>(string endPoint, Dictionary<string, string> headers) where T : new()
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.GET);
            IRestResponse<T> restResponse = SendRequest<T>(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The POST request that was made to get response
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <param name="body">request body</param>
        /// <param name="dataFormat">format of request body</param>
        /// <returns></returns>
        public IRestResponse<T> Post<T>(string endPoint, Dictionary<string, string> headers, object body, DataFormat dataFormat = DataFormat.Json) where T : new()
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.POST, body, dataFormat);
            IRestResponse<T> restResponse = SendRequest<T>(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The PUT request that was made to get response
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <param name="body">request body</param>
        /// <param name="dataFormat">format of request body</param>
        /// <returns></returns>
        public IRestResponse<T> Put<T>(string endPoint, Dictionary<string, string> headers, object body, DataFormat dataFormat = DataFormat.Json) where T : new()
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.PUT, body, dataFormat);
            IRestResponse<T> restResponse = SendRequest<T>(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The DELETE request that was made to get response
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <returns></returns>
        public IRestResponse<T> Delete<T>(string endPoint, Dictionary<string, string> headers) where T : new()
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.DELETE);
            IRestResponse<T> restResponse = SendRequest<T>(restRequest);
            return restResponse;
        }       

        /// <summary>
        /// Send a REST API request to server
        /// </summary>
        /// <param name="restRequest">api request</param>
        /// <returns></returns>
        private IRestResponse SendRequest(IRestRequest restRequest)
        {
            IRestResponse restResponse = _restClient.Execute(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The GET request that was made to get response
        /// </summary>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <returns></returns>
        public IRestResponse Get(string endPoint, Dictionary<string, string> headers)
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.GET);
            IRestResponse restResponse = SendRequest(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The POST request that was made to get response
        /// </summary>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <param name="body">request body</param>
        /// <param name="dataFormat">format of request body</param>
        /// <returns></returns>
        public IRestResponse Post(string endPoint, Dictionary<string, string> headers, object body, DataFormat dataFormat = DataFormat.Json)
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.POST, body, dataFormat);
            IRestResponse restResponse = SendRequest(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The PUT request that was made to get response
        /// </summary>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <param name="body">request body</param>
        /// <param name="dataFormat">format of request body</param>
        /// <returns></returns>
        public IRestResponse Put(string endPoint, Dictionary<string, string> headers, object body, DataFormat dataFormat = DataFormat.Json)
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.PUT, body, dataFormat);
            IRestResponse restResponse = SendRequest(restRequest);
            return restResponse;
        }

        /// <summary>
        /// The DELETE request that was made to get response
        /// </summary>
        /// <param name="endPoint">api endpoint</param>
        /// <param name="headers">request header</param>
        /// <returns></returns>
        public IRestResponse Delete(string endPoint, Dictionary<string, string> headers)
        {
            IRestRequest restRequest = CreateRequest(endPoint, headers, Method.DELETE);
            IRestResponse restResponse = SendRequest(restRequest);
            return restResponse;
        }
    }
}
