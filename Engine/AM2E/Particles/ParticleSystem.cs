using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Particles;

public class ParticleSystem
{
    public readonly ParticleDefinition Definition;
    
    public ParticleSystem(string definition)
    {
        Definition = ParticleDefinitions.Definitions[definition];
    }
    
    public ParticleSystem(ParticleDefinition definition)
    {
        Definition = definition;
    }

    public void Update()
    {
        
    }

    public void Draw(SpriteBatch spriteBatch, int x, int y)
    {
        
    }
}