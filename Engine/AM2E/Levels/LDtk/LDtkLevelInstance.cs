using System;
using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkLevelInstance
{
    /// <summary>
    /// Background color of the level (same as `bgColor`, except the default value is
    /// automatically used here if its value is `null`)
    /// </summary>
    [JsonProperty("__bgColor")]
    public string BgColor { get; set; }

    /// <summary>
    /// An array listing all other levels touching this one on the world map.<br/>  Only relevant
    /// for world layouts where level spatial positioning is manual (ie. GridVania, Free). For
    /// Horizontal and Vertical layouts, this array is always empty.
    /// </summary>
    [JsonProperty("__neighbours")]
    public LDtkNeighbourLevel[] Neighbours { get; set; }

    /// <summary>
    /// UID for this Level's CompositeBackgroundDefinition.
    /// </summary>
    [JsonProperty("backgroundUid")]
    public int? BackgroundUid { get; set; }

    /// <summary>
    /// An array containing this level custom field values.
    /// </summary>
    [JsonProperty("fieldInstances")]
    public LDtkFieldInstance[] FieldInstances { get; set; }

    /// <summary>
    /// User defined unique identifier
    /// </summary>
    [JsonProperty("identifier")]
    public string Identifier { get; set; }
    
    /// <summary>
    /// Unique instance identifier
    /// </summary>
    [JsonProperty("iid")]
    public string Iid { get; set; }

    /// <summary>
    /// An array containing all Layer instances. **IMPORTANT**: if the project option "*Save
    /// levels separately*" is enabled, this field will be `null`.<br/>  This array is **sorted
    /// in display order**: the 1st layer is the top-most and the last is behind.
    /// </summary>
    [JsonProperty("layerInstances")]
    public LDtkLayerInstance[] LayerInstances { get; set; }

    /// <summary>
    /// Height of the level in pixels
    /// </summary>
    [JsonProperty("pxHei")]
    public int PxHei { get; set; }

    /// <summary>
    /// Width of the level in pixels
    /// </summary>
    [JsonProperty("pxWid")]
    public int PxWid { get; set; }
    
    /// <summary>
    /// Unique Int identifier
    /// </summary>
    [JsonProperty("uid")]
    public long Uid { get; set; }

    /// <summary>
    /// Index that represents the "depth" of the level in the world. Default is 0, greater means
    /// "above", lower means "below".<br/>  This value is mostly used for display only and is
    /// intended to make stacking of levels easier to manage.
    /// </summary>
    [JsonProperty("worldDepth")]
    public long WorldDepth { get; set; }

    /// <summary>
    /// World X coordinate in pixels.<br/>  Only relevant for world layouts where level spatial
    /// positioning is manual (ie. GridVania, Free). For Horizontal and Vertical layouts, the
    /// value is always -1 here.
    /// </summary>
    [JsonProperty("worldX")]
    public int WorldX { get; set; }

    /// <summary>
    /// World Y coordinate in pixels.<br/>  Only relevant for world layouts where level spatial
    /// positioning is manual (ie. GridVania, Free). For Horizontal and Vertical layouts, the
    /// value is always -1 here.
    /// </summary>
    [JsonProperty("worldY")]
    public int WorldY { get; set; }
    
    public dynamic GetFieldInstance(string identifier)
        => FieldInstances.GetFieldInstance(identifier);

    public T GetFieldInstance<T>(string identifier) where T : struct, Enum
        => FieldInstances.GetFieldInstance<T>(identifier);

    public T[] GetFieldInstanceArray<T>(string identifier)
        => FieldInstances.GetFieldInstanceArray<T>(identifier);
}