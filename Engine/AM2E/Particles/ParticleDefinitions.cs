namespace AM2E.Particles;

/// <summary>
/// Static container for <see cref="ParticleDefinition"/>s.
/// </summary>
/// <remarks>
/// This class is just a <see cref="Dictionary{TKey,TValue}"/> wrapper to make defining and reusing
/// <see cref="ParticleDefinition"/>s simpler.
/// </remarks>
public static class ParticleDefinitions
{
    internal static readonly Dictionary<string, ParticleDefinition> Definitions = new();

    /// <summary>
    /// Registers a new <see cref="ParticleDefinition"/>.
    /// </summary>
    /// <param name="name">The name of the definition.</param>
    /// <param name="definition">The definition to be registered.</param>
    public static void Add(string name, ParticleDefinition definition)
    {
        Definitions.Add(name, definition);
    }

    /// <summary>
    /// Retrieves a <see cref="ParticleDefinition"/> with the given name.
    /// </summary>
    /// <param name="name">The name of the definition.</param>
    public static ParticleDefinition Get(string name)
    {
        return Definitions[name];
    }

    public static void UnloadAll()
    {
        Definitions.Clear();
    }
}