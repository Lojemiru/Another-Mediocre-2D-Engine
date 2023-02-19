using System.Collections.Generic;

namespace AM2E.Actors;

public static class PlayerPool
{
    private static List<IPlayer> players = new();

    public static void Add(IPlayer player)
    {
        if (!players.Contains(player)) players.Add(player);
    }

    public static IPlayer GetFirst()
    {
        return players[0];
    }
}