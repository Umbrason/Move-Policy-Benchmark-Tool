//Assumptions: not many nodes in here at a time, nodes get inserted in order of rising cost order

using System;

public class FastNodeQueue
{
    Ringbuffer lowerCostIndices;
    Ringbuffer higherCostIndices;
    public int NextCost;
    int higherCost;


    public FastNodeQueue(int capacity)
    {
        this.lowerCostIndices = new(capacity);
        this.higherCostIndices = new(capacity);
        NextCost = int.MaxValue;
        higherCost = int.MaxValue;
    }

    public int NextIndex => lowerCostIndices.NextValue;
    public int Count => lowerCostIndices.Count + higherCostIndices.Count;

    public void Dequeue(out int cost, out int index)
    {
        if (lowerCostIndices.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements");
        cost = NextCost;
        index = lowerCostIndices.Pop();
        if (lowerCostIndices.Count == 0)
        {
            if (higherCostIndices.Count > 0)
            {
                (higherCostIndices, lowerCostIndices) = (lowerCostIndices, higherCostIndices);
                NextCost = higherCost;
                higherCost = int.MaxValue;
            }
            else NextCost = int.MaxValue;
        }
    }

    public void Enqueue(int cost, int index)
    {
        if (NextCost == int.MaxValue) NextCost = cost;
        if (cost > NextCost && higherCost == int.MaxValue) higherCost = cost;

        if (cost < NextCost) throw new InvalidOperationException("Cannot add lower cost nodes!");
        if (cost > NextCost && cost > higherCost) throw new InvalidOperationException("Cannot add two tiers of higher cost nodes!");

        if (cost == NextCost) lowerCostIndices.Push(index);
        else higherCostIndices.Push(index);
    }

    public void Reset()
    {
        lowerCostIndices.Reset();
        higherCostIndices.Reset();
        NextCost = higherCost = int.MaxValue;
    }
}