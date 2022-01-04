using System.Collections.Generic;

namespace Automation_NCD_CLI.Models
{
    /// <summary>
    /// Content Types
    /// </summary>
    public class ContentType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseType { get; set; }
        public string Version { get; set; }
        public List<Property> Properties { get; set; }
    }

    /// <summary>
    /// Content type property
    /// </summary>
    public class Property
    {
        public string Name { get; set; }
        public string DataType { get; set; }
    }

    public class EditorDefinition
    {

    }

    public class PropertyGroup
    {
        public string Name { get; set; }
    }
}
