using System;
using Newtonsoft.Json;

namespace AM2E.Levels;

public struct LDtkReferenceToAnEntityInstance
{
    /// <summary>
    /// IID of the referred EntityInstance
    /// </summary>
    public string EntityIid => Iids.EntityIid;

    /// <summary>
    /// IID of the LayerInstance containing the referred EntityInstance
    /// </summary>
    public string LayerIid => Iids.LayerIid;

    /// <summary>
    /// IID of the Level containing the referred EntityInstance
    /// </summary>
    public string LevelIid => Iids.LevelIid;

    /// <summary>
    /// IID of the World containing the referred EntityInstance
    /// </summary>
    public string WorldIid => Iids.WorldIid;
    
    /// <summary>
    /// World X coordinate in pixels.
    /// </summary>
    [JsonProperty("worldX")]
    public int WorldX { get; set; }

    /// <summary>
    /// World Y coordinate in pixels.
    /// </summary>
    [JsonProperty("worldY")]
    public int WorldY { get; set; }
    
    /// <summary>
    /// Height of the referred entity in pixels
    /// </summary>
    [JsonProperty("pxHei")]
    public int PxHei { get; set; }

    /// <summary>
    /// Width of the referred entity in pixels
    /// </summary>
    [JsonProperty("pxWid")]
    public int PxWid { get; set; }
    
    [JsonProperty("iids")]
    public LDtkReferenceToAnEntityInstanceIids Iids { get; set; }

    /// <summary>
    /// List of FieldInstances for this EntityInstance that are marked to be copied to the ToC.
    /// </summary>
    [JsonProperty("fields")]
    public LDtkFieldInstance[] FieldInstances;
    
    public dynamic GetFieldInstance(string identifier)
        => FieldInstances.GetFieldInstance(identifier);
    
    public T GetFieldInstance<T>(string identifier) where T : struct, Enum
        => FieldInstances.GetFieldInstance<T>(identifier);

    public T[] GetFieldInstanceArray<T>(string identifier)
        => FieldInstances.GetFieldInstanceArray<T>(identifier);
}