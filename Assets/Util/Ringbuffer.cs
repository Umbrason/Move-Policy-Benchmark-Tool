using System;

public class Ringbuffer
{
    int[] data;

    public int Count;    
    public int NextValue => data[tail];

    int head;
    int tail;
    int capacity;

    public Ringbuffer(int capacity)
    {
        this.data = new int[capacity];
        this.capacity = capacity;
        this.head = 0;
        this.tail = 0;
        for (int i = 0; i < capacity; i++)
            data[i] = -1;
    }


    public int Pop()
    {
        if (Count == 0) throw new InvalidOperationException("Ringbuffer contains no elements!");
        var index = data[tail];
        tail++;
        if (tail >= capacity) tail -= capacity;
        Count--;
        return index;
    }

    public void Push(int value)
    {
        if (Count >= capacity) throw new Exception("Ringbuffer full!");
        data[head] = value;
        head++;
        if (head >= capacity) head -= capacity;
        Count++;
    }

    public void Reset()
    {
        for (int i = 0; i < capacity; i++) data[i] = -1;
        head = 0;
        tail = 0;
        Count = 0;
    }
}