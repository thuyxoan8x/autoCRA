using AlloyTemplates.Models.Pages;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;

namespace Alloy.Sample.Serialization
{
    public class CustomUserPropertyConverter : IPropertyConverter
    {
        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext)
        {
            if (propertyData is PropertyCustomUser propertyCustomUser)
            {
                return new CustomUserPropertyModel(propertyCustomUser);
            }

            return null;
        }
    }
}
