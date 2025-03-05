using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvoicingApp.DataStorage
{
    public class JsonStorage<T> : IDataStorage<T> where T : class, IEntity
    {
        private readonly string _folderPath;
        private readonly string _fileExtension;

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
            string filePath = GetFilePath(id);

            if (!File.Exists(filePath))
            {
                return null;
            }

            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            List<T> items = new List<T>();

            foreach (string filePath in Directory.GetFiles(_folderPath, $"*{_fileExtension}"))
            {
                string json = await File.ReadAllTextAsync(filePath);
                T item = JsonSerializer.Deserialize<T>(json);
                items.Add(item);
            }

            return items;
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

            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task DeleteAsync(string id)
        {
            string filePath = GetFilePath(id);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }

        public async Task<IEnumerable<T>> QueryAsync(Func<T, bool> predicate)
        {
            var allItems = await GetAllAsync();
            return new List<T>(allItems).Where(predicate).ToList();
        }

        private string GetFilePath(string id)
        {
            return Path.Combine(_folderPath, $"{id}{_fileExtension}");
        }
    }
}