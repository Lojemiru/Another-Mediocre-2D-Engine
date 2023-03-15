using System;
using System.Collections.Generic;
using GameContent;

namespace AM2E.Graphics;

public static class TextureManager
{
    private static Dictionary<PageIndex, TexturePage> pages = new();

    static TextureManager()
    {   
        foreach (var page in Enum.GetValues<PageIndex>())
        {
            pages.Add(page, null);
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

    public static void LoadPage(PageIndex index)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        if (IsPageLoaded(index))
            return;

        pages[index] = TexturePage.Load(index);
    }

    public static TexturePage GetPage(PageIndex index)
    {
        // Ensure page exists/is loaded.
        LoadPage(index);

        return pages[index];
    }

    public static void UnloadPage(PageIndex index)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        pages[index] = null;
    }
}