using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeArc
{
    Room m_startRoom;
    Room m_targetRoom;
    int m_weight;



    public NodeArc(Room t_start, Room t_target)
    {
        m_startRoom = t_start;
        m_targetRoom = t_target;

        m_weight = CalucalteWeight(m_startRoom.GetNodePositonOnMap(), m_targetRoom.GetNodePositonOnMap());
    }

    public Room GetStartRoom()
    {
        return m_startRoom;
    }

    public Room GetTargetRoom()
    {
        return m_targetRoom;
    }

    public int GetWeigtht()
    {
        return m_weight;
    }

    int CalucalteWeight(Pair<int,int> t_posOne, Pair<int,int>t_posTwo)
    {
        return (int)Mathf.Sqrt(Mathf.Pow(t_posOne.m_first - t_posTwo.m_first, 2) 
            + Mathf.Pow(t_posOne.m_second - t_posTwo.m_second, 2));
    }
}
