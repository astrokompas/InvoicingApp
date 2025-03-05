using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicingApp.Services
{
    public class CacheService<T> where T : class
    {
        private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();
        private readonly Dictionary<string, DateTime> _cacheTimes = new Dictionary<string, DateTime>();
        private readonly TimeSpan _defaultExpirationTime = TimeSpan.FromMinutes(5);

        public void Add(string key, T item, TimeSpan? expirationTime = null)
        {
            _cache[key] = item;
            _cacheTimes[key] = DateTime.Now.Add(expirationTime ?? _defaultExpirationTime);
        }

        public bool TryGetValue(string key, out T value)
        {
            // Check if the key exists and is not expired
            if (_cache.TryGetValue(key, out value) &&
                _cacheTimes.TryGetValue(key, out DateTime expirationTime) &&
                expirationTime > DateTime.Now)
            {
                return true;
            }

            // Remove expired items
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
                _cacheTimes.Remove(key);
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            _cache.Clear();
            _cacheTimes.Clear();
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _cacheTimes.Remove(key);
        }
    }
}