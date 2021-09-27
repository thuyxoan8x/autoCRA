using Automation_NCD_CLI.CLIControllers;
using Automation_NCD_CLI.Models;
using Automation_NCD_CLI.TestResources;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Automation_NCD_CLI.Scenarios
{
    class PullTests : TestsBase
    {
        private string outputFile = "";

        [Test, Description("TC01 Pull to terminal command line, TC03 Pull with authority")]
        [Category("Pull")]
        public void TC01_Pull_to_command_line()
        {
            string result = ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains("contentTypes"));
            Assert.IsTrue(result.Contains("editorDefinitions"));
            Assert.IsTrue(result.Contains("propertyGroups"));
        }

        [Test, Description("TC02 Pull to output file, TC 04 Pull all content manifest includes pushed  data")]
        [Category("Pull")]
        public void TC02_Pull_to_output_file()
        {
            outputFile = "pull_TC2.json";
            string result = ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            var outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));

            Assert.IsNotEmpty(pullResponse.ContentTypes);
            Assert.IsNotNull(pullResponse.ContentTypes.Where(x => x.Name == "MyContentTypeNoVersion_preparation").FirstOrDefault());
            Assert.IsEmpty(pullResponse.EditorDefinitions);
            Assert.IsNotEmpty(pullResponse.PropertyGroups);
        }

        [Test, Description("Pull without source")]
        [Category("Pull")]
        public void TC07_Pull_without_source()
        {
            string result = ManifestControllers.ExecutePullWithDefaultAuthority("");

            Assert.IsTrue(result.Contains(ResultMessages.ErrorOptionSourceIsRequired));
        }

        [Test, Description("Pull without argument value for @option")]
        [Category("Pull")]
        [TestCase("source")]
        [TestCase("authority")]
        [TestCase("client")]
        [TestCase("secret")]
        public void TC08_Pull_without_argument_value_for_option(string option)
        {
            string result = "";
            switch (option)
            {
                case "source":
                    result = ManifestControllers.ExecutePullWithDefaultAuthority("", extension: "--source");
                    break;
                case "authority":
                    result = ManifestControllers.ExecutePull(ConfigurationResource.ManagementSiteUrl, clientId: ConfigurationResource.Client, secretKey: ConfigurationResource.Secret, extension: "--authority");
                    break;
                case "client":
                    result = ManifestControllers.ExecutePull(ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, secretKey: ConfigurationResource.Secret, extension: "--client");
                    break;
                case "secret":
                    result = ManifestControllers.ExecutePull(ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, clientId: ConfigurationResource.Client, extension: "--secret");
                    break;
                default:
                    break;
            }

            Assert.IsTrue(result.Contains(ResultMessages.ErrorRequiredArgumentMissingForOption(option)));
        }

        [Test, Description("Pull with invalid source")]
        [Category("Pull")]
        [TestCase("http://epvnwrkming:8001")]  // source doesn't not exist
        [TestCase("invalid")]
        [TestCase("http://epvnwrkming:9000")] // source doen't support
        public void TC09_Pull_with_invalid_source(string invalidSource)
        {
            string result = ManifestControllers.ExecutePullWithDefaultAuthority(invalidSource);

            switch (invalidSource)
            {
                case "http://epvnwrkming:8001":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorNoConnectionCouldBeMade));
                    break;
                case "invalid":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorBaseAddressMustBeAnAbsoluteURI));
                    break;
                case "http://epvnwrkming:9000":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorTheServiceReturnedAnUnexpectedResponse));
                    break;
                default:
                    break;
            }
        }

        [Test, Description("Pull with invalid output")]
        [Category("Pull")]
        [TestCase("%$^*.json")] 
        [TestCase("C:\\EPI\\Stash\\EPiServer.ContentDelivery.AspNetCore\\samples1\\Alloy.DeliverySite\\CLI tool\\pull\\output.json")] 
        public void TC10_Pull_with_invalid_output(string invalidOutput)
        {
            string result = ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: invalidOutput);

            switch (invalidOutput)
            {
                case "%$^*.json":
                    Assert.IsTrue(result.Contains(ResultMessages.FileNameIncorrect));
                    break;
                default:
                    Assert.IsTrue(result.Contains(ResultMessages.CannotFindPath));
                    break;
            }
        }

        [Test, Description("Pull with invalid authority server")]
        [Category("Pull")]
        [TestCase("http://epvnwrkming:8001")]  // source doesn't not exist
        [TestCase("http://epvnwrkming:9000")] // not openID Connect server
        [TestCase("invalid")]
        public void TC11_Pull_with_invalid_authority_server(string invalidAuthority)
        {
            string result = ManifestControllers.ExecutePull(ConfigurationResource.ManagementSiteUrl, invalidAuthority, ConfigurationResource.Client, ConfigurationResource.Secret);

            switch (invalidAuthority)
            {
                case "http://epvnwrkming:8001":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorConnectingToServer(invalidAuthority)));
                    break;
                case "invalid": 
                    Assert.IsTrue(result.Contains("Malformed URL"));
                    break;
                case "http://epvnwrkming:9000":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorConnectingToServer(invalidAuthority)));
                    break;
                default:
                    break;
            }
        }

        [Test, Description("Pull with Invalid client/secret key")]
        [Category("Pull")]
        [TestCase("invalid", "cli")]
        [TestCase("cli", "invalid")]
        [TestCase("cli_noScope", "cli_noScope")] //valid client + secret key + invalid scope
        [TestCase("", "cli")]
        [TestCase("cli", "")]
        [TestCase("", "")]
        public void TC12_Pull_with_Invalid_client_secret_key(string client, string secret)
        {
            string result = ManifestControllers.ExecutePull(ConfigurationResource.ManagementSiteUrl, ConfigurationResource.ManagementSiteUrl, client, secret);

            if (string.IsNullOrEmpty(client) || string.IsNullOrEmpty(secret))
                Assert.IsTrue(result.Contains(ResultMessages.ErrorOptionAuthorityAlsoExpectsClientAndSecret));
            else if (client == "invalid")
                Assert.IsTrue(result.Contains(ResultMessages.ErrorTheSpecifiedClientIdIsInvalid));
            else if (secret == "invalid")
                Assert.IsTrue(result.Contains(ResultMessages.ErrorTheSpecifiedClientCredentialsAreInvalid));
            else
                Assert.IsTrue(result.Contains(ResultMessages.ErrorThisClientApplicationIsNotAllowed));
        }


        [SetUp]
        public void SetUp()
        {
            if (TestContext.CurrentContext.Test.MethodName == "TC02_Pull_to_output_file")
            {
                // Prepare non version content type
                ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_NoVersion_preparation.json", ConfigurationResource.ManagementSiteUrl);
            }

        }

        [TearDown]
        public void CleanUp()
        {
            // Remove content type
            string responseData = CDA_ContentTypeControllers.GetContentTypes().Content;
            List<ContentType> contentTypes = JsonConvert.DeserializeObject<List<ContentType>>(responseData);
            List<ContentType> testContetnTypes = contentTypes.Where(ct => ct.Name.StartsWith("MyContent")).ToList();
            foreach (ContentType ct in testContetnTypes)
                CDA_ContentTypeControllers.DeleteContentType(ct.Id);

            // Remove output file
            FileFolderHelper.EmptyDirectory(new DirectoryInfo(ConfigurationResource.PullWorkingDirectory));
        }
    }
}
