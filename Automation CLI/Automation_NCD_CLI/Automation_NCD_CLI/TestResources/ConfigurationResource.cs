using EPiServer.Automation.APITestingCore;
using System.Collections.Generic;

namespace Automation_NCD_CLI.TestResources
{
    /// <summary>
    /// Configuration in default.profile.json file
    /// </summary>
    class ConfigurationResource
    {
        #region Configs in default.profile.json
        /// <summary>
        /// Keeping configuration settings in default.profile.json
        /// </summary>
        private static readonly Dictionary<string, string> ConfigValue = FileHelpers.Current.GetConfigValue();
        /// <summary>
        /// Management Site Url
        /// </summary>
        public static string ManagementSiteUrl = ConfigValue["managementSiteUrl"];
        /// <summary>
        /// Cli Tool Name
        /// </summary>
        public static string CliToolName = ConfigValue["cliToolName"];
        /// <summary>
        /// Push Working Directory
        /// </summary>
        public static string PushWorkingDirectory = ConfigValue["pushWorkingDirectory"];
        /// <summary>
        /// Pull Working Directory
        /// </summary>
        public static string PullWorkingDirectory = ConfigValue["pullWorkingDirectory"];
        /// <summary>
        /// Export Working Directory
        /// </summary>
        public static string ExportWorkingDirectory = ConfigValue["exportWorkingDirectory"];
        /// <summary>
        /// Export Working Directory
        /// </summary>
        public static string MergeWorkingDirectory = ConfigValue["mergeWorkingDirectory"];
        /// <summary>
        /// CDA Content Type Endpoint
        /// </summary>
        public static string CDA_ContentTypeEndpoint = ConfigValue["contentTypeEndpoint"];
        /// <summary>
        /// Token Endpoint
        /// </summary>
        public static string TokenEndPoint = ConfigValue["tokenEndPoint"];
        /// <summary>
        /// User name
        /// </summary>
        public static string Username = ConfigValue["username"];
        /// <summary>
        /// Password
        /// </summary>
        public static string Password = ConfigValue["password"];
        /// <summary>
        /// Client
        /// </summary>
        public static string Client = ConfigValue["client"];
        /// <summary>
        /// Secret
        /// </summary>
        public static string Secret = ConfigValue["secret"];
        #endregion


    }
}
