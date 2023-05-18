using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkLightweightLevelInstance
{
    /// <summary>
    /// User defined unique identifier
    /// </summary>
    [JsonProperty("identifier")]
    public string Identifier { get; set; }
    
    /// <summary>
    /// Relative path to the external file providing this Enum
    /// </summary>
    [JsonProperty("externalRelPath")]
    public string ExternalRelPath { get; set; }
    
    /// <summary>
    /// Unique instance identifier
    /// </summary>
    [JsonProperty("iid")]
    public string Iid { get; set; }
}