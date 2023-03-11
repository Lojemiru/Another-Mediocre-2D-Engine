using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using AM2E.IO;

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

public class TexturePage
{
    public Texture2D Texture { get; private set; }

    [JsonProperty("sprites")]
    public Dictionary<SpriteIndex, Sprite> Sprites { get; private set; }

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

    public static TexturePage Load(PageIndex index)
    {
        // TODO: Make this safe or something lol

        var output = FromJson(File.ReadAllText(AssetManager.GetTextureMetadataPath(index)));
        FileStream fileStream = new(AssetManager.GetTexturePath(index), FileMode.Open);
        output.Texture = Texture2D.FromStream(EngineCore._graphics.GraphicsDevice, fileStream);
        fileStream.Dispose();

        // Assign sprites their TexturePage. Not the fastest thing ever, but I don't think I have any better options due to the direction JSON serializes in.
        // Unless we bake the name earlier in the process... that might be worth doing.
        // TODO: above.
        foreach (var spr in output.Sprites.Values)
        {
            spr.TexturePage = output;
        }

        return output;
    }
}