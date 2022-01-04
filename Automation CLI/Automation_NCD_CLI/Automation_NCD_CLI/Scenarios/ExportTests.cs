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
    class ExportTests : TestsBase
    {
        private string outputFile = "";

        [Test, Description("Export manifest to terminal command line")]
        [Category("Export")]
        public void TC01_Export_manifest_to_terminal_command_line()
        {
            string result = ManifestControllers.ExecuteExport(ConfigurationResource.DefaultAssembly);

            Assert.IsTrue(result.Contains("contentTypes"));
        }

        [Test, Description("Export manifest to output file")]
        [Category("Export")]
        public void TC02_Export_manifest_to_output_file()
        {
            outputFile = "export_TC2.json";
            string result = ManifestControllers.ExecuteExport(ConfigurationResource.DefaultAssembly, outputFile);
            var outputPath = Path.Combine(ConfigurationResource.ExportWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));

            Assert.IsNotEmpty(pullResponse.ContentTypes);
            Assert.IsNotNull(pullResponse.ContentTypes.Where(x => x.Name == "MyCodeTestPage").FirstOrDefault());
        }

        [Test, Description("Export without path")]
        [Category("Export")]
        public void TC03_Export_without_path()
        {
            outputFile = "export_TC2.json";
            string result = ManifestControllers.ExecuteExport("", outputFile);

            var outputPath = Path.Combine(ConfigurationResource.ExportWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));

            Assert.IsNotEmpty(pullResponse.ContentTypes);
            Assert.IsNotNull(pullResponse.ContentTypes.Where(x => x.Name == "MyCodeTestPage").FirstOrDefault());
        }

        [Test, Description("Export with Invalid path")]
        [Category("Export")]
        [TestCase("invalid")]
        [TestCase("none_existing_folder")]
        [TestCase("not_assembly_folder")]
        [TestCase("cms_folder")]
        public void TC04_Export_with_invalid_path(string path)
        {
            string invalidPath;
            switch(path)
            {
                case "none_existing_folder":
                    invalidPath = Path.Combine(ConfigurationResource.ExportWorkingDirectory, path);
                    break;
                case "not_assembly_folder":
                    invalidPath = ConfigurationResource.ExportWorkingDirectory;
                    break;
                case "cms_folder":
                    invalidPath = ConfigurationResource.ExportWorkingDirectory.Substring(0, ConfigurationResource.ExportWorkingDirectory.IndexOf("sample"));
                    break;
                default:
                    invalidPath = path;
                    break;
            }    

            string result = ManifestControllers.ExecuteExport(invalidPath);
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

        [Test, Description("Export with option ' --use-assembly-versioning'")]
        [Category("Export")]
        public void TC06_Export_with_option_use_assembly_versioning()
        {
            outputFile = "export_TC2.json";
            ManifestControllers.ExecuteExport(ConfigurationResource.DefaultAssembly, outputFile, "--use-assembly-versioning");

            // Get assembliesVersion
            FileVersionInfo assembliesVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(ConfigurationResource.DefaultAssembly, "Alloy.DeliverySite.dll"));
            string assembliesVersion = assembliesVersionInfo.FileVersion;

            var outputPath = Path.Combine(ConfigurationResource.ExportWorkingDirectory, outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));

            Assert.IsNotEmpty(pullResponse.ContentTypes);
            foreach(ContentType contentType in pullResponse.ContentTypes)
                Assert.AreEqual(assembliesVersion, contentType.Version);
        }

        [TearDown]
        public void CleanUp()
        {
            // Remove output file
            FileFolderHelper.EmptyDirectory(new DirectoryInfo(Path.Combine(ConfigurationResource.ExportWorkingDirectory)));
        }

    }
}
