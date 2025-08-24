using System.Drawing;
using Newtonsoft.Json.Linq;

namespace AM2E.Levels;

public static class LDtkTypeExtensions
{
    public static Point[] ToPoint(this LDtkGridPoint[] points, Level level)
    {
        var output = new Point[points.Length];

        for (var i = 0; i < points.Length; i++)
        {
            output[i] = points[i].ToPoint(level);
        }

        return output;
    }
    
    public static dynamic? GetFieldInstance(this IEnumerable<LDtkFieldInstance> fieldInstances, string identifier)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var field in fieldInstances)
        {
            if (field.Identifier != identifier)
                continue;

            if (field.Value is null)
                return null;

            switch (field.Type)
            {
                // LDtk will pass us floats as doubles... even though they're called floats in the editor.
                // This could also just be a JSON issue. Either way, I'd rather catch it here than on the other end every time...
                case "Float":
                    return (float)field.Value;
                // Ints report back as doubles, I believe. This forces them to be typed appropriately.
                case "Int":
                    return (int)field.Value;
                // Colors are passed as a string in the format #RRGGBB.
                // This is very annoying to parse, but I've done it nonetheless.
                case "Color":
                {
                    var r = Convert.ToInt32(field.Value.Substring(1, 2), 16);
                    var g = Convert.ToInt32(field.Value.Substring(3, 2), 16);
                    var b = Convert.ToInt32(field.Value.Substring(5, 2), 16);
                    return new Microsoft.Xna.Framework.Color(r, g, b);
                }
                case "Point":
                    return LDtkGridPoint.FromDynamic(field.Value);
                case "EntityRef":
                    return LDtkFieldInstanceEntityReference.FromDynamic(field.Value);
                default:
                    return field.Value;
            }
        }

        return null;
    }

    public static T? GetFieldInstance<T>(this IEnumerable<LDtkFieldInstance> fieldInstances, string identifier) where T : struct, Enum
    {
        foreach (var field in fieldInstances)
        {
            if (field.Identifier != identifier)
                continue;

            if (field.Value is null)
                return null;

            if (Enum.TryParse((string)field.Value, out T output))
                return output;
        }
        
        return null;
    }

    public static T[]? GetFieldInstanceArray<T>(this IEnumerable<LDtkFieldInstance> fieldInstances, string identifier)
    {
        // LDtk hands us a JArray instead of an actual array. This should resolve that before any other code sees that.
        
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var field in fieldInstances)
        {
            if (field.Value is not JArray array || field.Identifier != identifier)
                continue;

            return array.ToObject<T[]>();
        }

        return null;
    }
}