using System;
using System.Collections.Generic;
using System.Linq;

public interface ITargetAssignmentStrategy
{
    public Dictionary<Cop, Robber> AssignAll(CopsNRobberGame game);
    public virtual Dictionary<Cop, Robber>[] AssignmentOptions(CopsNRobberGame game)
    {
        var uncaughtRobbers = game.Robbers.Agents.Where(robber => !(robber as Robber).Caught).ToList();
        var optionCount = (int)Math.Pow(uncaughtRobbers.Count, game.Cops.Agents.Count);
        var options = new Dictionary<Cop, Robber>[optionCount];
        for (int a = 0; a < options.Length; a++)
        {
            options[a] = new();
            for (int c = 0; c < game.Cops.Agents.Count; c++)
            {
                var r = a / (int)Math.Pow(uncaughtRobbers.Count, c) % uncaughtRobbers.Count;
                options[a][(Cop)game.Cops.Agents[c]] = (Robber)uncaughtRobbers[r];
            }
        }
        return options;
    }
}