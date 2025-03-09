using System;
using System.Collections.Generic;
using System.Threading;

namespace AM2E.Graphics;

public static class TextureManager
{
    internal static readonly Dictionary<string, WeakReference<TexturePage>> Pages = new();
    private static readonly Dictionary<string, bool> IsLoadingPage = new();
    private static readonly Dictionary<string, bool> IsUnloadingPage = new();
    private static readonly Dictionary<string, Action<TexturePage>> LoadCallbacks = new();
    private static readonly Dictionary<string, Thread> LoadingThreads = new();

    private static void AddPageName(string page)
    {
        if (Pages.ContainsKey(page))
            return;
        
        Pages.Add(page, null);
        LoadingThreads.Add(page, null);
        IsLoadingPage.Add(page, false);
        IsUnloadingPage.Add(page, false);
        LoadCallbacks.Add(page, _ => { });
    }

    private static TexturePage AccessPage(string index)
    {
        AddPageName(index);
        
        // If our page is still loaded, return it!
        if (Pages[index] is not null && Pages[index].TryGetTarget(out var page))
            return page;
        
        // Otherwise, do a blocking load...
        LoadPageBlocking(index);

        // And then return the page.
        Pages[index].TryGetTarget(out page);
        return page;
    }

    public static bool IsPageLoaded(Enum index)
        => IsPageLoaded(index.ToString());
    
    public static bool IsPageLoaded(string index)
    {
        AddPageName(index);
        
        return Pages[index] is not null && Pages[index].TryGetTarget(out _);
    }

    public static void LoadPageBlocking(Enum index)
        => LoadPageBlocking(index.ToString());
    
    public static void LoadPageBlocking(string index)
    {
        if (IsPageLoaded(index))
            return;

        if (IsLoadingPage[index])
        {
            LoadingThreads[index].Join();
        }
        else
        {
            IsLoadingPage[index] = true;
            DoLoad(index);
        }
    }

    public static void LoadPage(Enum index, Action<TexturePage> callback = null)
        => LoadPage(index.ToString(), callback);
    
    public static void LoadPage(string index, Action<TexturePage> callback = null)
    {
        AddPageName(index);
        
        if (Pages[index] is not null && Pages[index].TryGetTarget(out var page))
        {
            callback?.Invoke(page);
            return;
        }

        if (callback is not null)
            LoadCallbacks[index] += callback;

        if (IsLoadingPage[index])
            return;

        IsLoadingPage[index] = true;

        var t = new Thread(() =>
        {
            DoLoad(index);
            if (IsUnloadingPage[index])
                DoUnload(index);
        });

        t.IsBackground = true;
        LoadingThreads[index] = t;
        
        t.Start();
    }

    private static void DoLoad(string index)
    {
        AddPageName(index);

        var page = TexturePage.Load(index);
        
        Pages[index] = new WeakReference<TexturePage>(page);
        LoadCallbacks[index](page);
        LoadCallbacks[index] = _ => { };
        IsLoadingPage[index] = false;
        LoadingThreads[index] = null;
    }

    public static Sprite GetSprite(Enum page, Enum sprite)
        => GetSprite(page.ToString(), sprite.ToString());
    
    public static Sprite GetSprite(string page, string sprite)
    {
        return AccessPage(page).Sprites[sprite];
    }

    public static void UnloadPage(Enum index)
        => UnloadPage(index.ToString());
    
    public static void UnloadPage(string index)
    {
        AddPageName(index);
        
        if (IsLoadingPage[index])
        {
            IsUnloadingPage[index] = true;
            return;
        }
        
        DoUnload(index);
    }

    private static void DoUnload(string index)
    {
        Pages[index] = null;
        IsLoadingPage[index] = false;
        IsUnloadingPage[index] = false;
        LoadCallbacks[index] = _ => { };

        GC.Collect();
    }

    public static void UnloadAll()
    {
        foreach (var thread in LoadingThreads.Values)
        {
            thread?.Join();
        }
        
        LoadingThreads.Clear();
        Pages.Clear();
        IsLoadingPage.Clear();
        IsUnloadingPage.Clear();
        LoadCallbacks.Clear();
        
        GC.Collect();
    }
}