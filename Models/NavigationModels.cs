using System;
using System.Collections.Generic;

namespace InvoicingApp.Models
{
    public class NavigationParameter
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public NavigationParameter() { }

        public NavigationParameter(string key, object value)
        {
            Add(key, value);
        }

        public void Add(string key, object value)
        {
            _parameters[key] = value;
        }

        public T Get<T>(string key)
        {
            if (_parameters.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        public bool Contains(string key)
        {
            return _parameters.ContainsKey(key);
        }
    }

    public class NavigationHistoryEntry
    {
        public Type ViewModelType { get; set; }
        public NavigationParameter Parameter { get; set; }
    }
}