using System;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Serialization;
using EPiServer.Framework.Serialization.Internal;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace Alloy.Sample.Models.Properties
{
    public class PropertyListBase<T> : PropertyList<T>
    {
        public PropertyListBase()
        {
            _objectSerializer = this._objectSerializerFactory.Service.GetSerializer("application/json");
        }
        private Injected<ObjectSerializerFactory> _objectSerializerFactory;

        private IObjectSerializer _objectSerializer;
        protected override T ParseItem(string value)
        {
            return _objectSerializer.Deserialize<T>(value);
        }
#pragma warning disable CS0672 // Member overrides obsolete member
        public override PropertyData ParseToObject(string value)
#pragma warning restore CS0672 // Member overrides obsolete member
        {
            ParseToSelf(value);
            return this;
        }
    }
}
