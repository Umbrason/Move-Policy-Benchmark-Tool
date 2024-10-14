using System.Collections.Generic;

public class Team
{
    public List<Agent> Agents { get; }
    public string Name { get; }

    public Team(List<Agent> agents, string name)
    {
        this.Agents = agents;
        this.Name = name;
    }
}
