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

    public override bool Equals(object t_otherObj)
    {
        if (t_otherObj == null)
        {
            return false;
        }

        if (t_otherObj == this)
        {
            return true;
        }

        Pair<T, U> otherPair = t_otherObj as Pair<T, U>;
        
        if (otherPair == null)
        {
            return false;
        }


        return 
            (((m_first == null) && (otherPair.m_first == null))
            || ((m_first != null) && m_first.Equals(otherPair.m_first)))
            &&
            (((m_second == null) && (otherPair.m_second == null))
            || ((m_second != null) && m_second.Equals(otherPair.m_second)));
    }

    public override int GetHashCode()
    {
        int hashcode = 0;
        if (m_first != null)
        {
            hashcode += m_first.GetHashCode();
        }

        if (m_second != null)
        {
            hashcode += m_second.GetHashCode();
        }

        return hashcode;
    }
}


