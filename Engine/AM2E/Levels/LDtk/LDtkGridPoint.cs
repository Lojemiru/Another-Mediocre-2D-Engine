using Newtonsoft.Json;

namespace AM2E.Levels;

public class LDtkGridPoint
{
    /// <summary>
    /// X grid-based coordinate
    /// </summary>
    [JsonProperty("cx")]
    public int X { get; private set; }

    /// <summary>
    /// Y grid-based coordinate
    /// </summary>
    [JsonProperty("cy")]
    public int Y { get; private set; }

    public static LDtkGridPoint FromDynamic(dynamic input)
    {
        try
        {
            return new LDtkGridPoint
            {
                X = (int)input.cx,
                Y = (int)input.cy
            };
        }
        catch
        {
            return default;
        }
    }
}