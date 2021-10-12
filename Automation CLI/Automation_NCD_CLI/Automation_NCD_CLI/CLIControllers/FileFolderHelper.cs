using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation_NCD_CLI.CLIControllers
{
    class FileFolderHelper
    {
        public static void EmptyDirectory(DirectoryInfo directory)
        {
            // Delete all file and subdirectories recursively
            Array.ForEach(directory.GetFiles(), delegate (FileInfo file) { file.Delete(); });
            Array.ForEach(directory.GetDirectories(), delegate (DirectoryInfo subDirectory)
            {
                EmptyDirectory(subDirectory);
                subDirectory.Delete();
            });
        }
    }
}
