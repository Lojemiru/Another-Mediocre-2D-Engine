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
}