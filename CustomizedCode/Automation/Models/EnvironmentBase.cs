using System.Collections.Generic;

namespace AlloyTemplates.Automation.Models
{
    public class EnvironmentBase
    {
        public const string ContentApiRead = "contentapiread";
        public const string ContentApiWrite = "contentapiwrite";
        public const string ApplicationName = "test-info-client";

        public List<VariableItem> Values { get; }

        public EnvironmentBase()
        {
            Values = new List<VariableItem>();
        }
    }
}
