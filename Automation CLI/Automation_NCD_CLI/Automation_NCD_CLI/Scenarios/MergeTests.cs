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
    class MergeTests : TestsBase
    {
        private string outputFile = "";

        [Test, Description("Merge 2 json files without duplicate to command line")]
        [Category("Merge")]
        public void TC01_Merge_json_files_without_duplicate_to_command_line()
        {
            List<string> paths = new List<string>() { "merge_TC1_contentType1.json", "merge_TC1_contentType2.json" };
            string result = ManifestControllers.ExecuteMerge(paths);

            Assert.IsTrue(result.Contains("contentTypes"));
        }

        [Test, Description("Merge 2 json files without duplicate to output file")]
        [Category("Merge")]
        [TestCase("merge_TC1_contentType1.json,merge_TC1_contentType2.json,merge_TC1_contentType3.json,merge_TC1_emptyContentTypes.json")]
        [TestCase("merge_TC1_contentType1.json,merge_TC1_emptyFile.json,merge_TC1_contentType3.json,merge_TC1_contentType2.json")]
        [TestCase("merge_TC1_contentType3.json,merge_TC1_contentType2.json,merge_TC1_contentType1.json")]
        public void TC02_Merge_json_files_without_duplicate_to_output_file(string mergingFiles)
        {
            string outputFile = "merge_TC1_contentTypes.json";
            List<string> files = mergingFiles.Split(',').ToList();
            string result = ManifestControllers.ExecuteMerge(files, outputFile);

            // build the expected contentType list in merged order
            List<string> expectedContentTypeNames = new List<string>();
            foreach(string file in files)
            {
                var filePath = Path.Combine(ConfigurationResource.MergeWorkingDirectory, file);
                PullResponse fileContent = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(filePath));
                if(fileContent != null)
                    foreach (ContentType contentType in fileContent.ContentTypes)
                        expectedContentTypeNames.Add(contentType.Name);
            }    

            var outputPath = Path.Combine(ConfigurationResource.MergeWorkingDirectory, "output", outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            List<string> mergedContentTypeNames = pullResponse.ContentTypes.Select(x => x.Name).ToList();

            Assert.AreEqual(expectedContentTypeNames, mergedContentTypeNames);
        }

        [Test, Description("Merge 2 json files with duplicate content type")]
        [Category("Merge")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1Properties2.json")]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name1Id1.json", true)]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1Id1.json")]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name2Id1.json", true)]
        [TestCase("merge_TC1_contentType1.json,merge_TC1_contentType1.txt,merge_TC1_contentType1")]
        public void TC03_Merge_json_files_with_duplicate_content_type(string mergingFiles, bool isDuplicatedId = false)
        {
            string outputFile = "merge_TC3_contentTypes.json";
            List<string> files = mergingFiles.Split(',').ToList();
            string result = ManifestControllers.ExecuteMerge(files, outputFile);

            // build the expected contenType list in merged order
            Dictionary<string, ContentType> expectedContentTypesDict = new Dictionary<string, ContentType>();
            foreach (string file in files)
            {
                var filePath = Path.Combine(ConfigurationResource.MergeWorkingDirectory, file);
                PullResponse fileContent = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(filePath));
                foreach (ContentType contentType in fileContent.ContentTypes)
                {
                    string key = isDuplicatedId ? contentType.Id : contentType.Name;
                    if (expectedContentTypesDict.ContainsKey(key))
                    {
                        expectedContentTypesDict[key].Name = contentType.Name;
                        if (!string.IsNullOrEmpty(contentType.Id))
                            expectedContentTypesDict[key].Id = contentType.Id;
                        if (!string.IsNullOrEmpty(contentType.Version))
                            expectedContentTypesDict[key].Version = contentType.Version;
                        foreach (Property prop in contentType.Properties)
                            if(!expectedContentTypesDict[key].Properties.Where(x => x.Name == prop.Name).Any())
                                expectedContentTypesDict[key].Properties.Add(prop);
                    }
                    else
                        expectedContentTypesDict.Add(key, contentType);
                }
            }
            List<ContentType> expectedContentTypes = expectedContentTypesDict.Values.ToList();

            var outputPath = Path.Combine(ConfigurationResource.MergeWorkingDirectory, "output", outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            List<ContentType> mergedContentTypes = pullResponse.ContentTypes.ToList();

            Assert.AreEqual(JsonConvert.SerializeObject(expectedContentTypes), JsonConvert.SerializeObject(mergedContentTypes));
        }

        [Test, Description("Merge 2 json files with duplicate content type name but different id/ base type")]
        [Category("Merge")]
        [TestCase("merge_TC4_name1Id1.json,merge_TC4_name1Id2.json")]
        [TestCase("merge_TC4_name1Id1.json,merge_TC4_name1Id1Base2.json", true)]
        [TestCase("merge_TC4_name1.json,merge_TC4_name1Base2.json", true)]
        public void TC04_Merge_json_files_with_duplicate_name_but_different_id_baseType(string mergingFiles, bool isBaseTypeChanged = false)
        {
            string outputFile = "merge_TC4_contentTypes.json";
            List<string> files = mergingFiles.Split(',').ToList();
            string result = ManifestControllers.ExecuteMerge(files, outputFile);

            if(isBaseTypeChanged)
                Assert.IsTrue(result.Contains(ResultMessages.ContentTypeCannotBeMergedBaseType));
            else
                Assert.IsTrue(result.Contains(ResultMessages.ContentTypeCannotBeMergedId));
        }

        [Test, Description("Merge without path")]
        [Category("Merge")]
        public void TC08_Merge_without_path()
        {
            string result = ManifestControllers.ExecuteMerge(new List<string>());

            Assert.IsTrue(result.Contains(ResultMessages.RequireArgumentForExport("merge")));
        }

        [Test, Description("Merge with Invalid file path")]
        [Category("Merge")]
        [TestCase("non_existing.json")]
        [TestCase("output")]
        [TestCase("*.*")]
        public void TC09_Merge_with_Invalid_file_path(string invalidFile)
        {
            List<string> files = new List<string>() { "merge_TC1_contentType1.json" };
            files.Add(invalidFile);
                   
            string result = ManifestControllers.ExecuteMerge(files);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorFileDoesNotExist(invalidFile)));
        }

        [Test, Description("Merge when json file has wrong format")]
        [Category("Merge")]
        [TestCase("invalidJson.json")]
        public void TC10_Merge_when_json_file_has_wrong_format(string invalidFile)
        {
            List<string> files = new List<string>() { "merge_TC1_contentType1.json" };
            files.Add(invalidFile);
                   
            string result = ManifestControllers.ExecuteMerge(files);

            Assert.IsTrue(result.Contains(ResultMessages.ErrorInvalidJSONFormat));
        }

        [Test, Description("Merge when json file has invalid data")]
        [Category("Merge")]
        [TestCase("merge_TCx_InvalidContentType_nameIsNull.json", "invalidNameMissing")]
        [TestCase("merge_TCx_InvalidContentType_nameIsEmpty.json", "invalidNameMissing")]
        [TestCase("merge_TCx_InvalidContentType_nameIncludeSpace.json", "invalidName")]
        [TestCase("merge_TCx_InvalidContentType_nameContainsSpecialCharacters.json", "invalidName")]
        [TestCase("merge_TCx_InvalidContentType_nameExceedLength.json", "invalidName")]
        [TestCase("merge_TCx_InvalidContentType_nameIsNotAString.json", "invalidNameFormat")]
        [TestCase("merge_TCx_InvalidContentType_baseTypeContainsSpace.json", "invalidBaseType")]
        [TestCase("merge_TCx_InvalidContentType_baseTypeContainsSpecialCharacters.json", "invalidBaseType")]
        [TestCase("merge_TCx_InvalidContentType_dataTypeIsNull.json", "invalidDataTypeMissing")]
        [TestCase("merge_TCx_InvalidContentType_dataTypeIsEmpty.json", "invalidDataTypeMissing")]
        [TestCase("merge_TCx_InvalidContentType_dataTypeContainsSpace.json", "invalidDataType")]
        [TestCase("merge_TCx_InvalidContentType_dataTypeContainsSpecialCharacters.json", "invalidDataType")]
        //[TestCase("merge_TCx_InvalidContentType_baseTypeIsNull.json", "invalidBaseType")]
        //[TestCase("merge_TCx_InvalidContentType_baseTypeIsEmpty.json", "invalidBaseType")]
        public void TC11_Merge_when_json_file_has_invalid_data(string invalidFile, string invalidType)
        {
            List<string> files = new List<string>() { "merge_TC1_contentType1.json" };
            files.Add(invalidFile);
                   
            string result = ManifestControllers.ExecuteMerge(files);
            switch(invalidType)
            {
                case "invalidNameMissing":
                    Assert.IsTrue(result.Contains(ResultMessages.InvalidContentTypeNameMissing));
                    break;
                case "invalidName":
                    Assert.IsTrue(result.Contains(ResultMessages.InvalidContentTypeName));
                    break;
                case "invalidNameFormat":
                    Assert.IsTrue(result.Contains(ResultMessages.InvalidContentTypeNameFormat));
                    break;
                case "invalidBaseType":
                    // NCD-1582 NOT FIXED (message is not correct)
                    Assert.IsTrue(result.Contains(ResultMessages.InvalidContentTypeName));
                    break;
                case "invalidDataTypeMissing":
                    Assert.IsTrue(result.Contains(ResultMessages.InvalidContentTypeDataTypeMissing));
                    break;
                case "invalidDataType":
                    Assert.IsTrue(Regex.IsMatch(result, ResultMessages.InvalidContentTypeDataType));
                    break;
                default:
                    break;
            }    
        }

        [Test, Description("Merge 2 json files with duplicate content type")]
        [Category("Merge")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1Properties2.json")]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name1Id1.json", true)]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name1.json")]
        [TestCase("merge_TC3_name1.json,merge_TC3_name1Id1.json")]
        [TestCase("merge_TC3_name1Id1.json,merge_TC3_name2Id1.json", true)]
        [TestCase("merge_TC1_contentType1.json,merge_TC1_contentType1.txt,merge_TC1_contentType1")]
        public void TC12_Merge_json_files_with_duplicate_content_type_with_reverse(string mergingFiles, bool isDuplicatedId = false)
        {
            string outputFile = "merge_TC3_contentTypes.json";
            List<string> files = mergingFiles.Split(',').ToList();
            string result = ManifestControllers.ExecuteMerge(files, outputFile, "--reverse");

            // build the expected contenType list in merged order
            Dictionary<string, ContentType> expectedContentTypesDict = new Dictionary<string, ContentType>();
            files.Reverse();
            foreach (string file in files)
            {
                var filePath = Path.Combine(ConfigurationResource.MergeWorkingDirectory, file);
                PullResponse fileContent = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(filePath));
                foreach (ContentType contentType in fileContent.ContentTypes)
                {
                    string key = isDuplicatedId ? contentType.Id : contentType.Name;
                    if (expectedContentTypesDict.ContainsKey(key))
                    {
                        expectedContentTypesDict[key].Name = contentType.Name;
                        if (!string.IsNullOrEmpty(contentType.Id))
                            expectedContentTypesDict[key].Id = contentType.Id;
                        if (!string.IsNullOrEmpty(contentType.Version))
                            expectedContentTypesDict[key].Version = contentType.Version;
                        foreach (Property prop in contentType.Properties)
                            if (!expectedContentTypesDict[key].Properties.Where(x => x.Name == prop.Name).Any())
                                expectedContentTypesDict[key].Properties.Add(prop);
                    }
                    else
                        expectedContentTypesDict.Add(key, contentType);
                }
            }
            List<ContentType> expectedContentTypes = expectedContentTypesDict.Values.ToList();

            var outputPath = Path.Combine(ConfigurationResource.MergeWorkingDirectory, "output", outputFile);
            PullResponse pullResponse = JsonConvert.DeserializeObject<PullResponse>(File.ReadAllText(outputPath));
            List<ContentType> mergedContentTypes = pullResponse.ContentTypes.ToList();

            Assert.AreEqual(JsonConvert.SerializeObject(expectedContentTypes), JsonConvert.SerializeObject(mergedContentTypes));
        }

        [TearDown]
        public void CleanUp()
        {
            // Remove output file
            FileFolderHelper.EmptyDirectory(new DirectoryInfo(Path.Combine(ConfigurationResource.MergeWorkingDirectory, "output")));
        }

    }
}
