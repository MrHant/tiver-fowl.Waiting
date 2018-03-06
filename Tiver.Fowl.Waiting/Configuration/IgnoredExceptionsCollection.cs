namespace Tiver.Fowl.Waiting.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ExceptionConfigElement), AddItemName = "Exception", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class IgnoredExceptionsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExceptionConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExceptionConfigElement)element).Type.ToString();
        }

        public ExceptionConfigElement this[int index]
        {
            get => (ExceptionConfigElement)BaseGet(index);
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public ExceptionConfigElement this[string type] => (ExceptionConfigElement)BaseGet(type);

        public int IndexOf(ExceptionConfigElement exception)
        {
            return BaseIndexOf(exception);
        }

        public void Add(ExceptionConfigElement exception)
        {
            BaseAdd(exception);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(ExceptionConfigElement exception)
        {
            if (BaseIndexOf(exception) >= 0)
            {
                BaseRemove(exception.Type.ToString());
            }
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string type)
        {
            BaseRemove(type);
        }

        public void Clear()
        {
            BaseClear();
        }
    }
}