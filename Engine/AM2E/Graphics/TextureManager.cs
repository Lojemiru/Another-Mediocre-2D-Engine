using System;
using System.Collections.Generic;
using System.Threading;
using GameContent;

namespace AM2E.Graphics;

public static class TextureManager
{
    private static readonly Dictionary<PageIndex, TexturePage> Pages = new();
    private static readonly Dictionary<PageIndex, bool> IsLoadingPage = new();
    private static readonly Dictionary<PageIndex, bool> IsUnloadingPage = new();
    private static readonly Dictionary<PageIndex, Action<TexturePage>> LoadCallbacks = new();
    private static readonly Dictionary<PageIndex, Thread> LoadingThreads = new();

    static TextureManager()
    {   
        foreach (var page in Enum.GetValues<PageIndex>())
        {
            Pages.Add(page, null);
            LoadingThreads.Add(page, null);
            IsLoadingPage.Add(page, false);
            IsUnloadingPage.Add(page, false);
            LoadCallbacks.Add(page, _ => { });
        }
    }

    public static bool IsPageLoaded(PageIndex index)
    {
        return Pages[index] != null;
    }

    public static void LoadPageBlocking(PageIndex index)
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

    public static void LoadPage(PageIndex index, Action<TexturePage> callback = null)
    {
        if (IsPageLoaded(index))
        {
            callback?.Invoke(Pages[index]);
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

    private static void DoLoad(PageIndex index)
    {
        Pages[index] = TexturePage.Load(index);
        LoadCallbacks[index](Pages[index]);
        LoadCallbacks[index] = _ => { };
        IsLoadingPage[index] = false;
        LoadingThreads[index] = null;
    }

    public static Sprite GetSprite(PageIndex page, SpriteIndex sprite)
    {
        LoadPageBlocking(page);
        
        return Pages[page].Sprites[sprite];
    }

    public static void UnloadPage(PageIndex index)
    {
        if (IsLoadingPage[index])
        {
            IsUnloadingPage[index] = true;
            return;
        }
        
        DoUnload(index);
    }

    private static void DoUnload(PageIndex index)
    {
        Pages[index] = null;
        IsUnloadingPage[index] = false;
        LoadCallbacks[index] = _ => { };

        GC.Collect();
    }
}