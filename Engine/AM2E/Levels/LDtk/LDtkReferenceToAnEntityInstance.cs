using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkReferenceToAnEntityInstance
{
    /// <summary>
    /// IID of the refered EntityInstance
    /// </summary>
    [JsonProperty("entityIid")]
    public string EntityIid { get; set; }

    /// <summary>
    /// IID of the LayerInstance containing the refered EntityInstance
    /// </summary>
    [JsonProperty("layerIid")]
    public string LayerIid { get; set; }

    /// <summary>
    /// IID of the Level containing the refered EntityInstance
    /// </summary>
    [JsonProperty("levelIid")]
    public string LevelIid { get; set; }

    /// <summary>
    /// IID of the World containing the refered EntityInstance
    /// </summary>
    [JsonProperty("worldIid")]
    public string WorldIid { get; set; }
}