using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using AM2E.IO;
using GameContent;
using Microsoft.Xna.Framework;

/*
    * UNQUENCHED THIRST FOR GLORY
    * SETS OFF A LOADED GUN
    * 
    * MISCOUNTED INVENTORY
    * LEAVES YOU WITH CLOSE TO NONE
    * 
    * BETRAY YOUR INHIBITIONS
    * BUT DON'T BETRAY YOUR SON
    * 
    * CHAOS IS YOUR REDEMPTION
    * BETTER HIDE, BETTER RUN
*/

namespace AM2E.Graphics;

public sealed class TexturePage
{
    /// <summary>
    /// Collection of all <see cref="Sprite"/>s contained within this <see cref="TexturePage"/>.
    /// </summary>
    [JsonProperty("sprites")]
    public Dictionary<SpriteIndex, Sprite> Sprites { get; private set; }
    
    /// <summary>
    /// The <see cref="Texture2D"/> represented by this <see cref="TexturePage"/>.
    /// </summary>
    public Texture2D Texture { get; private set; }

    /// <summary>
    /// ONLY TO BE USED BY THE TEXTURE MANAGER
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="???"></param>
    internal TexturePage(Texture2D texture)
    {
        Texture = texture;
        Sprites = new Dictionary<SpriteIndex, Sprite>();
    }

    #region Templated JSON nonsense

    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };

    private static TexturePage FromJson(string json) => JsonConvert.DeserializeObject<TexturePage>(json, Settings);

    #endregion
    
    /// <summary>
    /// Instantiates the <see cref="TexturePage"/> associated with the given <see cref="PageIndex"/>.
    /// </summary>
    /// <param name="index">The index of the <see cref="TexturePage"/> to be instantiated.</param>
    /// <returns>The instantiated <see cref="TexturePage"/>.</returns>
    public static TexturePage Load(PageIndex index)
    {
        // TODO: Make this safe or something lol

        var output = FromJson(File.ReadAllText(AssetManager.GetTextureMetadataPath(index)));
        FileStream fileStream = new(AssetManager.GetTexturePath(index), FileMode.Open);
        output.Texture = Texture2D.FromStream(EngineCore._graphics.GraphicsDevice, fileStream);
        fileStream.Dispose();

        // Assign sprites their TexturePage. Not the fastest thing ever, but I don't think I have any better options due to the direction JSON serializes in.
        // Baking the name earlier in the process results in nullrefs so I believe this is the best option.
        // Probably not that slow in the grand scheme of things.
        foreach (var spr in output.Sprites.Values)
            spr.TexturePage = output;
        
        return output;
    }
}