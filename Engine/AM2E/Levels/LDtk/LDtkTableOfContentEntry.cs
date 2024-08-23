using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkTableOfContentEntry
{
    [JsonProperty("identifier")]
    public string Identifier { get; set; }

    [JsonProperty("instancesData")]
    public LDtkReferenceToAnEntityInstance[] Instances { get; set; }
}