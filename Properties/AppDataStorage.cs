using System;
using System.IO;
using Newtonsoft.Json;

namespace Structure
{
    public class CachedLogin
    {
        public string Username { get; set; }
        public DateTime Expiry { get; set; }
    }

    public static class AppDataStorage
    {
        private static readonly string CachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BIMLoginCache.json");

        public static void SaveUser(string username, DateTime expiry)
        {
            var data = new CachedLogin { Username = username, Expiry = expiry };
            File.WriteAllText(CachePath, JsonConvert.SerializeObject(data));
        }

        public static CachedLogin LoadUser()
        {
            if (!File.Exists(CachePath)) return null;

            var json = File.ReadAllText(CachePath);
            return JsonConvert.DeserializeObject<CachedLogin>(json);
        }
    }
}