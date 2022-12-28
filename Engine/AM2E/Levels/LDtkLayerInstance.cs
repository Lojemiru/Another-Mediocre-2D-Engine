using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkLayerInstance
{
    /// <summary>
    /// Grid size
    /// </summary>
    [JsonProperty("__gridSize")]
    public long GridSize { get; set; }

    /// <summary>
    /// Layer definition identifier
    /// </summary>
    [JsonProperty("__identifier")]
    public string Identifier { get; set; }

    /// <summary>
    /// The definition UID of corresponding Tileset, if any.
    /// </summary>
    // TODO: this or the next one need to get converted into our actual sprite reference or something. Ugh. 
    [JsonProperty("__tilesetDefUid")]
    public long? TilesetDefUid { get; set; }

    /// <summary>
    /// The relative path to corresponding Tileset, if any.
    /// </summary>
    [JsonProperty("__tilesetRelPath")]
    public string TilesetRelPath { get; set; }

    /// <summary>
    /// Layer type (possible values: IntGrid, Entities, Tiles or AutoLayer)
    /// </summary>
    [JsonProperty("__type")]
    public LDtkLayerType Type { get; set; }

    [JsonProperty("entityInstances")]
    public LDtkEntityInstance[] EntityInstances { get; set; }

    [JsonProperty("gridTiles")]
    public LDtkTileInstance[] GridTiles { get; set; }

    /// <summary>
    /// Layer instance visibility
    /// </summary>
    [JsonProperty("visible")]
    public bool Visible { get; set; }
}