using Automation_NCD_CLI.TestResources;

namespace Automation_NCD_CLI.Models
{
    /// <summary>
    /// Token
    /// </summary>
    public class Token
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
        public string Refresh_token { get; set; }
        public string Client_id { get; set; }
        public string Username { get; set; }
        public string Issued { get; set; }
        public string Expires { get; set; }
    }

    /// <summary>
    /// Client Config
    /// </summary>
    public class ClientConfig
    {
        public string client_id { get; set; } = ConfigurationResource.Client;
        public string client_secret { get; set; } = ConfigurationResource.Secret;
        public string grant_type { get; set; } = "password";
        public string username { get; set; } = ConfigurationResource.Username;
        public string password { get; set; } = ConfigurationResource.Password;
        public string scope { get; set; } = "epi_content_definitions";
    }
}
