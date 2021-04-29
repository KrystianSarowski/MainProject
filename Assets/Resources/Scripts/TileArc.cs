using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public class TileArc
{
    RoomLayout m_startRoom;
    RoomLayout m_targetRoom;

    GridIndex m_startPos;
    GridIndex m_targetPos;

    int m_weight;

    public TileArc()
    {
        m_startRoom = null;
        m_targetRoom = null;

        m_weight = 999999;
    }

    public TileArc(RoomLayout t_start, RoomLayout t_target)
    {
        m_startRoom = t_start;
        m_targetRoom = t_target;

        m_startPos = m_startRoom.GetNodePositonOnMap();
        m_targetPos = m_targetRoom.GetNodePositonOnMap();

        m_weight = CalculateWeight(m_startPos, m_targetPos);
    }

    public void SetTargetPos(GridIndex t_targetPos)
    {
        m_targetPos = t_targetPos;

        m_weight = CalculateWeight(m_startPos, m_targetPos);
    }

    public void SetStartPos(GridIndex t_startPos)
    {
        m_startPos = t_startPos;
        m_weight = CalculateWeight(m_startPos, m_targetPos);
    }

    public GridIndex GetStartPos()
    {
        return m_startPos;
    }

    public GridIndex GetTargetPos()
    {
        return m_targetPos;
    }

    public RoomLayout GetStartRoom()
    {
        return m_startRoom;
    }

    public RoomLayout GetTargetRoom()
    {
        return m_targetRoom;
    }

    public int GetWeigtht()
    {
        return m_weight;
    }

    public static int CalculateWeight(GridIndex t_posOne, GridIndex t_posTwo)
    {
        return (int)Mathf.Sqrt(Mathf.Pow(t_posOne.m_x - t_posTwo.m_x, 2) 
            + Mathf.Pow(t_posOne.m_y - t_posTwo.m_y, 2));
    }
}