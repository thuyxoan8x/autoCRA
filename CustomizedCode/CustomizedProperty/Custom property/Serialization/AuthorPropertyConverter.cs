using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alloy.Sample.Models.Properties;
using Alloy.Sample.Serialization.Models;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;

namespace Alloy.Sample.Serialization
{
    public class AuthorPropertyConverter : IPropertyConverter
    {
        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext)
        {
            if (propertyData is PropertyAuthor propertyAuthor)
            {
                return new AuthorPropertyModel(propertyAuthor);
            }

            return null;
        }
    }
}
