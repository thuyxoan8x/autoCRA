using AlloyTemplates.Models.Blocks;
using EPiServer;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.PlugIn;
using EPiServer.SpecializedProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlloyTemplates.Models.Pages
{
    public class CustomUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [PropertyDefinitionTypePlugIn]
    public class PropertyCustomUser : PropertyLongString
    {
        public override Type PropertyValueType => typeof(CustomUser);

        /// <inheritdoc />
        public override object Value
        {
            get
            {
                var value = base.Value as string;
                if (value == null)
                {
                    return null;
                }
                var parts = value.Split(',');
                return new CustomUser
                {
                    FirstName = parts[0],
                    LastName = parts[1]
                };
            }
            set
            {
                SetPropertyValue(value, delegate ()
                {
                    if (value is string stringValue)
                    {
                        base.Value = stringValue;
                    }
                    else if (value is CustomUser poco)
                    {
                        base.Value = $"{poco.FirstName},{poco.LastName}";
                    }
                    else
                    {
                        throw new NotSupportedException("value should string or CustomPoco");
                    }
                });
            }
        }

        public override void LoadData(object value) => base.LongString = value as string;
        public override object SaveData(PropertyDataCollection properties) => base.LongString;

        public CustomUser User
        {
            get { return Value as CustomUser; }
            set { Value = value; }
        }
    }

    public class CustomUserPropertyModel : PropertyModel<CustomUser, PropertyCustomUser>
    {
        [Newtonsoft.Json.JsonConstructor]
        internal CustomUserPropertyModel() : base(new PropertyCustomUser())
        {

        }

        public CustomUserPropertyModel(PropertyCustomUser propertyCustomUser) : base(propertyCustomUser)
        {
            Value = propertyCustomUser.Value as CustomUser;
        }
    }
}
