using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkDefinition
{
    /// <summary>
    /// All entities definitions, including their custom fields
    /// </summary>
    //[JsonProperty("entities")]
    //public EntityDefinition[] Entities { get; set; }

    /// <summary>
    /// All internal enums
    /// </summary>
    //[JsonProperty("enums")]
    //public EnumDefinition[] Enums { get; set; }

    /// <summary>
    /// Note: external enums are exactly the same as `enums`, except they have a `relPath` to
    /// point to an external source file.
    /// </summary>
    //[JsonProperty("externalEnums")]
    //public EnumDefinition[] ExternalEnums { get; set; }

    /// <summary>
    /// All layer definitions
    /// </summary>
    //[JsonProperty("layers")]
    //public LayerDefinition[] Layers { get; set; }

    /// <summary>
    /// All custom fields available to all levels.
    /// </summary>
    //[JsonProperty("levelFields")]
    //public FieldDefinition[] LevelFields { get; set; }

    /// <summary>
    /// All tilesets
    /// </summary>
    [JsonProperty("tilesets")]
    public LDtkTilesetDefinition[] Tilesets { get; set; }
}