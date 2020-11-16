using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public TileGrid m_roomGrid;
    public List<NodeArc> m_nodeArcs;

    int m_roomID = -1;
    public bool m_roomAdded = false;

    Pair<int, int> m_nodePosIndex;
    Pair<int, int> m_posIndex;

    bool m_isVisited = false;
    public void GenerateRoom()
    {
        m_roomGrid = new TileGrid();
        m_nodeArcs = new List<NodeArc>();

        m_roomGrid.m_height = GameplayMananger.s_seedRandom.Next(5, 15);
        m_roomGrid.m_width = GameplayMananger.s_seedRandom.Next(5, 15);

        m_roomGrid.CreateTileGrid();
        RandomFillRoom();

        m_nodePosIndex = new Pair<int, int>(m_roomGrid.m_width / 2, m_roomGrid.m_height / 2);
        m_roomGrid.SetTileType(m_nodePosIndex.m_first, m_nodePosIndex.m_second, TileType.Node);
    }

    void RandomFillRoom()
    {
        for (int x = 0; x < m_roomGrid.m_width; x++)
        {
            for (int y = 0; y < m_roomGrid.m_height; y++)
            {
                if (x == 0 || x == m_roomGrid.m_width - 1 || y == 0 || y == m_roomGrid.m_height - 1)
                {
                    m_roomGrid.SetTileType(x, y, TileType.Wall);
                }
                else
                {
                    //m_roomGrid.SetTileType(x, y, (GameplayMananger.s_seedRandom.Next(0, 100) < 45) ? TileType.Wall : TileType.Empty);
                    m_roomGrid.SetTileType(x, y, TileType.Empty);
                }
            }
        }
    }

    public void SetRoomID(int t_newRoomID)
    {
        m_roomID = t_newRoomID;
    }

    public int GetRoomID()
    {
        return m_roomID;
    }

    public void SetPositionIndex(Pair<int,int> t_newPosIndex)
    {
        m_posIndex = t_newPosIndex;
    }

    public Pair<int,int> GetPositionIndex()
    {
        return m_posIndex;
    }

    public Pair<int, int> GetNodePositonOnMap()
    {
        if(m_posIndex != null)
        {
            return new Pair<int, int>(m_posIndex.m_first + m_nodePosIndex.m_first, 
                m_posIndex.m_second + m_nodePosIndex.m_second);
        }

        return null;
    }

    public void AddArc(Room t_newRoom)
    {
        m_nodeArcs.Add(new NodeArc(this, t_newRoom));
    }

    public void SetIsVisited(bool t_isVisited)
    {
        m_isVisited = t_isVisited;
    }

    public bool GetIsVisited()
    {
        return m_isVisited;
    }
}
