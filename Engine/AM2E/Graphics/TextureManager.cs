using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AM2E.IO;
using GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class TextureManager
{
    private static ConcurrentDictionary<PageIndex, TexturePage> pages = new();
    private static ConcurrentDictionary<PageIndex, bool> isLoadingPage = new();
    private static ConcurrentDictionary<PageIndex, Action<TexturePage>> loadCallbacks = new();

    static TextureManager()
    {   
        foreach (var page in Enum.GetValues<PageIndex>())
        {
            pages.TryAdd(page, TexturePage.Load(page));
            isLoadingPage.TryAdd(page, false);
            loadCallbacks.TryAdd(page, _ => { });
        }
    }

    public static bool PageExists(PageIndex index)
    {
        return pages.ContainsKey(index);
    }

    public static bool IsPageLoaded(PageIndex index)
    {
        return pages[index] != null;
    }

    public static void LoadPageBlocking(PageIndex index)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        if (IsPageLoaded(index))
            return;

        isLoadingPage[index] = true;
        pages[index] = TexturePage.Load(index);
        loadCallbacks[index](pages[index]);
        loadCallbacks[index] = _ => { };
        isLoadingPage[index] = false;
    }

    public static void LoadPage(PageIndex index, Action<TexturePage> callback = null)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        if (IsPageLoaded(index))
        {
            callback?.Invoke(pages[index]);
            return;
        }

        if (callback is not null)
            loadCallbacks[index] += callback;

        if (isLoadingPage[index])
            return;
        
        isLoadingPage[index] = true;

        var t = new Thread(() =>
        {
            if (!isLoadingPage[index])
                return;
            
            pages[index] = TexturePage.Load(index);
            loadCallbacks[index](pages[index]);
            loadCallbacks[index] = _ => { };
            isLoadingPage[index] = false;
        })
        {
            IsBackground = true
        };
        
        t.Start();
    }

    public static Sprite GetSprite(PageIndex page, SpriteIndex sprite)
    {
        LoadPageBlocking(page);
        
        return pages[page].Sprites[sprite];
    }

    public static void UnloadPage(PageIndex index)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        pages[index] = null;
        
        GC.Collect();
    }
}