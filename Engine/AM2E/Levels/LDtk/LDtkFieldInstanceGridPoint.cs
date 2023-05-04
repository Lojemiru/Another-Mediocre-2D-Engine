using Newtonsoft.Json;

namespace AM2E.Levels;

public sealed class LDtkFieldInstanceGridPoint
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

    public static LDtkFieldInstanceGridPoint FromDynamic(dynamic input)
    {
        try
        {
            return new LDtkFieldInstanceGridPoint
            {
                X = input.cx,
                Y = input.cy
            };
        }
        catch
        {
            return null;
        }
    }
}