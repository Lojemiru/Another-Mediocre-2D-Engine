using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkBackgroundDefinition
{
    /// <summary>
    /// User defined unique identifier
    /// </summary>
    [JsonProperty("identifier")]
    public string Identifier { get; set; }
    
    [JsonProperty("parallaxX")]
    public float ParallaxX { get; set; }
    
    [JsonProperty("parallaxY")]
    public float ParallaxY { get; set; }
    
    [JsonProperty("pivotX")]
    public float PivotX { get; set; }
    
    [JsonProperty("pivotY")]
    public float PivotY { get; set; }
    
    [JsonProperty("pos")]
    public LDtkLevelBackgroundPosition Pos { get; set; }

    /// <summary>
    /// Path to the source file, relative to the current project JSON file<br/>  It can be null
    /// if no image was provided, or when using an embed atlas.
    /// </summary>
    [JsonProperty("relPath")]
    public string RelPath { get; set; }
    
    [JsonProperty("repeatX")]
    public bool RepeatX { get; set; }
    
    [JsonProperty("repeatY")]
    public bool RepeatY { get; set; }

    /// <summary>
    /// Unique Intidentifier
    /// </summary>
    [JsonProperty("uid")]
    public int Uid { get; set; }
}