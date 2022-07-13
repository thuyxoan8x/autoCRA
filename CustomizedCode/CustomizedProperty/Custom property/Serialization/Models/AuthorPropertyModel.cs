using System.Collections.Generic;
using Alloy.Sample.Models.Properties;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace Alloy.Sample.Serialization.Models
{
    public class AuthorPropertyModel : PropertyModel<Author, PropertyAuthor>
    {
        [JsonConstructor]
        internal AuthorPropertyModel() : base(new PropertyAuthor())
        {

        }

        public AuthorPropertyModel(PropertyAuthor propertyAuthor) : base(propertyAuthor)
        {
            Value = propertyAuthor.Value as Author;
        }
    }
}
