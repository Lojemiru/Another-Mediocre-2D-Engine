using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkTileInstance
{
    /// <summary>
    /// "Flip bits", a 2-bits integer to represent the mirror transformations of the tile.<br/>
    /// - Bit 0 = X flip<br/>   - Bit 1 = Y flip<br/>   Examples: f=0 (no flip), f=1 (X flip
    /// only), f=2 (Y flip only), f=3 (both flips)
    /// </summary>
    [JsonProperty("f")]
    public byte F { get; set; }

    /// <summary>
    /// Pixel coordinates of the tile in the **layer** (`[x,y]` format). Don't forget optional
    /// layer offsets, if they exist!
    /// </summary>
    [JsonProperty("px")]
    public int[] Px { get; set; }

    /// <summary>
    /// Pixel coordinates of the tile in the **tileset** (`[x,y]` format)
    /// </summary>
    [JsonProperty("src")]
    public int[] Src { get; set; }

    /// <summary>
    /// The *Tile ID* in the corresponding tileset.
    /// </summary>
    [JsonProperty("t")]
    public int T { get; set; }
}