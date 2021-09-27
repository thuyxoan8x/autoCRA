using System.Collections.Generic;

namespace Automation_NCD_CLI.Models
{
    /// <summary>
    /// Pull Response
    /// </summary>
    public class PullResponse
    {
        /// <summary>
        /// ContentTypes array
        /// </summary>
        public List<ContentType> ContentTypes { get; set; }

        /// <summary>
        /// EditorDefinitions array
        /// </summary>
        public List<EditorDefinition> EditorDefinitions { get; set; }
        
        /// <summary>
        /// PropertyGroups array
        /// </summary>
        public List<PropertyGroup> PropertyGroups { get; set; }
    }
}
