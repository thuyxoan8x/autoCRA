using AlloyTemplates.Models.Pages;
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
    [PropertyDataValueConverter(typeof(CustomUserPropertyModel))]
    internal class PropertyCustomUserConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            return (propertyModel as CustomUserPropertyModel).Value;
        }
    }
}
