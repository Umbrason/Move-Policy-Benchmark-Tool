public class Cop : Agent
{
    protected override void OnEnterNode(Node node)
    {
        base.OnEnterNode(node);
        foreach (var occupant in node.Occupants)
            if (occupant is Robber robber) robber.Catch(this);
    }
}