using Automation_NCD_CLI.Models;
using Automation_NCD_CLI.TestResources;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Automation_NCD_CLI.Scenarios
{
    public class PushTests : TestsBase
    {
        [Test, Description("Verify push successfully")]
        [Category("Hackday")]
        [TestCase("push_TC1_noVersion.json")]
        [TestCase("push_TC1_version.json")]
        public void TC01_Verify_push_successfully(string json)
        {
            //dotnet epi-content-manifest push push_TC1_noVersion.json -s https://localhost:16002/ --authority https://localhost:16002/ --client cli --secret cli 

            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(1)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("Push with authority")]
        [Category("Hackday")]
        [TestCase("push_TC1_noVersion.json")]
        [TestCase("push_TC1_version.json")]
        public void TC02_Push_with_authority(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(1)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("Push with  no data")]
        [Category("Hackday")]
        [TestCase("push_noData1.json")]
        [TestCase("push_noData2.json")]
        [TestCase("push_noData3.json")]
        public void TC03_Push_with_no_data(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(0)));
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(0)));
        }

        #region Cases to check options
        [Test, Description("Push without path")]
        [Category("Hackday")]
        public void TC04_Push_without_path()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("", ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorNoPath));
        }

        [Test, Description("Push with invalid path")]
        [Category("Hackday")]
        [TestCase("push_TC1_noVersion.txt")]
        [TestCase("push_TC1_InvalidJsonFormat.txt")]
        [TestCase("invalidPath.json")]
        public void TC05_Push_with_invalid_path(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            switch(json)
            {
                case "push_TC1_noVersion.txt":
                    Assert.IsTrue(result.Contains(ResultMessages.SuccessImportPropertyGroup(1)));
                    Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
                    break;
                case "push_TC1_InvalidJsonFormat.txt":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorInvalidJSONFormat));
                    break;
                case "invalidPath.json":
                    Assert.IsTrue(result.Contains(ResultMessages.ErrorFileDoesNotExist(json)));
                    break;
                default:
                    break;
            }    
        }

        [Test, Description("Push without source")]
        [Category("Hackday")]
        public void TC06_Push_without_source()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TC1_noVersion", "");

            Assert.IsTrue(result.Contains(ResultMessages.ErrorOptionSourceIsRequired));
        }

        [Test, Description("Push without argument value for @option")]
        [Category("Hackday")]
        [TestCase("source")]
        [TestCase("authority")]
        [TestCase("client")]        
        [TestCase("secret")]
        [TestCase("allowed-upgrades")]
        public void TC07_Push_without_argument_value_for_option(string option)
        {
            string result = "";
            string json = "push_TC1_noVersion.json";
            switch (option)
            {
                case "source":
                    result = ManifestControllers.ExecutePushWithDefaultAuthority(json, "", extension: "--source");
                    break;
                case "authority":
                    result = ManifestControllers.ExecutePush(json, ConfigurationResource.ManagementSiteUrl, clientId: ConfigurationResource.Client, secretKey: ConfigurationResource.Secret, extension: "--authority");
                    break;
                case "client":
                    result = ManifestControllers.ExecutePush(json, ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, secretKey: ConfigurationResource.Secret, extension: "--client");
                    break;
                case "secret":
                    result = ManifestControllers.ExecutePush(json, ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, clientId: ConfigurationResource.Client, extension: "--secret");
                    break;
                case "allowed-upgrades":
                    result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, extension: "--allowed-upgrades");
                    break;
                default:
                    break;
            }

            Assert.IsTrue(result.Contains(ResultMessages.ErrorRequiredArgumentMissingForOption(option)));
        }

        [Test, Description("Push with invalid source")]
        [Category("Hackday")]
        [TestCase("http://epvnwrkming:8001")]  // source doesn't not exist
        [TestCase("invalid")]
        [TestCase("http://epvnwrkming:9000")] // source doen't support
        public void TC08_Push_with_invalid_source(string invalidSource)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TC1_noVersion.json", invalidSource);

            switch(invalidSource)
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

        [Test, Description("Push with invalid authority server")]
        [Category("Hackday")]
        [TestCase("http://epvnwrkming:8001")]  // source doesn't not exist
        [TestCase("http://epvnwrkming:9000")] // not openID Connect server
        [TestCase("invalid")]
        public void TC09_Push_with_invalid_authority_server(string invalidAuthority)
        {
            string result = ManifestControllers.ExecutePush("push_TC1_noVersion.json", ConfigurationResource.ManagementSiteUrl, authority: invalidAuthority, clientId: ConfigurationResource.Client, secretKey: ConfigurationResource.Secret);

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

        [Test, Description("Push with Invalid client/secret key")]
        [Category("Hackday")]
        [TestCase("invalid", "cli")]
        [TestCase("cli", "invalid")]
        [TestCase("cli_noScope", "cli_noScope")] //valid client + secret key + invalid scope
        [TestCase("", "cli")]
        [TestCase("cli", "")]
        [TestCase("", "")]
        public void TC10_Push_with_Invalid_client_secret_key(string client, string secret)
        {
            //dotnet epi-content-manifest push push_TC1_noVersion.json -s https://localhost:16002/ --authority https://localhost:16002/ --client cli_noScope --secret cli_noScope

            string result = ManifestControllers.ExecutePush("push_TC1_noVersion.json", ConfigurationResource.ManagementSiteUrl, authority: ConfigurationResource.ManagementSiteUrl, clientId: client, secretKey: secret);

            if (string.IsNullOrEmpty(client) || string.IsNullOrEmpty(secret))
                Assert.IsTrue(result.Contains(ResultMessages.ErrorOptionAuthorityAlsoExpectsClientAndSecret));
            else if (client == "invalid")
                Assert.IsTrue(result.Contains(ResultMessages.ErrorTheSpecifiedClientIdIsInvalid));
            else if (secret == "invalid")
                Assert.IsTrue(result.Contains(ResultMessages.ErrorTheSpecifiedClientCredentialsAreInvalid));
            else
                Assert.IsTrue(result.Contains(ResultMessages.ErrorThisClientApplicationIsNotAllowed));
        }
        #endregion

        [Test, Description("push with invalid data")]
        [Category("Hackday")]
        public void TC11_push_with_invalid_data()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_InvalidContentType.json", ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorNoBaseType("InvalidType")));
        }

        [Test, Description("push an content type with same name")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_sameContentTypeName.json", "MyContentTypeVersion_preparation")]
        [TestCase("push_TCx_Version_sameContentTypeName_UPCASE.json", "MyContentTypeVersion_PREPARATION")]
        public void TC12_push_existing_version_content_type_with_same_name(string json, string contentTypeName)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorExistedContentType(contentTypeName)));
        }

        #region Non-version content type
        [Test, Description("push a new content type")]
        [Category("Hackday")]
        [TestCase("major")]
        [TestCase("minor")]
        [TestCase("patch")]
        public void TC13_push_a_new_content_type(string severity)
        {
            string json = "push_TCx_NoVersion1.json";
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push content type with allowed-upgrades = None" )]
        [Category("Hackday")]
        [TestCase("push_TCx_NoVersion_major1.json")]
        [TestCase("push_TCx_NoVersion_minor.json")]
        public void TC14_push_existing_content_type_with_allowed_upgrades_None(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, "none");

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content with allowed-upgrades  = change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_NoVersion_major1.json", "major")]
        [TestCase("push_TCx_NoVersion_major2.json", "major")]
        [TestCase("push_TCx_NoVersion_major3.json", "major")]
        [TestCase("push_TCx_NoVersion_minor.json", "minor")]
        public void TC15_push_existing_content_with_allowed_upgrades_equals_change_level(string json, string severity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push content type  with allowed-upgrades < change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_NoVersion_major1.json", "minor")]
        [TestCase("push_TCx_NoVersion_minor.json", "patch")]
        public void TC16_push_existing_content_type_with_allowed_upgrades_less_than_change_level(string json, string severity)
        {
            // dotnet epi-content-manifest push push_TCx_NoVersion_major1.json -s https://localhost:16002/ --allowed-upgrades minor --authority https://localhost:16002/ --client cli --secret cli 

            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content type  with allowed-upgrades > change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_NoVersion_minor.json", "major")]
        public void TC17_push_existing_content_type_with_allowed_upgrades_greater_than_change_level(string json, string severity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push content type without allowed-upgrades and major change")]
        [Category("Hackday")]
        public void TC18_push_existing_content_type_without_allowed_upgrades_and_major_change()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_NoVersion_major1.json", ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content type without allowed-upgrades - change level is minor or patch")]
        [Category("Hackday")]
        [TestCase("push_TCx_NoVersion_minor.json")]
        public void TC19_push_existing_content_type_without_allowed_upgrades_change_level_is_minor_or_patch(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        #endregion

        #region Version content type
        [Test, Description("push a new content type")]
        [Category("Hackday")]
        [TestCase("major")]
        [TestCase("minor")]
        [TestCase("patch")]
        public void TC20_push_a_new_content_type(string severity)
        {
            string json = "push_TCx_Version1.json";
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push content type with allowed-upgrades = None")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_major1.json")]
        [TestCase("push_TCx_Version_minor.json")]
        [TestCase("push_TCx_Version_patch.json")]
        public void TC21_push_existing_version_content_type_with_allowed_upgrades_None(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, "none");

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content with allowed-upgrades  = change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_major1.json", "major")]
        [TestCase("push_TCx_Version_minor.json", "minor")]
        public void TC22_push_existing_version_content_with_allowed_upgrades_equals_change_level(string json, string severity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push content type  with allowed-upgrades < change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_major1.json", "minor")]
        [TestCase("push_TCx_Version_minor.json", "patch")]
        public void TC23_push_existing_version_content_type_with_allowed_upgrades_less_than_change_level(string json, string severity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content type  with allowed-upgrades > change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_minor.json", "major")]
        public void TC24_push_existing_version_content_type_with_allowed_upgrades_greater_than_change_level(string json, string severity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, severity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push existed content type without allowed-upgrades and content change level greater than version change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_minor_versionPatch.json", "Minor", "Patch")]
        [TestCase("push_TCx_Version_major1_versionMinor.json", "Major", "Minor")]
        public void TC25_push_existing_version_content_type_without_allowed_upgrades_change_level_greater_than_version_change_level(string json, string changeSeverity, string versionSeverity)
        {
            var filePath = Path.Combine(ConfigurationResource.PushWorkingDirectory, json);
            ContentType contentType = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(filePath)).ContentTypes[0];

            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngradesLevelLessThanChange(contentType.Name, versionSeverity, changeSeverity, "1.1.1.1", contentType.Version)));
        }

        [Test, Description("push existed content type without allowed-upgrades and content change level is equal to version change level = minor")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_minor_versionMinor.json")]
        public void TC26_push_existing_version_content_type_without_allowed_upgrades_change_level_equals_version_change_level_mỉnor(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push existed content type without allowed-upgrades and content change level is equal to version change level = major")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_major1_versionMajor.json")]
        public void TC26_push_existing_version_content_type_without_allowed_upgrades_change_level_equals_version_change_level_major(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push existed content type without allowed-upgrades and content change level is less than version change level")]
        [Category("Hackday")]
        public void TC27_push_existing_version_content_type_without_allowed_upgrades_change_level_less_than_version_change_level()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_Version_patch_versionMinor.json", ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push existing content type without force")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_majorVersionDown.json")]
        [TestCase("push_TCx_Version_majorVersionUp.json")]
        [TestCase("push_TCx_Version_minorVersionDown.json")]
        [TestCase("push_TCx_Version_minorVersionUp.json")]
        [TestCase("push_TCx_Version_patchVersionDown.json")]
        [TestCase("push_TCx_Version_patchVersionUp.json")]
        public void TC28_push_existing_version_content_type_without_force(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);
            if(json.Contains("minorVersionUp") || json.Contains("patchVersionUp"))
                Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
            else
                Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content type - with force")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_majorVersionDown.json")]
        [TestCase("push_TCx_Version_majorVersionUp.json")]
        [TestCase("push_TCx_Version_minorVersionDown.json")]
        [TestCase("push_TCx_Version_minorVersionUp.json")]
        [TestCase("push_TCx_Version_patchVersionDown.json")]
        [TestCase("push_TCx_Version_patchVersionUp.json")]
        public void TC29_push_existing_version_content_type_with_force(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, force:true);
                
            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        /*
        [Test, Description("push existing content type without allow-downgrades and content type version is down")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_majorVersionDown.json")]
        [TestCase("push_TCx_Version_minorVersionDown.json")]
        [TestCase("push_TCx_Version_patchVersionDown.json")]
        public void TC28_push_existing_version_content_type_without_allow_downgrades_version_is_down(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push existing content type with allow-downgrades = none and content type version is down")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_majorVersionDown.json")]
        [TestCase("push_TCx_Version_minorVersionDown.json")]
        [TestCase("push_TCx_Version_patchVersionDown.json")]
        public void TC29_push_existing_version_content_type_with_allow_downgrades_none_version_is_down(string json)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content type with allow-downgrade = content type change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_patchVersionDown.json", "Patch")]
        [TestCase("push_TCx_Version_minorVersionDown.json", "Minor")]
        [TestCase("push_TCx_Version_majorVersionDown.json", "Major")]
        public void TC30_push_existing_version_content_type_with_allow_downgrades_equals_content_type_change_level(string json, string downgradeSeverity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, allowDowngrades: downgradeSeverity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push existing content type with allow-downgrade < content type change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_majorVersionDown.json", "Minor")]
        [TestCase("push_TCx_Version_minorVersionDown.json", "Patch")]
        public void TC31_push_existing_version_content_type_with_allow_downgrades_less_than_content_type_change_level(string json, string downgradeSeverity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, allowDowngrades: downgradeSeverity);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorUpgradesDowngrades));
        }

        [Test, Description("push content type - with allow-downgrade > content change level")]
        [Category("Hackday")]
        [TestCase("push_TCx_Version_patchVersionDown.json", "Minor")]
        [TestCase("push_TCx_Version_minorVersionDown.json", "Major")]
        public void TC32_push_existing_version_content_type_with_allow_downgrades_greater_than_content_type_change_level(string json, string downgradeSeverity)
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority(json, ConfigurationResource.ManagementSiteUrl, allowDowngrades: downgradeSeverity);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }
        */
        #endregion

        #region Combined case
        [Test, Description("push content type which has all property data types")]
        [Category("Hackday")]
        public void TC33_push_content_type_which_has_all_property_data_types()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TC_allProperties.json", ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push multiple content types")]
        [Category("Hackday")]
        public void TC34_push_multiple_content_types()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TC_multipleContentTypes.json", ConfigurationResource.ManagementSiteUrl);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(2)));
        }

        [Test, Description("push existing content types with both allowed-upgrades and allowed-downgrades")]
        [Category("Hackday")]
        public void TC35_push_multi_existing_content_types_with_both_allowed_upgrades_downgrades()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TC_multipleContentTypes_majorVersionUp_majorVersionDown.json", ConfigurationResource.ManagementSiteUrl, "Major", true);

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(2)));
        }

        [Test, Description("push to update non-version content type to version content type")]
        [Category("Hackday")]
        public void TC36_push_existing_content_type_to_version_content_type()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_NoVersion_toVersion.json", ConfigurationResource.ManagementSiteUrl, "Major");

            Assert.IsTrue(result.Contains(ResultMessages.SuccessImportContentType(1)));
        }

        [Test, Description("push to update version content type to non-version content type")]
        [Category("Hackday")]
        public void TC37_push_existing_version_content_type_to_non_version_content_type()
        {
            string result = ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_Version_toNoVersion.json", ConfigurationResource.ManagementSiteUrl, "Major");

            Assert.IsTrue(result.Contains(ResultMessages.ErrorConvertVerionToNonVersion("MyContentTypeVersion_preparation")));
        }

        #endregion

        [SetUp]
        public void SetUp()
        {
            if (TestContext.CurrentContext.Test.MethodName.Contains("push_existing_content_type"))
            {
                // Prepare non version content type
                ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_NoVersion_preparation.json", ConfigurationResource.ManagementSiteUrl);
            } 
            else if (TestContext.CurrentContext.Test.MethodName.Contains("push_existing_version_content_type"))
            {
                // Prepare version content type
                ManifestControllers.ExecutePushWithDefaultAuthority("push_TCx_Version_preparation.json", ConfigurationResource.ManagementSiteUrl);
            }
            else if(TestContext.CurrentContext.Test.MethodName == "TestContext.CurrentContext.Test.MethodName")
            {
                // Prepare multiple content types
                ManifestControllers.ExecutePushWithDefaultAuthority("push_TC_multipleContentTypes.json", ConfigurationResource.ManagementSiteUrl);
            }    
        }

        [TearDown]
        public void CleanUp()
        {
            string responseData = CDA_ContentTypeControllers.GetContentTypes().Content;
            List<ContentType> contentTypes = JsonConvert.DeserializeObject<List<ContentType>>(responseData);
            List<ContentType> testContetnTypes = contentTypes.Where(ct => ct.Name.StartsWith("MyContent")).ToList();
            foreach (ContentType ct in testContetnTypes)
                CDA_ContentTypeControllers.DeleteContentType(ct.Id);
        }
    }
}
