using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkNeighbourLevel
{
    /// <summary>
    /// A lowercase string tipping on the level location (`n`orth, `s`outh, `w`est,
    /// `e`ast).  Since 1.4.0, this value can also be the less than symbol (neighbour depth is lower),
    /// greater than symbol (neighbour depth is greater) or `o` (levels overlap and share the same world
    /// depth). Since 1.5.3, this value can also be `nw`,`ne`,`sw` or `se` for levels only
    /// touching corners.
    /// </summary>
    [JsonProperty("dir")]
    public string Dir { get; set; }

    /// <summary>
    /// Neighbour Instance Identifier
    /// </summary>
    [JsonProperty("levelIid")]
    public string LevelIid { get; set; }
}