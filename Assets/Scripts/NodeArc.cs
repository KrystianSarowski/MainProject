using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeArc
{
    Room m_startRoom;
    Room m_targetRoom;

    Pair<int, int> m_startPos;
    Pair<int, int> m_targetPos;

    int m_weight;

    public NodeArc()
    {
        m_startRoom = null;
        m_targetRoom = null;

        m_weight = 999999;
    }

    public NodeArc(Room t_start, Room t_target)
    {
        m_startRoom = t_start;
        m_targetRoom = t_target;

        m_startPos = m_startRoom.GetNodePositonOnMap();
        m_targetPos = m_targetRoom.GetNodePositonOnMap();

        m_weight = CalculateWeight(m_startPos, m_targetPos);
    }

    public void SetTargetPos(Pair<int,int> t_targetPos)
    {
        m_targetPos = t_targetPos;

        if(m_startPos != null)
        {
            m_weight = CalculateWeight(m_startPos, m_targetPos);
        }
    }

    public void SetStartPos(Pair<int, int> t_startPos)
    {
        m_startPos = t_startPos;

        if (m_targetPos != null)
        {
            m_weight = CalculateWeight(m_startPos, m_targetPos);
        }
    }

    public Pair<int, int> GetStartPos()
    {
        return m_startPos;
    }

    public Pair<int, int> GetTargetPos()
    {
        return m_targetPos;
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

    public static int CalculateWeight(Pair<int,int> t_posOne, Pair<int,int>t_posTwo)
    {
        return (int)Mathf.Sqrt(Mathf.Pow(t_posOne.m_first - t_posTwo.m_first, 2) 
            + Mathf.Pow(t_posOne.m_second - t_posTwo.m_second, 2));
    }
}
