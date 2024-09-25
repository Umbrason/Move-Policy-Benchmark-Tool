public class Agent
{
    public Node OccupiedNode { get; protected set; }

    public void SetPosition(Node node)
    {
        if (OccupiedNode == node) return;
        OccupiedNode?.Occupants?.Remove(this);
        node?.Occupants?.Add(this);
        this.OccupiedNode = node;
        if (node != null) OnEnterNode(node);
    }

    public void Move(Node node)
    {
        if (node == OccupiedNode) return;
        if (!OccupiedNode.Neighbours.Contains(node)) return;
        SetPosition(node);
    }

    protected virtual void OnEnterNode(Node node) { }

    public virtual void Reset() { SetPosition(null); }
}
