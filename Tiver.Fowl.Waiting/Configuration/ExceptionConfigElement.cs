namespace Tiver.Fowl.Waiting.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    public class ExceptionConfigElement : ConfigurationElement
    {
        [TypeConverter(typeof(TypeNameConverter))]
        [ConfigurationProperty("type", IsRequired = true, IsKey = true)]
        public Type Type
        {
            get => this["type"] as Type;
            set => this["type"] = value;
        }
    }
}