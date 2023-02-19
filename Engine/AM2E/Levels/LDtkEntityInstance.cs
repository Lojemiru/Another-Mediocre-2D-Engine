using System;
using Newtonsoft.Json;

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
    public long Height { get; set; }

    /// <summary>
    /// Unique instance identifier
    /// </summary>
    // TODO: Are we going to use this? Probably not.
    [JsonProperty("iid")]
    public string Iid { get; set; }

    /// <summary>
    /// Pixel coordinates (`[x,y]` format) in current level coordinate space. Don't forget
    /// optional layer offsets, if they exist!
    /// </summary>
    [JsonProperty("px")]
    public long[] Px { get; set; }

    /// <summary>
    /// Entity width in pixels. For non-resizable entities, it will be the same as Entity
    /// definition.
    /// </summary>
    [JsonProperty("width")]
    public long Width { get; set; }
    
    /// <summary>
    /// "Flip bits", a 2-bits integer to represent the mirror transformations of the entity.<br/>
    /// - Bit 0 = X flip<br/>   - Bit 1 = Y flip<br/>   Examples: f=0 (no flip), f=1 (X flip
    /// only), f=2 (Y flip only), f=3 (both flips)
    /// </summary>
    [JsonProperty("f")]
    public int F { get; set; }
}