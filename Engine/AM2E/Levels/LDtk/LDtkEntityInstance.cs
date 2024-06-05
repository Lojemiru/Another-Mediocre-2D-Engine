using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AM2E.Levels;

public struct LDtkEntityInstance
{
    /// <summary>
    /// Entity definition identifier
    /// </summary>
    [JsonProperty("__identifier")]
    public string Identifier { get; set; }

    /// <summary>
    /// An array of all custom fields and their values.
    /// </summary>
    [JsonProperty("fieldInstances")]
    public LDtkFieldInstance[] FieldInstances { get; set; }

    /// <summary>
    /// Entity height in pixels. For non-resizable entities, it will be the same as Entity
    /// definition.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Unique instance identifier
    /// </summary>
    [JsonProperty("iid")]
    public string Iid { get; set; }

    /// <summary>
    /// Pixel coordinates (`[x,y]` format) in current level coordinate space. Don't forget
    /// optional layer offsets, if they exist!
    /// </summary>
    [JsonProperty("px")]
    public int[] Px { get; set; }

    /// <summary>
    /// Entity width in pixels. For non-resizable entities, it will be the same as Entity
    /// definition.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; set; }
    
    /// <summary>
    /// "Flip bits", a 2-bits integer to represent the mirror transformations of the entity.<br/>
    /// - Bit 0 = X flip<br/>   - Bit 1 = Y flip<br/>   Examples: f=0 (no flip), f=1 (X flip
    /// only), f=2 (Y flip only), f=3 (both flips)
    /// </summary>
    [JsonProperty("f")]
    public byte F { get; set; }

    public dynamic GetFieldInstance(string identifier)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var field in FieldInstances)
        {
            if (field.Identifier != identifier)
                continue;
            
            // LDtk will pass us floats as doubles... even though they're called floats in the editor.
            // This could also just be a JSON issue. Either way, I'd rather catch it here than on the other end every time...
            if (field.Type == "Float")
                return (float)field.Value;

            // Ints report back as doubles, I believe. This forces them to be typed appropriately.
            if (field.Type == "Int")
                return (int)field.Value;

            // Colors are passed as a string in the format #RRGGBB.
            // This is very annoying to parse, but I've done it nonetheless.
            if (field.Type == "Color")
            {
                var r = Convert.ToInt32(field.Value.Substring(1, 2), 16);
                var g = Convert.ToInt32(field.Value.Substring(3, 2), 16);
                var b = Convert.ToInt32(field.Value.Substring(5, 2), 16);
                return new Color(r, g, b);
            }

            if (field.Type == "Point")
                return LDtkGridPoint.FromDynamic(field.Value);

            if (field.Type == "EntityRef")
                return LDtkFieldInstanceEntityReference.FromDynamic(field.Value);

            return field.Value;
        }

        return null;
    }

    public T[] GetFieldInstanceArray<T>(string identifier)
    {
        // LDtk hands us a JArray instead of an actual array. This should resolve that before any other code sees that.
        
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var field in FieldInstances)
        {
            if (field.Value is not JArray array || field.Identifier != identifier)
                continue;

            return array.ToObject<T[]>();
        }

        return null;
    }
}