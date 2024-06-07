using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkCompositeBackgroundDefinition
{
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