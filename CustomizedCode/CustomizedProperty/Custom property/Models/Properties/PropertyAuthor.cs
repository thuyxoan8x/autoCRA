using System;
using EPiServer.Core;
using EPiServer.PlugIn;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using Newtonsoft.Json;

namespace Alloy.Sample.Models.Properties
{
    [PropertyDefinitionTypePlugIn(Description = "A property for author", DisplayName = "Author")]
    public class PropertyAuthor : PropertyLongString
    {
        public override Type PropertyValueType
        {
            get { return typeof(Author); }
        }

        public override object Value
        {
            get
            {
                var value = base.Value as string;

                if (value == null)
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<Author>(value);
            }

            set
            {
                if (value is Author)
                {
                    base.Value = JsonConvert.SerializeObject(value);
                }
                else
                {
                    base.Value = value;
                }
            }
        }

        public override object SaveData(PropertyDataCollection properties)
        {
            return LongString;
        }
    }
    [EditorDescriptorRegistration(TargetType = typeof(Author),
        UIHint = UIHint)]
    public class AuthorEditorDescriptor : EditorDescriptor
    {
        public const string UIHint = "Author";
        private const string AuthorProperty = "alloy/editors/AuthorProperty";

        public AuthorEditorDescriptor()
        {
            ClientEditingClass = AuthorProperty;
        }
    }
}
