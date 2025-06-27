using Newtonsoft.Json;

namespace AM2E.Levels;

/// <summary>
/// In a tileset definition, enum based tag infos
/// </summary>
public struct LDtkEnumTagValue
{
    [JsonProperty("enumValueId")]
    public string EnumValueId { get; set; }

    [JsonProperty("tileIds")]
    public int[] TileIds { get; set; }
}