using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace AM2E.IO;

public static class LocalStorage
{
    internal static void Initialize()
    {
        var path = GetPath();

        if (!Directory.Exists(path))
        {
            Logger.Engine("Local storage not found! Creating at path " + path);
            Directory.CreateDirectory(path);
        }
    }

    public static void SafeAddSubfolder(string path)
    {
        if (!Directory.Exists(GetPath() + "/" + path))
            Directory.CreateDirectory(GetPath() + "/" + path);
    }
    
    public static string GetPath()
    {
        var separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/" : "/.";

        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.None) + 
               separator + EngineCore.LocalStorageName;
    }

    public static bool Exists(string name)
    {
        return File.Exists(GetPath() + "/" + name);
    }

    public static void Write(string name, object data)
    {
        using var writer = File.CreateText(GetPath() + "/" + name);
        var serializer = new JsonSerializer();
        serializer.Serialize(writer, data);
    }

    public static void WriteAsync(string name, object data, Action callback = null)
    {
        var t = new Thread(() =>
        {
            Write(name, data);
            callback?.Invoke();
        });
        t.IsBackground = true;
        t.Start();
    }

    public static void Read<T>(string name, out T data)
    {
        if (!Exists(name))
        {
            data = default;
            Write(name, data);
            return;
        }

        var text = File.ReadAllText(GetPath() + "/" + name);
        data = JsonConvert.DeserializeObject<T>(text);
    }

    public static void Delete(string name)
    {
        var path = GetPath() + "/" + name;
        if (File.Exists(path))
            File.Delete(path);
    }
}