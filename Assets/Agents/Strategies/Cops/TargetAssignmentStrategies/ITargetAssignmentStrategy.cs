using System;
using System.Collections.Generic;

public interface ITargetAssignmentStrategy
{
    public Dictionary<Cop, Robber> AssignAll(CopsNRobberGame game);
    public virtual Dictionary<Cop, Robber>[] AssignmentOptions(CopsNRobberGame game)
    {
        var optionCount = (int)Math.Pow(game.Robbers.agents.Count, game.Cops.agents.Count);
        var options = new Dictionary<Cop, Robber>[optionCount];
        for (int a = 0; a < options.Length; a++)
        {
            options[a] = new();
            for (int c = 0; c < game.Cops.agents.Count; c++)
            {
                var r = a / (int)Math.Pow(game.Robbers.agents.Count, c) % game.Robbers.agents.Count;
                options[a][(Cop)game.Cops.agents[c]] = (Robber)game.Robbers.agents[r];
            }
        }
        return options;
    }
}