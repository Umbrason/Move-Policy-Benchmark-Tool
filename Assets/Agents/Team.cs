using System.Collections.Generic;

public class Team
{
    public readonly List<Agent> agents = new();

    public Team(List<Agent> agents)
    {
        this.agents = agents;
    }
}
