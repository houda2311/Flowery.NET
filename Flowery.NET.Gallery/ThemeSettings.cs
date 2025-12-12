using System;
using System.IO;

namespace Flowery.NET.Gallery;

public static class ThemeSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Flowery.NET.Gallery",
        "theme.txt");

    private static readonly string LanguagePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Flowery.NET.Gallery",
        "language.txt");

    public static string? Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return File.ReadAllText(SettingsPath).Trim();
        }
        catch { }
        return null;
    }

    public static void Save(string themeName)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, themeName);
        }
        catch { /* ignore */ }
    }

    public static string? LoadLanguage()
    {
        try
        {
            if (File.Exists(LanguagePath))
                return File.ReadAllText(LanguagePath).Trim();
        }
        catch { }
        return null;
    }

    public static void SaveLanguage(string cultureName)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LanguagePath)!);
            File.WriteAllText(LanguagePath, cultureName);
        }
        catch { /* ignore */ }
    }
}

