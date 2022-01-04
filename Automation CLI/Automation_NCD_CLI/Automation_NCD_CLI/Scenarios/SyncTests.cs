using Automation_NCD_CLI.CLIControllers;
using Automation_NCD_CLI.Models;
using Automation_NCD_CLI.TestResources;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation_NCD_CLI.Scenarios
{
    class SyncTests : TestsBase
    {
        private string outputFile = "";
        private string updatedProperty = "MyUpdatedProperty";
        private string addedProperty = "MyAddedProperty";
        private string removedProperty = "MyRemovedProperty";

        [Test, Description("Sync when no updating")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        public void TC1_Sync_with_Default_Essembly()
        {
            //Sync with default assembly before running test
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(18)));
        }

        [Test, Description("Sync after adding some content types to delivery site assembly")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        public void TC2_Sync_after_adding_some_content_types_to_delivery_site_assembly()
        {
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.AddedContentAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(21)));

            // Verify new content types are sync to Management site
            outputFile = "sync_pull.json";
            ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            var outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));

            Assert.IsNotNull(pullResponse.ContentTypes.Where(x => x.Name == "MyCodeCustomBlock").FirstOrDefault());
            Assert.IsNotNull(pullResponse.ContentTypes.Where(x => x.Name == "MyCodeCustomPage").FirstOrDefault());
            Assert.IsNotNull(pullResponse.ContentTypes.Where(x => x.Name == "MyCodeCustomMedia").FirstOrDefault());
        }
        
        [Test, Description("Sync after updating content type properties to delivery site/assembly")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        public void TC3_Sync_after_updating_content_type_properties_to_delivery_site_assembly()
        {
            // Preparation: sync new added content type to Management site
            // MyCodeCustomMedia has MyRemovedProperty property
            // MyCodeCustomPage has MyUpdatedProperty (string)
            // MyCodeCustomBlock does not have MyAddedProperty
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.AddedContentAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(21)));

            // Verify new content types and their properties are sync to Management site
            outputFile = "sync_pull.json";
            ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            var outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            ContentType contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomMedia")).FirstOrDefault();
           
            Assert.IsNotNull(contentType.Properties.Where(x => x.Name == removedProperty).FirstOrDefault());
            
            contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomPage")).FirstOrDefault();
            
            Assert.IsNotNull(contentType.Properties.Where(x => x.Name == updatedProperty & x.DataType == "PropertyString").FirstOrDefault());
            
            contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomBlock")).FirstOrDefault();
            
            Assert.IsNull(contentType.Properties.Where(x => x.Name == addedProperty).FirstOrDefault());

            // Sync content types with added/removed/updated properties
            result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.UpdatedContentAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(21)));

            // Verify new content types and their properties are sync to Management site
            outputFile = "sync2_pull.json";
            ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomMedia")).FirstOrDefault();

            Assert.IsNull(contentType.Properties.Where(x => x.Name == removedProperty).FirstOrDefault());

            contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomPage")).FirstOrDefault();

            Assert.IsNotNull(contentType.Properties.Where(x => x.Name == updatedProperty & x.DataType == "PropertyBoolean").FirstOrDefault());

            contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomBlock")).FirstOrDefault();

            Assert.IsNotNull(contentType.Properties.Where(x => x.Name == addedProperty).FirstOrDefault());
        }

        [Test, Description("Sync with option --use-assembly-versioning")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        public void TC4_Sync_with_option_use_assembly_versioning()
        {
            // Preparation: sync new added content type to Management site
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.AddedContentAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(21)));

            // Get assembliesVersion
            FileVersionInfo assembliesVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(ConfigurationResource.AddedContentAssembly, "Alloy.DeliverySite.dll"));
            string assembliesVersion = assembliesVersionInfo.FileVersion;

            // Verify new content types and their properties are sync to Management site
            outputFile = "sync_pull.json";
            ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            var outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            ContentType contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomPage")).FirstOrDefault();
            Assert.AreEqual(assembliesVersion, contentType.Version);

            // Sync content types with added/removed/updated properties
            result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.ChangeVersionAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(21)));

            // Get assembliesVersion
            assembliesVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(ConfigurationResource.ChangeVersionAssembly, "Alloy.DeliverySite.dll"));
            string newAssembliesVersion = assembliesVersionInfo.FileVersion;

            // Verify new content types and their properties are sync to Management site
            outputFile = "sync2_pull.json";
            ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            contentType = pullResponse.ContentTypes.Where(x => (x.Name == "MyCodeCustomMedia")).FirstOrDefault();
            Assert.AreEqual(newAssembliesVersion, contentType.Version);
            Assert.AreNotEqual(assembliesVersion, newAssembliesVersion);
        }

        [Test, Description("Sync with option -merge")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("merge_names.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1Properties2.json")]
        [TestCase("merge_TC3_name1Properties2.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1Id1.json")]
        [TestCase("merge_TC1_contentType1.json,merge_TC1_contentType1.txt,merge_TC1_contentType1")]
        public void TC5_Sync_with_option_merge(string mergingFiles)
        {
            List<string> files = mergingFiles.Split(',').ToList();

            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.ChangeVersionAssembly, files, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");

            // export assembly to export.json file
            outputFile = "export.json";
            ManifestControllers.ExecuteExport(ConfigurationResource.ChangeVersionAssembly, outputFile, "--use-assembly-versioning");
            // reorder as sync use merge reverse
            files.Reverse();
            files.Add(Path.Combine(ConfigurationResource.ExportWorkingDirectory, outputFile));

            // build the expected contenType list in merged order
            Dictionary<string, ContentType> expectedContentTypesDict = new Dictionary<string, ContentType>();
            foreach (string file in files)
            {
                var filePath = Path.Combine(ConfigurationResource.SyncWorkingDirectory, file);
                PullResponse fileContent = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(filePath));
                foreach (ContentType contentType in fileContent.ContentTypes)
                {
                    string key = contentType.Name;
                    if (expectedContentTypesDict.ContainsKey(key))
                    {
                        expectedContentTypesDict[key].Name = contentType.Name;
                        if (!string.IsNullOrEmpty(contentType.Id))
                            expectedContentTypesDict[key].Id = contentType.Id;
                        if (!string.IsNullOrEmpty(contentType.Version))
                            expectedContentTypesDict[key].Version = contentType.Version;
                        foreach (Property prop in contentType.Properties)
                        {
                            if (!expectedContentTypesDict[key].Properties.Where(x => x.Name == prop.Name).Any())
                                expectedContentTypesDict[key].Properties.Add(prop);
                            else
                            {
                                for (int i = 0; i < expectedContentTypesDict[key].Properties.Count; i++)
                                    if (expectedContentTypesDict[key].Properties[i].Name == prop.Name)
                                        expectedContentTypesDict[key].Properties[i] = prop;
                            }    
                        }
                    }
                    else
                        expectedContentTypesDict.Add(key, contentType);
                }
            }
            List<ContentType> expectedContentTypes = new List<ContentType>();
            foreach (ContentType ct in expectedContentTypesDict.Values)
                if (ct.Name.StartsWith("MyCode") || ct.Name.StartsWith("MyContent"))
                    expectedContentTypes.Add(ct);
            
            // Verify new content types are sync to Management site
            outputFile = "sync_pull.json";
            ManifestControllers.ExecutePullWithDefaultAuthority(ConfigurationResource.ManagementSiteUrl, output: outputFile);
            var outputPath = Path.Combine(ConfigurationResource.PullWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));

            foreach (ContentType ct in expectedContentTypes)
            {
                ContentType c = pullResponse.ContentTypes.Where(x => x.Name == ct.Name).FirstOrDefault();
                c.Id = null;
                ct.Id = null;
                Assert.AreEqual(JsonConvert.SerializeObject(ct), JsonConvert.SerializeObject(c));
            }
        }

        [Test, Description("Sync with Invalid path")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("invalid")]
        [TestCase("none_existing_folder")]
        [TestCase("not_assembly_folder")]
        [TestCase("cms_folder")]
        public void TC06_Sync_with_Invalid_path(string path)
        {
            string invalidPath;
            switch (path)
            {
                case "none_existing_folder":
                    invalidPath = Path.Combine(ConfigurationResource.DefaultAssembly, path);
                    break;
                case "not_assembly_folder":
                    invalidPath = Path.Combine(ConfigurationResource.ExportWorkingDirectory);
                    break;
                case "cms_folder":
                    invalidPath = ConfigurationResource.ExportWorkingDirectory.Substring(0, ConfigurationResource.SyncWorkingDirectory.IndexOf("sample"));
                    break;
                default:
                    invalidPath = path;
                    break;
            }

            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(invalidPath, null, ConfigurationResource.ManagementSiteUrl);
            switch (path)
            {
                case "not_assembly_folder":
                    Assert.IsTrue(result.Contains(ResultMessages.UnableToFindAssemblies));
                    break;
                case "cms_folder":
                    Assert.IsTrue(result.Contains(ResultMessages.UnableToExportCMS));
                    break;
                case "none_existing_folder":
                default:
                    Assert.IsTrue(result.Contains(ResultMessages.FileNotExist(invalidPath)));
                    break;
            }
        }

        [Test, Description("Sync without source")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        public void TC07_Sync_without_source()
        {
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, null, "");

            Assert.IsTrue(result.Contains(ResultMessages.ErrorOptionSourceIsRequired));
        }

        [Test, Description("Sync without argument value for option")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("source")]
        [TestCase("merge")]
        [TestCase("authority")]
        [TestCase("client")]
        [TestCase("secret")]
        public void TC08_Sync_without_argument_value_for_option(string option)
        {
            string result = "";
            switch (option)
            {
                case "source":
                    result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, null, "", extension: "--source"); 
                    break; 
                case "merge":
                    result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--merge"); 
                    break;
                case "authority":
                    result = ManifestControllers.ExecuteSync(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, clientId: ConfigurationResource.Client, secretKey: ConfigurationResource.Secret, extension: "--authority");
                    break;
                case "client":
                    result = ManifestControllers.ExecuteSync(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, secretKey: ConfigurationResource.Secret, extension: "--client");
                    break;
                case "secret":
                    result = ManifestControllers.ExecuteSync(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, clientId: ConfigurationResource.Client, extension: "--secret");
                    break;
                default:
                    break;
            }

            Assert.IsTrue(result.Contains(ResultMessages.ErrorRequiredArgumentMissingForOption(option)));
        }

        [Test, Description("Sync with invalid source")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("http://epvnwrkming:8001")]  // source doesn't not exist
        [TestCase("invalid")]
        [TestCase("http://epvnwrkming:9000")] // source doen't support
        public void TC09_Sync_with_invalid_source(string invalidSource)
        {
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, null, invalidSource);

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

        [Test, Description("Sync with invalid authority server")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("http://epvnwrkming:8001")]  // source doesn't not exist
        [TestCase("http://epvnwrkming:9000")] // not openID Connect server
        [TestCase("invalid")]
        public void TC10_Sync_with_invalid_authority_server(string invalidAuthority)
        {
            string result = ManifestControllers.ExecuteSync(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, invalidAuthority, ConfigurationResource.Client, ConfigurationResource.Secret);

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

        [Test, Description("Sync with Invalid client/secret key")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("invalid", "cli")]
        [TestCase("cli", "invalid")]
        [TestCase("cli_noScope", "cli_noScope")] //valid client + secret key + invalid scope
        [TestCase("", "cli")]
        [TestCase("cli", "")]
        [TestCase("", "")]
        public void TC11_Sync_with_Invalid_client_secret_key(string client, string secret)
        {
            string result = ManifestControllers.ExecuteSync(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, ConfigurationResource.ManagementSiteUrl, client, secret);

            if (string.IsNullOrEmpty(client) || string.IsNullOrEmpty(secret))
                Assert.IsTrue(result.Contains(ResultMessages.ErrorOptionAuthorityAlsoExpectsClientAndSecret));
            else if (client == "invalid")
                Assert.IsTrue(result.Contains(ResultMessages.ErrorTheSpecifiedClientIdIsInvalid));
            else if (secret == "invalid")
                Assert.IsTrue(result.Contains(ResultMessages.ErrorTheSpecifiedClientCredentialsAreInvalid));
            else
                Assert.IsTrue(result.Contains(ResultMessages.ErrorThisClientApplicationIsNotAllowed));
        }

        [Test, Description("Sync with invalid merge path")]
        [Category("HackdayQ4")]
        [Category("Sync")]
        [TestCase("invalid.json")]
        [TestCase("*.*")]
        public void TC12_Sync_with_invalid_merge_path(string invalidFile)
        {
            string result = ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, new List<string>() { invalidFile }, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorFileDoesNotExist(invalidFile)));
        }

        [TearDown]
        public void CleanUp()
        {
            // Remove custom content type
            string responseData = CDA_ContentTypeControllers.GetContentTypes().Content;
            List<ContentType> contentTypes = JsonConvert.DeserializeObject<List<ContentType>>(responseData);
            List<ContentType> testContentTypes = contentTypes.Where(ct => (ct.Name.StartsWith("MyContent") || ct.Name.StartsWith("MyCode"))).ToList();
            string a;
            foreach (ContentType ct in testContentTypes)
                a = CDA_ContentTypeControllers.DeleteContentType(ct.Id);

            // Remove pull output file 
            FileFolderHelper.EmptyDirectory(new DirectoryInfo(ConfigurationResource.PullWorkingDirectory));

            //Sync with default assembly before running test
            ManifestControllers.ExecuteSyncWithDefaultAuthority(ConfigurationResource.DefaultAssembly, null, ConfigurationResource.ManagementSiteUrl, extension: "--force --use-assembly-versioning");
        }

        [SetUp]
        public void SetUp()
        {
        }
    }
}
