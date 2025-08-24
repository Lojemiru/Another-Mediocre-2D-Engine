namespace AM2E;

public class State
{
    public Action Step { get; init; } = () => { };
    public Action Enter { get; init; } = () => { };
    public Action Leave { get; init; } = () => { };
}