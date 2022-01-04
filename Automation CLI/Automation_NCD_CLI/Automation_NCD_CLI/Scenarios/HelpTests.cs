using Automation_NCD_CLI.CLIControllers;
using Automation_NCD_CLI.Models;
using Automation_NCD_CLI.TestResources;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Automation_NCD_CLI.Scenarios
{
    class HelpTests : TestsBase
    {
        [Test, Description("Help")]
        [Category("Merge")]
        [TestCase("-h", "merge", "merge_expected_help.txt")]
        [TestCase("-?", "merge", "merge_expected_help.txt")]
        [TestCase("--help", "merge", "merge_expected_help.txt")]        
        [TestCase("-h", "pull", "pull_expected_help.txt")]
        [TestCase("-?", "pull", "pull_expected_help.txt")]
        [TestCase("--help", "pull", "pull_expected_help.txt")]
        [TestCase("-h", "export", "export_expected_help.txt")]
        [TestCase("-?", "export", "export_expected_help.txt")]
        [TestCase("--help", "export", "export_expected_help.txt")]        
        [TestCase("-h", "push", "push_expected_help.txt")]
        [TestCase("-?", "push", "push_expected_help.txt")]
        [TestCase("--help", "push", "push_expected_help.txt")]
        [TestCase("-h", "sync", "sync_expected_help.txt")]
        [TestCase("-?", "sync", "sync_expected_help.txt")]
        [TestCase("--help", "sync", "sync_expected_help.txt")]
        public void TC01_Merge_Help(string help, string command, string expectedFile)
        {
            string expectedHelp = File.ReadAllText(Path.Combine(ConfigurationResource.HelpDirectory, expectedFile));
            string result = "";
            switch (command)
            {
                case "merge":
                    result = ManifestControllers.ExecuteMerge(null, extension: help);
                    break;
                case "pull":
                    result = ManifestControllers.ExecutePull(null, extension: help);
                    break;
                case "export":
                    result = ManifestControllers.ExecuteExport(null, extension: help);
                    break; 
                case "push":
                    result = ManifestControllers.ExecutePush(null, null, extension: help);
                    break;
                case "sync":
                    result = ManifestControllers.ExecuteSync(null, null, null, extension: help);
                    break;
                default:
                    break;
            }

            if (command == "export" || command == "sync")
                Assert.IsTrue(Regex.IsMatch(result, expectedHelp));
            else
                Assert.IsTrue(result.Contains(expectedHelp));
        }
    }
}
