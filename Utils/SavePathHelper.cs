using System;
using System.IO;
using System.Runtime.InteropServices;

public static class SavePathHelper
{
    public static string GetSaveFolder(string gameName)
    {
        string folder;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), gameName, "SaveData");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", gameName, "SaveData");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", gameName, "SaveData");
        }
        else
        {
            // fallback: local folder
            folder = Path.Combine(AppContext.BaseDirectory, "SaveData");
        }

        Directory.CreateDirectory(folder); // ensure it exists
        return folder;
    }
}
