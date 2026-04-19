using System;
using System.IO;
using System.Text.Json;

namespace SimpleDPI;

public class AppSettings
{
    private static readonly string AppVersion = "v1.0.57"; // Match the current version
    private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleDPI", AppVersion);
    private static readonly string SettingsPath = Path.Combine(AppDataFolder, "settings.json");
    
    public const string DefaultArguments = "-5 --dns-addr 77.88.8.8 --dns-port 1253 --dnsv6-addr 2a02:6b8::feed:0ff --dnsv6-port 1253";

    public string Arguments { get; set; } = DefaultArguments;
    public string? Language { get; set; } = null;
    
    public bool EnableBlur { get; set; } = true;
    public bool StartOnBoot { get; set; } = false;
    public bool AutoStartService { get; set; } = false;

    public static AppSettings Load()
    {
        if (File.Exists(SettingsPath))
        {
            try
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(AppDataFolder)) Directory.CreateDirectory(AppDataFolder);
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
