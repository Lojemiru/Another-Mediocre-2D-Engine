
namespace AM2E.Collision;

public interface ICollider
{
    public Collider Collider { get; }
    
    // TODO: These should ALWAYS point at the Collider's X/Y properties. Should this be an abstract class instead?
    public int X { get; set; }
    public int Y { get; set; }
}