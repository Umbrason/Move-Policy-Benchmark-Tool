using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public List<Agent> Agents { get; }
    public string Name { get; }
    public Color Color { get; }

    public Team(List<Agent> agents, Color color, string name)
    {
        this.Agents = agents;
        this.Name = name;
        this.Color = color;
    }
}
