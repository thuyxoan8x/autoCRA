using System;
using System.Linq;
using Alloy.Sample.Models.Properties;
using Alloy.Sample.Serialization.Models;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagement.ContentApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="CategoryPropertyModel"/> to <see cref="CategoryList"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(AuthorPropertyModel))]
    internal class PropertyAuthorConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            return (propertyModel as AuthorPropertyModel).Value;
        }
    }
}
