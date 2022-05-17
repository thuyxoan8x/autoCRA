using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
