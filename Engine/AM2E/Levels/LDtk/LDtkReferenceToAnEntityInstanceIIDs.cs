using Newtonsoft.Json;

namespace AM2E.Levels;

public class LDtkReferenceToAnEntityInstanceIids
{
    /// <summary>
    /// IID of the referred EntityInstance
    /// </summary>
    [JsonProperty("entityIid")]
    public string EntityIid { get; set; }

    /// <summary>
    /// IID of the LayerInstance containing the referred EntityInstance
    /// </summary>
    [JsonProperty("layerIid")]
    public string LayerIid { get; set; }

    /// <summary>
    /// IID of the Level containing the referred EntityInstance
    /// </summary>
    [JsonProperty("levelIid")]
    public string LevelIid { get; set; }

    /// <summary>
    /// IID of the World containing the referred EntityInstance
    /// </summary>
    [JsonProperty("worldIid")]
    public string WorldIid { get; set; }
}