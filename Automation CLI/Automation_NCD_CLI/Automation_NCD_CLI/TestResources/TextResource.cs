namespace Automation_NCD_CLI.TestResources
{
    /// <summary>
    /// Include const strings
    /// </summary>
    public class TextResource
    {
        public enum LanguageCode
        {
            en,
            sv,
            es
        }
    }

    public class ResultMessages
    {
        /// <summary>
        /// Success: Imported {count} property group
        /// </summary>
        public static string SuccessImportPropertyGroup(int count) => $"Success: Imported {count} property group";
        /// <summary>
        /// Success: Imported {count} content type
        /// </summary>
        public static string SuccessImportContentType(int count) => $"Success: Imported {count} content type";
        /// <summary>
        /// Error message returns when allowUpgrades or allowDowngrades condition is not matched.
        /// Error: Import of the manifest section 'ContentTypes' failed.The version transition is not allowed with the provided content type save options.
        /// </summary>
        public static string ErrorUpgradesDowngrades = "Error: Import of the manifest section 'ContentTypes' failed. The version transition is not allowed with the provided content type save options.";
        /// <summary>
        /// Import of the manifest section 'ContentTypes' failed.The version transition is not allowed with the provided content type save options.The changes in 'MyContentTypeVersion_preparation' are 'Major', going from '1.1.1.1' to '1.2.1.1' is 'Minor'.
        /// </summary>
        /// <param name="contentTypeName">content type name</param>
        /// <param name="request">allowUpgrades or allowDowngrades level</param>
        /// <param name="change">change level in json file</param> 
        /// <param name="oldVersion">old Version</param>
        /// <param name="newVersion">new Version</param>
        /// <returns></returns>
        public static string ErrorUpgradesDowngradesLevelLessThanChange(string contentTypeName, string request, string change, string oldVersion, string newVersion) => $"Import of the manifest section 'ContentTypes' failed. The version transition is not allowed with the provided content type save options. The changes in '{contentTypeName}' are '{change}', going from '{oldVersion}' to '{newVersion}' is '{request}'.";
        /// <summary>
        /// Error message return when try to push content type which name is existed in system - non case sensitive
        /// </summary>
        /// <param name="contentTypeName"></param>
        /// <returns></returns>
        public static string ErrorExistedContentType(string contentTypeName) => $"Error: Import of the manifest section 'ContentTypes' failed. There is already a content type registered with name: {contentTypeName}";
        /// <summary>
        /// File does not exist: {fileName}
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ErrorFileDoesNotExist(string fileName) => $"File does not exist: {fileName}";
        /// <summary>
        /// Invalid JSON format in the file
        /// </summary>
        public static string ErrorInvalidJSONFormat = "Invalid JSON format in the file";
        /// <summary>
        /// Error message returned when try to push existing version content type with non-version content type
        /// </summary>
        /// <param name="contentTypeName"></param>
        /// <returns></returns>
        public static string ErrorConvertVerionToNonVersion(string contentTypeName) => $"Error: Import of the manifest section 'ContentTypes' failed. The content type '{contentTypeName}' is versioned. It's not possible to remove versioning, you need to specify a version.";
        /// <summary>
        /// Error message returned when call push without path
        /// </summary>
        public static string ErrorNoPath = "push command must either include the path argument or provide a manifest using StdIn.";
        /// <summary>
        /// Option '--source' is required.
        /// </summary>
        public static string ErrorOptionSourceIsRequired = "Option '--source' is required.";

        /// <summary>
        /// Required argument missing for option: --option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string ErrorRequiredArgumentMissingForOption(string option) => $"Required argument missing for option: --{option}";

        /// <summary>
        /// The base address must be an absolute URI. (Parameter 'value')
        /// </summary>
        public static string ErrorBaseAddressMustBeAnAbsoluteURI = "The base address must be an absolute URI. (Parameter 'value')";

        /// <summary>
        /// The service returned an unexpected response. The remote service returned status code 'NotFound'.
        /// </summary>
        public static string ErrorTheServiceReturnedAnUnexpectedResponse = "The service returned an unexpected response. The remote service returned status code 'NotFound'.";
        /// <summary>
        /// No connection could be made because the target machine actively refused it. (epvnwrkming:8001)
        /// </summary>
        public static string ErrorNoConnectionCouldBeMade = "No connection could be made because the target machine actively refused it.";
        /// <summary>
        /// Error connecting to http://localhost:9000/.well-known/openid-configuration
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static string ErrorConnectingToServer(string server) => $"Error connecting to {server}/.well-known/openid-configuration";
        /// <summary>
        /// Option '--authority' also expects '--client' and '--secret'
        /// </summary>
        public static string ErrorOptionAuthorityAlsoExpectsClientAndSecret = "Option '--authority' also expects '--client' and '--secret'";
        /// <summary>
        /// The specified 'client_id' is invalid.
        /// </summary>
        public static string ErrorTheSpecifiedClientIdIsInvalid = "The specified 'client_id' is invalid.";
        /// <summary>
        /// The specified client credentials are invalid.
        /// </summary>
        public static string ErrorTheSpecifiedClientCredentialsAreInvalid = "The specified client credentials are invalid.";
        /// <summary>
        /// This client application is not allowed to use the specified scope.
        /// </summary>
        public static string ErrorThisClientApplicationIsNotAllowed = "This client application is not allowed to use the specified scope.";
        /// <summary>
        /// The resource was invalid.: There is no base type registered with identifier 'InvalidType'
        /// </summary>
        public static string ErrorNoBaseType(string baseType) => $"The resource was invalid.\r\n: There is no base type registered with identifier '{baseType}'";
        /// <summary>
        /// The filename, directory name, or volume label syntax is incorrect.
        /// </summary>
        public static string FileNameIncorrect = "The filename, directory name, or volume label syntax is incorrect.";
        /// <summary>
        /// The system cannot find the path specified.
        /// </summary>
        public static string CannotFindPath = "The system cannot find the path specified.";
        /// <summary>
        /// Required argument missing for command: export
        /// </summary>
        public static string RequireArgumentForExport(string command) => $"Required argument missing for command: {command}";
        /// <summary>
        /// The Content delivery CLI tools are unable to export types from an application build on the full CMS. The CLI tools should be run on applications build with the Content delivery Client SDK.
        /// </summary>
        public static string UnableToExportCMS = "The Content delivery CLI tools are unable to export types from an application build on the full CMS. The CLI tools should be run on applications build with the Content delivery Client SDK.";
        /// <summary>
        /// File or directory does not exist: invalidPath
        /// </summary>
        public static string FileNotExist(string invalidPath) => $"File or directory does not exist: {invalidPath}";
        /// <summary>
        /// Unable to find any assemblies in or under the provided path
        /// </summary>
        public static string UnableToFindAssemblies = "Unable to find any assemblies in or under the provided path";
        /// <summary>
        /// A content type can only be merge with another content type with the same or empty identity.
        /// </summary>
        public static string ContentTypeCannotBeMergedId = "A content type can only be merge with another content type with the same or empty identity.";
        /// <summary>
        /// A content type can only be merge with another content type with the same or empty base type.
        /// </summary>
        public static string ContentTypeCannotBeMergedBaseType = "A content type can only be merge with another content type with the same or empty base type.";
        /// <summary>
        /// The name of a content type must be a string starting with a standard letter and being between 2 and 50 characters long.
        /// </summary>
        public static string InvalidContentTypeName = "A content type can only be merge with another content type with the same or empty base type.";
    }
}
