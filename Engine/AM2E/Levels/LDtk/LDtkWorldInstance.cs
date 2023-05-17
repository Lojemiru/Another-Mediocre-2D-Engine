using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkWorldInstance
{
    /// <summary>
    /// All levels. The order of this array is only relevant in `LinearHorizontal` and
    /// `linearVertical` world layouts (see `worldLayout` value).<br/>  Otherwise, you should
    /// refer to the `worldX`,`worldY` coordinates of each Level.
    /// </summary>
    [JsonProperty("levels")]
    public LDtkLightweightLevelInstance[] Levels { get; set; }
    
    [JsonProperty("defs")]
    public LDtkDefinition Defs { get; set; }
    
    /// <summary>
    /// All instances of entities that have their `exportToToc` flag enabled are listed in this
    /// array.
    /// </summary>
    [JsonProperty("toc")]
    public LDtkTableOfContentEntry[] TableOfContent { get; set; }
    
    /// <summary>
    /// Height of the world grid in pixels.
    /// </summary>
    [JsonProperty("worldGridHeight")]
    public int WorldGridHeight { get; set; }

    /// <summary>
    /// Width of the world grid in pixels.
    /// </summary>
    [JsonProperty("worldGridWidth")]
    public int WorldGridWidth { get; set; }
}