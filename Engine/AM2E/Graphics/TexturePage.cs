using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Xna.Framework.Graphics;
using AM2E.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

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
    public Dictionary<string, Sprite> Sprites { get; private set; }
    
    /// <summary>
    /// The <see cref="Texture2D"/> represented by this <see cref="TexturePage"/>.
    /// </summary>
    public Texture2D Texture { get; private set; }

    #region Templated JSON nonsense

    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
            new RectangleJsonConverter()
        },
    };

    private static TexturePage FromJson(string json) 
        => JsonConvert.DeserializeObject<TexturePage>(json, Settings);
    
    private class RectangleJsonConverter : JsonConverter<Rectangle>
    {
        public override Rectangle ReadJson(JsonReader reader, Type objectType, Rectangle existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            return new Rectangle(
                obj["X"]?.Value<int>() ?? 0,
                obj["Y"]?.Value<int>() ?? 0,
                obj["Width"]?.Value<int>() ?? 0,
                obj["Height"]?.Value<int>() ?? 0
            );
        }

        public override void WriteJson(JsonWriter writer, Rectangle value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("X");
            writer.WriteValue(value.X);

            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);

            writer.WritePropertyName("Width");
            writer.WriteValue(value.Width);

            writer.WritePropertyName("Height");
            writer.WriteValue(value.Height);

            writer.WriteEndObject();
        }
    }
        
    #endregion

    public static TexturePage Load(Enum index)
        => Load(index.ToString());
    
    
    /// <summary>
    /// Instantiates the <see cref="TexturePage"/> associated with the given <see cref="PageIndex"/>.
    /// </summary>
    /// <param name="index">The index of the <see cref="TexturePage"/> to be instantiated.</param>
    /// <returns>The instantiated <see cref="TexturePage"/>.</returns>
    public static TexturePage Load(string index)
    {
        string fileText;
        TexturePage output;
        FileStream fileStream;

        // Try to load file as text...
        try
        {
            fileText = File.ReadAllText(AssetManager.GetTextureMetadataPath(index));
        }
        catch (FileNotFoundException e)
        {
            throw new FileNotFoundException("Unable to find metadata file for page \"" + index + "\"\n" + e.StackTrace);
        }

        // Then try to convert to JSON...
        try
        {
            output = FromJson(fileText);
        }
        catch (JsonReaderException e)
        {
            throw new JsonReaderException("Error loading JSON for page \"" + index + "\":" + e.Message + "\n" + e.StackTrace);
        }

        // Ensure we didn't get handed null.
        if (output is null)
        {
            throw new NullReferenceException("Error loading metadata file for page \"" + index + "\": JSON conversion returned null.");
        }

        // Then try to open a FileStream to the texture atlas...
        try
        {
            fileStream = new FileStream(AssetManager.GetTexturePath(index), FileMode.Open);
        }
        catch (FileNotFoundException e)
        {
            throw new FileNotFoundException("Unable to find texture file for page \"" + index + "\"\n" + e.StackTrace);
        }

        // Then try to convert said FileStream to the actual Texture2D...
        try
        {
            output.Texture = Texture2D.FromStream(EngineCore._graphics.GraphicsDevice, fileStream);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException("Found unsupported image format while loading texture for page \"" + index + "\". What are you doing???\n" + e.StackTrace);
        }

        fileStream.Dispose();
        
        // Assign sprites their TexturePage. Not the fastest thing ever, but I don't think I have any better options due to the direction JSON serializes in.
        // Baking the name earlier in the process results in nullrefs so I believe this is the best option.
        // Probably not that slow in the grand scheme of things.
        foreach (var spr in output.Sprites)
        {
            spr.Value.Name = spr.Key;
            spr.Value.TexturePage = output;
        }

        return output;
    }
}