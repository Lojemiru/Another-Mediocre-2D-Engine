using Newtonsoft.Json;

namespace AM2E.Levels;

public sealed class LDtkFieldInstanceEntityReference
{
    /// <summary>
    /// IID of the refered EntityInstance
    /// </summary>
    [JsonProperty("entityIid")]
    public string EntityIid { get; private set; }

    /// <summary>
    /// IID of the LayerInstance containing the refered EntityInstance
    /// </summary>
    [JsonProperty("layerIid")]
    public string LayerIid { get; private set; }

    /// <summary>
    /// IID of the Level containing the refered EntityInstance
    /// </summary>
    [JsonProperty("levelIid")]
    public string LevelIid { get; private set; }

    public static LDtkFieldInstanceEntityReference FromDynamic(dynamic input)
    {
        try
        {
            return new LDtkFieldInstanceEntityReference
            {
                EntityIid = input.entityIid,
                LayerIid = input.layerIid,
                LevelIid = input.levelIid
            };
        }
        catch
        {
            return null;
        }
    }
}