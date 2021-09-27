using Automation_NCD_CLI.TestResources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Automation_NCD_CLI.Helper
{
    /// <summary>
    /// Working with CLI commands
    /// </summary>
    public class CLIHelper
    {
        /// <summary>
        /// Execute CLI command
        /// </summary>
        /// <param name="command">Command to executed</param>
        public static string ExecuteCommand(string command, string workingDirectory)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.StartInfo.Arguments = "/c" + command;

                StringBuilder sb = new StringBuilder();
                process.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => sb.AppendLine(args.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return sb.ToString();
            }
        }
    }
}