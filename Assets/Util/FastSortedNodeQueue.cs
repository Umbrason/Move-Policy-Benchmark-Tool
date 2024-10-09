//Assumptions: not many nodes in here at a time, nodes get inserted in order of rising cost order

public class FastNodeQueue
{
    int[] indexQueue;
    int[] costQueue;
    int head;
    int tail;
    int capacity;

    public FastNodeQueue(int capacity)
    {
        this.indexQueue = new int[capacity];
        this.costQueue = new int[capacity];
        this.capacity = capacity;
        for (int i = 0; i < capacity; i++)
        {
            indexQueue[i] = -1;
            costQueue[i] = int.MaxValue;
        }
    }

    public int NextCost => costQueue[tail];
    public int NextIndex => indexQueue[tail];

    public void Dequeue(out int cost, out int index)
    {
        cost = costQueue[tail];
        index = indexQueue[tail];
        costQueue[tail] = int.MaxValue;
        indexQueue[tail] = -1;
        tail++;
        if(tail >= capacity) tail -= capacity;        
    }

    public void Enqueue(int cost, int index)
    {
        costQueue[head] = cost;
        indexQueue[head] = index;
        head++;
        if(head >= capacity) head -= capacity;
        if (head == tail) throw new System.Exception("Ringbuffer looped!");
    }

    public void Reset()
    {
        for (int i = 0; i < capacity; i++)
        {
            indexQueue[i] = -1;
            costQueue[i] = int.MaxValue;
        }
        head = 0;
        tail = 0;
    }
}