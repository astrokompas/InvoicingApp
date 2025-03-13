using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace InvoicingApp.DataStorage
{
    public class JsonStorage<T> : IDataStorage<T> where T : class, IEntity
    {
        private readonly string _folderPath;
        private readonly string _fileExtension;

        // Add an optional cache to improve performance
        private Dictionary<string, T> _itemCache = new Dictionary<string, T>();
        private bool _isAllItemsCached = false;
        private readonly object _cacheLock = new object();

        public JsonStorage(string folderPath, string fileExtension = ".json")
        {
            _folderPath = folderPath;
            _fileExtension = fileExtension;

            // Ensure the directory exists
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
        }

        public async Task<T> GetByIdAsync(string id)
        {
            // Try to get from cache first
            lock (_cacheLock)
            {
                if (_itemCache.TryGetValue(id, out T cachedItem))
                {
                    return cachedItem;
                }
            }

            string filePath = GetFilePath(id);
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var item = JsonSerializer.Deserialize<T>(json);

                // Add to cache
                lock (_cacheLock)
                {
                    _itemCache[id] = item;
                }

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading item {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Check if all items are already cached
            lock (_cacheLock)
            {
                if (_isAllItemsCached)
                {
                    return _itemCache.Values.ToList();
                }
            }

            try
            {
                var filePaths = Directory.GetFiles(_folderPath, $"*{_fileExtension}");

                // Use Task.WhenAll to read all files in parallel for better performance
                var readTasks = filePaths.Select(async filePath =>
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(filePath);
                        return JsonSerializer.Deserialize<T>(json);
                    }
                    catch (Exception ex)
                    {
                        string fileName = Path.GetFileName(filePath);
                        Console.WriteLine($"Error reading file {fileName}: {ex.Message}");
                        return null;
                    }
                });

                var items = await Task.WhenAll(readTasks);
                var validItems = items.Where(item => item != null).ToList();

                // Update cache
                lock (_cacheLock)
                {
                    _itemCache.Clear();
                    foreach (var item in validItems)
                    {
                        _itemCache[item.Id] = item;
                    }
                    _isAllItemsCached = true;
                }

                return validItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all items: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task SaveAsync(T item)
        {
            // Ensure the item has an ID
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            string filePath = GetFilePath(item.Id);
            string json = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await File.WriteAllTextAsync(filePath, json);

                // Update cache
                lock (_cacheLock)
                {
                    _itemCache[item.Id] = item;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving item {item.Id}: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            string filePath = GetFilePath(id);

            try
            {
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));

                    // Remove from cache
                    lock (_cacheLock)
                    {
                        _itemCache.Remove(id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<T>> QueryAsync(Func<T, bool> predicate)
        {
            var allItems = await GetAllAsync();
            return allItems.Where(predicate).ToList();
        }

        private string GetFilePath(string id)
        {
            return Path.Combine(_folderPath, $"{id}{_fileExtension}");
        }

        // Add a method to invalidate the cache
        public void InvalidateCache()
        {
            lock (_cacheLock)
            {
                _itemCache.Clear();
                _isAllItemsCached = false;
            }
        }
    }
}