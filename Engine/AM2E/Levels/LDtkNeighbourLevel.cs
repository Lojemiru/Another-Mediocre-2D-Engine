using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkNeighbourLevel
{
    /// <summary>
    /// A single lowercase character tipping on the level location (`n`orth, `s`outh, `w`est,
    /// `e`ast).
    /// </summary>
    [JsonProperty("dir")]
    public char Dir { get; set; }

    /// <summary>
    /// Neighbour Instance Identifier
    /// </summary>
    [JsonProperty("levelIid")]
    public string LevelIid { get; set; }

    /// <summary>
    /// **WARNING**: this deprecated value will be *removed* completely on version 1.2.0+
    /// Replaced by: `levelIid`
    /// </summary>
    [JsonProperty("levelUid", NullValueHandling = NullValueHandling.Ignore)]
    public long? LevelUid { get; set; }
}