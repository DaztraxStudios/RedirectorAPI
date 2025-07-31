using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO;

namespace RedirectorAPI.Middleware
{
    public class RedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _jsonPath;
        private Dictionary<string, string> _redirects = new();
        private readonly object _lock = new();

        public RedirectMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _jsonPath = Path.Combine(env.ContentRootPath, "redirect.json");

            EnsureJsonExists();
            LoadRedirects();

            var watcher = new FileSystemWatcher(Path.GetDirectoryName(_jsonPath)!)
            {
                Filter = Path.GetFileName(_jsonPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
            };

            watcher.Changed += (s, e) =>
            {
                Console.WriteLine("[INFO] redirect.json modified, reloading...");
                LoadRedirects();
            };

            watcher.EnableRaisingEvents = true;
        }

        private void EnsureJsonExists()
        {
            if (!File.Exists(_jsonPath))
            {
                Console.WriteLine("[INFO] redirect.json not detected, creating file...");

                var initial = new Dictionary<string, string>
                {
                    { "/", "https://www.daztraxstudios.com" },
                    { "/home", "https://www.daztraxstudios.com" },
                    { "/lez", "https://lez.daztraxstudios.com" },
                    { "/label", "https://label.daztraxstudios.com" },
                    { "/dev", "https://dev.daztraxstudios.com" },
                    { "/api", "https://dev.daztraxstudios.com" },
                    { "/developer", "https://dev.daztraxstudios.com" },
                };

                File.WriteAllText(_jsonPath, JsonConvert.SerializeObject(initial, Formatting.Indented));
                Console.WriteLine("[INFO] redirect.json file created.");
            }
        }

        private void LoadRedirects()
        {
            lock (_lock)
            {
                try
                {
                    var json = File.ReadAllText(_jsonPath);
                    _redirects = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new();
                    Console.WriteLine("[INFO] Redirects updated.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error on reading \"redirect.json\": {ex}");
                }
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.ToString(); // Incluye "/"

            if (_redirects.TryGetValue(requestPath, out var targetUrl))
            {
                Console.WriteLine($"[REDIRECT] {requestPath} -> {targetUrl}");
                context.Response.Redirect(targetUrl, permanent: false);
                return;
            }

            await _next(context);
        }
    }
}
