using Automation_NCD_CLI.TestResources;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Automation_NCD_CLI.Helper
{
    /// <summary>
    /// Working with CLI commands
    /// </summary>
    public class ManifestControllers : CLIHelper
    {
        #region Push command
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="source"></param>
        /// <param name="allowUpgrades"></param>
        /// <param name="allowDowngrades"></param>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <param name="secretKey"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string ExecutePush(string path, string source, string allowUpgrades = null, string allowDowngrades = null, string authority = null, string clientId = null, string secretKey = null, string extension = null)
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} push ";
            if (!string.IsNullOrEmpty(path))
                command += path + " ";
            if (!string.IsNullOrEmpty(source))
                command += $"-s {source} ";
            if (!string.IsNullOrEmpty(allowUpgrades))
                command += $"--allowed-upgrades {allowUpgrades} ";
            if (!string.IsNullOrEmpty(allowDowngrades))
                command += $"--allowed-downgrades {allowDowngrades} ";
            if (!string.IsNullOrEmpty(authority))
                command += $"--authority {authority} ";
            if (!string.IsNullOrEmpty(clientId))
                command += $"--client {clientId} ";
            if (!string.IsNullOrEmpty(secretKey))
                command += $"--secret {secretKey} ";
            if (!string.IsNullOrEmpty(extension))
                command += extension;

            return ExecuteCommand(command, ConfigurationResource.PushWorkingDirectory);
        }

        /// <summary>
        /// Execute push command with default authority got from Configuration
        /// </summary>
        /// <param name="path"></param>
        /// <param name="source"></param>
        /// <param name="allowUpgrades"></param>
        /// <param name="allowDowngrades"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string ExecutePushWithDefaultAuthority(string path, string source, string allowUpgrades = null, bool force = false, string extension = null)
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} push ";
            if (!string.IsNullOrEmpty(path))
                command += path + " ";
            if (!string.IsNullOrEmpty(source))
                command += $"-s {source} ";
            if (!string.IsNullOrEmpty(allowUpgrades))
                command += $"--allowed-upgrades {allowUpgrades} ";
            if (force)
                command += $"--force ";

            command += $"--authority {ConfigurationResource.ManagementSiteUrl} ";
            command += $"--client {ConfigurationResource.Client} ";
            command += $"--secret {ConfigurationResource.Secret} ";

            if (!string.IsNullOrEmpty(extension))
                command += extension;

            return ExecuteCommand(command, ConfigurationResource.PushWorkingDirectory);
        
        }
        #endregion

        #region Pull command
        /// <summary>
        /// Execute pull command with default authority got from Configuration
        /// </summary>
        /// <param name="output"></param>
        /// <param name="source"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string ExecutePullWithDefaultAuthority(string source, string extension = null, string output = "")
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} pull ";

            if (!string.IsNullOrEmpty(source))
                command += $"-s {source} ";

            command += $"--authority {ConfigurationResource.ManagementSiteUrl} ";
            command += $"--client {ConfigurationResource.Client} ";
            command += $"--secret {ConfigurationResource.Secret} ";

            if (!string.IsNullOrEmpty(output))
                command += $"> {output} ";

            if (!string.IsNullOrEmpty(extension))
                command += extension;

            return ExecuteCommand(command, ConfigurationResource.PullWorkingDirectory);
        }

        public string ExecutePull(string source, string authority = null, string clientId = null, string secretKey = null, string extension = null, string output = "")
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} pull ";

            if (!string.IsNullOrEmpty(source))
                command += $"-s {source} ";
            if (!string.IsNullOrEmpty(authority))
                command += $"--authority {authority} ";
            if (!string.IsNullOrEmpty(clientId))
                command += $"--client {clientId} ";
            if (!string.IsNullOrEmpty(secretKey))
                command += $"--secret {secretKey} ";

            if (!string.IsNullOrEmpty(output))
                command += $"> {output} ";

            if (!string.IsNullOrEmpty(extension))
                command += extension;

            return ExecuteCommand(command, ConfigurationResource.PullWorkingDirectory);
        }
        #endregion

        /// <summary>
        /// Execute export command got from Configuration
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ExecuteExport(string path, string output = "", string extension = "")
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} export ";

            if (!string.IsNullOrEmpty(path))
                if (path.IndexOf(" ") != -1)
                    command += $@"""{path}""";
                else
                    command += path;
            if (!string.IsNullOrEmpty(extension))
                command += $" {extension}";
            if (!string.IsNullOrEmpty(output))
                command += $@" > ""{ConfigurationResource.ExportWorkingDirectory}//{output}""";

            return ExecuteCommand(command, string.IsNullOrEmpty(path) ? ConfigurationResource.DefaultAssembly : ConfigurationResource.ExportWorkingDirectory);
        }

        #region Merge command
        /// <summary>
        /// Execute merge command got from Configuration
        /// </summary>
        /// <param name="paths">List of merging file paths</param>
        /// <param name="output">output file path</param>
        /// <param name="extension">extenstion for options</param>
        /// <returns></returns>
        public string ExecuteMerge(List<string> paths, string output = "", string extension = "")
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} merge ";

            if (paths != null)
            {
                foreach (string path in paths)
                {
                    if (path.IndexOf(" ") != -1)
                        command += $@"""{path}"" ";
                    else
                        command += path + " ";
                }
            } 
            if (!string.IsNullOrEmpty(extension))
                command += $" {extension}";
            if (!string.IsNullOrEmpty(output))
                command += $@" > ""{ConfigurationResource.MergeWorkingDirectory}\\output\\{output}""";

            return ExecuteCommand(command, ConfigurationResource.MergeWorkingDirectory);
        }
        #endregion

        #region Sync command
        /// <summary>
        /// Execute sync command
        /// </summary>
        /// <param name="path"></param>
        /// <param name="source"></param>
        /// <param name="allowUpgrades"></param>
        /// <param name="allowDowngrades"></param>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <param name="secretKey"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string ExecuteSync(string path, List<string> mergedPaths, string source, string authority = null, string clientId = null, string secretKey = null, string allowUpgrades = null, string allowDowngrades = null, string extension = null)
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} sync ";
            if (!string.IsNullOrEmpty(path))
            {
                if (path.IndexOf(" ") != -1)
                    command += $@"""{path}"" ";
                else
                    command += path + " ";
            }
            if (mergedPaths != null)
            {
                command += "--merge ";
                foreach (string mPath in mergedPaths)
                {
                    if (mPath.IndexOf(" ") != -1)
                        command += $@"""{mPath}"" ";
                    else
                        command += mPath + " ";
                }
            }
            if (!string.IsNullOrEmpty(source))
                command += $"-s {source} ";
            if (!string.IsNullOrEmpty(allowUpgrades))
                command += $"--allowed-upgrades {allowUpgrades} ";
            if (!string.IsNullOrEmpty(allowDowngrades))
                command += $"--allowed-downgrades {allowDowngrades} ";
            if (!string.IsNullOrEmpty(authority))
                command += $"--authority {authority} ";
            if (!string.IsNullOrEmpty(clientId))
                command += $"--client {clientId} ";
            if (!string.IsNullOrEmpty(secretKey))
                command += $"--secret {secretKey} ";
            if (!string.IsNullOrEmpty(extension))
                command += extension;

            return ExecuteCommand(command, ConfigurationResource.SyncWorkingDirectory);
        }

        /// <summary>
        /// Execute sync command with default authority got from Configuration
        /// </summary>
        /// <param name="path"></param>
        /// <param name="source"></param>
        /// <param name="allowUpgrades"></param>
        /// <param name="allowDowngrades"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string ExecuteSyncWithDefaultAuthority(string path, List<string> mergedPaths, string source, string allowUpgrades = null, bool force = false, string extension = null)
        {
            string command = $"dotnet {ConfigurationResource.CliToolName} sync ";
            if (!string.IsNullOrEmpty(path))
            {
                if (path.IndexOf(" ") != -1)
                    command += $@"""{path}"" ";
                else
                    command += path + " ";
            }
            if (mergedPaths != null)
            {
                command += "--merge ";
                foreach (string mPath in mergedPaths)
                {
                    if (mPath.IndexOf(" ") != -1)
                        command += $@"""{mPath}"" ";
                    else
                        command += mPath + " ";
                }
            }
            if (!string.IsNullOrEmpty(source))
                command += $"-s {source} ";
            if (!string.IsNullOrEmpty(allowUpgrades))
                command += $"--allowed-upgrades {allowUpgrades} ";
            if (force)
                command += $"--force ";

            command += $"--authority {ConfigurationResource.ManagementSiteUrl} ";
            command += $"--client {ConfigurationResource.Client} ";
            command += $"--secret {ConfigurationResource.Secret} ";

            if (!string.IsNullOrEmpty(extension))
                command += extension;

            return ExecuteCommand(command, ConfigurationResource.SyncWorkingDirectory);

        }
        #endregion
    }
}