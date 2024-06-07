namespace AM2E.Levels;

public sealed class CompositeBackground
{
    public CompositeBackground(int uid)
    {
        var def = World.GetCompositeBackground(uid);
    }
}