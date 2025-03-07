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
    [JsonProperty("__tilesetDefUid")]
    public int? TilesetDefUid { get; set; }

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
    
    [JsonProperty("autoLayerTiles")]
    public LDtkTileInstance[] AutoLayerTiles { get; set; }

    /// <summary>
    /// Layer instance visibility
    /// </summary>
    [JsonProperty("visible")]
    public bool Visible { get; set; }
}