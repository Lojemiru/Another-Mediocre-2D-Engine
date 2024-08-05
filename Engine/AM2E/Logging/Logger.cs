using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AM2E;

public static class Logger
{
    private static readonly Queue<string> Events = new();
    private static string[] Prefixes = { "Engine", "DEBUG", "INFO", "WARN" };
    private static StreamWriter streamWriter;

    public static LoggingLevel Level = LoggingLevel.Engine;
    public static bool WriteToConsole = false;
    public static bool TracePath = false;
    public static int BufferSize = 10;
    public static Queue<string> Buffer = new();

    public static string[] CrashMessages =
    {
        "When in doubt, blame the engine.",
        "Tell the developer that they can override AM2E.Logger.CrashMessages with their own set of snarky remarks. It's a nice distraction from the fact that their code just blew up.",
        "Yes, these messages are just a thing because I grew up dealing with Minecraft crash outputs. Cope.",
        "All your crash are belong to us.",
        "Was writing my own logging solution really necessary? No, but it adds *personality*!",
        "For your own sanity, I hope the following exception is in your native language.",
        "There are a non-zero number of people alive who will tell you this wouldn't have happened if you used Rust.",
        "Please advise.",
        "And my callstack is down! (oo ooh hoo hoo)",
        "Oo ee oo I look just like an exception.",
        "Blame Canada.",
        "You can do anything you want with code, except whatever you just tried to do.",
        "Error: programmer frustrated successfully.",
        "Works on my machine.",
        "Another happy landing!",
    };

    internal static void Init()
    {
        var logsFolder = "Logs";
        var logPath = logsFolder + "/" + DateTime.Now.ToString("MM-dd-yyyy (HH.mm.ss)") + ".log";
        const int LOGS_COUNT = 5;

        if (!Directory.Exists(logsFolder))
            Directory.CreateDirectory(logsFolder);

        streamWriter = File.Exists(logPath) ? new StreamWriter(File.OpenWrite(logPath)) : File.CreateText(logPath);

        var fileInfos = Directory.GetFiles(logsFolder)
            .Select(file => new FileInfo(file))
            .OrderBy(x => x.CreationTime)
            .ToList();

        while (fileInfos.Count > LOGS_COUNT)
        {
            File.Delete(fileInfos[0].FullName);
            fileInfos.RemoveAt(0);
        }
    }
    
    public static void Log(LoggingLevel level, string message, [CallerFilePath] string path = "no path", 
        [CallerLineNumber] int lineNumber = 0)
    {
        if (level < Level)
            return;

        var log = DateTime.Now.ToString("HH:mm:ss.ffff | ") + Prefixes[(int)level] + " | " + message;

        if (TracePath)
            log += " | " + path + ":" + lineNumber;

        Events.Enqueue(log);
        Buffer.Enqueue(log);
        while (Buffer.Count > BufferSize)
            Buffer.Dequeue();
    }

    internal static void WriteException(Exception e)
    {
        WriteAll();
        
        streamWriter.WriteLine("[----------GAME CRASHED----------]");
        streamWriter.WriteLine(CrashMessages[RNG.Random(CrashMessages.Length() - 1)]);
        streamWriter.WriteLine();
        streamWriter.WriteLine(e);
        streamWriter.WriteLine();
        streamWriter.WriteLine("Thank you for using Another Mediocre 2D Engine. Good luck debugging.");
        
        streamWriter.Flush();
    }

    internal static void WriteAll()
    {
        foreach (var ev in Events)
        {
            if (WriteToConsole)
                Console.WriteLine(ev);
            
            streamWriter.WriteLine(ev);
        }
        
        Events.Clear();
    }
}