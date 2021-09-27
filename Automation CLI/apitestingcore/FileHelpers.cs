using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EPiServer.Automation.APITestingCore
{
    /// <summary>
    /// File methods
    /// </summary>
    public class FileHelpers : HelperBase<FileHelpers>
    {
        /// <summary>
        /// Read default.profile.json and return configuration settings
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetConfigValue()
        {
            var executingFolderLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configFilePath = Path.Combine(executingFolderLocation, @"Config\default.profile.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(configFilePath));
        }
    }
}