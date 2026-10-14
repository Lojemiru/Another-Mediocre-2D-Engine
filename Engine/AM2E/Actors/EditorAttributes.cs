using Microsoft.Xna.Framework;

namespace AM2E;

// TODO: editor values; size/origin, sprite reference, flippable allowances, resizing details, allow out of bounds,
// color and other display attributes, count restrictions, comments

[AttributeUsage(AttributeTargets.Class)]
public sealed class AM2EPlaceableAttribute(params string[]? tags) 
    : Attribute
{
    public string[]? Tags { get; } = tags;
}

public abstract class AM2EAttribute(string name) 
    : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EEnumAttribute : AM2EAttribute
{
    public Type EnumType { get; }
    public object? DefaultValue { get; }

    public AM2EEnumAttribute(string name, Type enumType, object? defaultValue)
        : base(name)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"{nameof(enumType)} must be an enum type");

        EnumType = enumType;

        if (defaultValue is null)
            return;

        if (defaultValue.GetType() != enumType)
            throw new ArgumentException($"{nameof(defaultValue)} must be the same type as {nameof(enumType)}");

        DefaultValue = defaultValue;
    }
}

public sealed class AM2EEnumArrayAttribute(
    string name,
    Type enumType,
    object? defaultValue,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EEnumAttribute(name, enumType, defaultValue)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EColorAttribute(
    string name,
    Color defaultValue,
    bool overrideEntityColor)
    : AM2EAttribute(name)
{
    public Color DefaultValue { get; } = defaultValue;
    public bool OverrideEntityColor { get; } = overrideEntityColor;
}

public sealed class AM2EColorArrayAttribute(
    string name,
    Color defaultValue,
    bool overrideEntityColor,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EColorAttribute(name, defaultValue, overrideEntityColor)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EStringAttribute(
    string name,
    string? defaultValue = null,
    bool allowNull = false)
    : AM2EAttribute(name)
{
    public string? DefaultValue { get; } = defaultValue;
    public bool AllowNull { get; } = allowNull;
}

public sealed class AM2EStringArrayAttribute(
    string name,
    string? defaultValue = null,
    bool allowNull = false,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EStringAttribute(name, defaultValue, allowNull)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EPointAttribute(
    string name)
    : AM2EAttribute(name)
{
    
}

public sealed class AM2EPointArrayAttribute(
    string name,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EPointAttribute(name)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EEntityRefAttribute(
    string name,
    bool sameLevel = true,
    string[]? permittedTags = null)
    : AM2EAttribute(name)
{
    public bool SameLevel { get; } = sameLevel;
    public string[]? PermittedTags { get; } = permittedTags;
}

public sealed class AM2EEntityRefArrayAttribute(
    string name,
    bool sameLevel = true,
    string[]? permittedTags = null,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EEntityRefAttribute(name, sameLevel, permittedTags)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EBoolAttribute(
    string name,
    bool defaultValue = false,
    bool random = false)
    : AM2EAttribute(name)
{
    public bool Default { get; } = defaultValue;
    public bool Random { get; } = random;
}

public sealed class AM2EBoolArrayAttribute(
    string name,
    bool defaultvalue = false,
    bool random = false,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EBoolAttribute(name, defaultvalue, random)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EIntAttribute(
    string name,
    int defaultValue = 0,
    int min = int.MinValue,
    int max = int.MaxValue,
    bool random = false)
    : AM2EAttribute(name)
{
    public int Default { get; } = defaultValue;
    public int Min { get; } = min;
    public int Max { get; } = max;
    public bool Random { get; } = random;
}

public sealed class AM2EIntArrayAttribute(
    string name,
    int defaultValue = 0,
    int min = int.MinValue,
    int max = int.MaxValue,
    bool random = false,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EIntAttribute(name, defaultValue, min, max, random)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EFloatAttribute(
    string name,
    float defaultValue = 0,
    float min = float.MinValue,
    float max = float.MaxValue,
    bool random = false)
    : AM2EAttribute(name)
{
    public float Default { get; } = defaultValue;
    public float Min { get; } = min;
    public float Max { get; } = max;
    public bool Random { get; } = random;
}

[AttributeUsage(AttributeTargets.Class)]
public class AM2EFloatArrayAttribute(
    string name,
    float defaultValue = 0,
    float min = float.MinValue,
    float max = float.MaxValue,
    bool random = false,
    int arrayMin = 0,
    int arrayMax = int.MaxValue)
    : AM2EFloatAttribute(name, defaultValue, min, max, random)
{
    public int ArrayMin { get; } = arrayMin;
    public int ArrayMax { get; } = arrayMax;
}