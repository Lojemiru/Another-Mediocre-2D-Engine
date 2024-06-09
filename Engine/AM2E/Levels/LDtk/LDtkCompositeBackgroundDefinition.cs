using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkCompositeBackgroundDefinition
{
    /// <summary>
    /// The Backgrounds contained by this CompositeBackground
    /// </summary>
    [JsonProperty("backgrounds")]
    public LDtkBackgroundDefinition[] Backgrounds { get; set; }

    /// <summary>
    /// User defined unique identifier
    /// </summary>
    [JsonProperty("identifier")]
    public string Identifier { get; set; }
    
    /// <summary>
    /// Unique Intidentifier
    /// </summary>
    [JsonProperty("uid")]
    public int Uid { get; set; }
}