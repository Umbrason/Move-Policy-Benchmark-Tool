using System;
using System.Collections.Generic;

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare(TKey a, TKey b)
    {
        int result = a.CompareTo(b);
        if (result == 0) return 1;
        else return result;
    }
}


public class DuplicateIntKeyComparer : IComparer<int>
{
    public int Compare(int x, int y) => x >= y ? 1 : -1;
}