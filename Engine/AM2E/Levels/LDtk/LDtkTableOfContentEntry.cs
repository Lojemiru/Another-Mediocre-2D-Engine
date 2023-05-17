using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkTableOfContentEntry
{
    [JsonProperty("identifier")]
    public string Identifier { get; set; }

    [JsonProperty("instances")]
    public LDtkReferenceToAnEntityInstance[] Instances { get; set; }
}