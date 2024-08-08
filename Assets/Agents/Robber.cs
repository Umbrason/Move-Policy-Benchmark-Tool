public class Robber : Agent
{
    public bool Caught { get; private set; }
    public void Catch(Cop cop)
    {
        if (cop.OccupiedNode == this.OccupiedNode)
        {
            this.Caught = true;
        }
    }
    public override void Reset()
    {
        base.Reset();
        Caught = false;
    }
}