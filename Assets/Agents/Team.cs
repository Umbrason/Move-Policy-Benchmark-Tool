using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public List<Agent> Agents { get; }
    public Color Color { get; }
    public string Name { get; }

    public Team(List<Agent> agents, Color color, string name)
    {
        this.Agents = agents;
        this.Color = color;
        this.Name = name;
    }
}
