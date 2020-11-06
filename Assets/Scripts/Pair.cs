using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair<T, U>
{
    public T m_first { get; set; }
    public U m_second { get; set; }

    public Pair()
    {

    }

    public Pair(T t_first, U t_second)
    {
        this.m_first = t_first;
        this.m_second = t_second;
    }
}
