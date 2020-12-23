using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public TileGrid m_roomGrid;
    public List<TileArc> m_nodeArcs;

    int m_roomID = -1;
    public bool m_roomAdded = false;

    GridIndex m_nodePosIndex;
    GridIndex m_posIndex;

    List<GridIndex> m_possibleExitList = new List<GridIndex>();
    List<GridIndex> m_exitList = new List<GridIndex>();

    bool m_isVisited = false;

    public void GenerateRoom()
    {
        m_roomGrid = new TileGrid();
        m_nodeArcs = new List<TileArc>();

        m_roomGrid.m_height = GameplayManager.s_seedRandom.Next(5, 15);
        m_roomGrid.m_width = GameplayManager.s_seedRandom.Next(5, 15);

        m_roomGrid.CreateTileGrid();
        RandomFillRoom();

        m_nodePosIndex = new GridIndex(m_roomGrid.m_width / 2, m_roomGrid.m_height / 2);
        m_roomGrid.SetTileType(m_nodePosIndex, TileType.Node);

        CreatePossibleExits();
    }

    void RandomFillRoom()
    {
        for (int x = 0; x < m_roomGrid.m_width; x++)
        {
            for (int y = 0; y < m_roomGrid.m_height; y++)
            {
                if (x == 0 || x == m_roomGrid.m_width - 1 || y == 0 || y == m_roomGrid.m_height - 1)
                {
                    m_roomGrid.SetTileType(new GridIndex(x, y), TileType.Wall);
                }
                else
                {
                    m_roomGrid.SetTileType(new GridIndex(x, y), TileType.Empty);
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

    public void SetPositionIndex(GridIndex t_newPosIndex)
    {
        m_posIndex = t_newPosIndex;
    }

    public GridIndex GetPositionIndex()
    {
        return m_posIndex;
    }

    public GridIndex GetNodePositonOnMap()
    {
        return new GridIndex(m_posIndex.m_x + m_nodePosIndex.m_x, m_posIndex.m_y + m_nodePosIndex.m_y); 
    }

    public void AddArc(Room t_newRoom)
    {
        m_nodeArcs.Add(new TileArc(this, t_newRoom));
    }

    public void SetIsVisited(bool t_isVisited)
    {
        m_isVisited = t_isVisited;
    }

    public bool GetIsVisited()
    {
        return m_isVisited;
    }

    void CreatePossibleExits()
    {
        m_possibleExitList.Add(new GridIndex(0, m_nodePosIndex.m_y));
        m_possibleExitList.Add(new GridIndex(m_nodePosIndex.m_x, 0));
        m_possibleExitList.Add(new GridIndex(m_roomGrid.m_width - 1, m_nodePosIndex.m_y));
        m_possibleExitList.Add(new GridIndex(m_nodePosIndex.m_x, m_roomGrid.m_height - 1));
    }

    public List<GridIndex> GetPossibleExitsOnMap()
    {
        List<GridIndex> possibleExitsOnMap = new List<GridIndex>();

        foreach (GridIndex exit in m_possibleExitList)
        {
            possibleExitsOnMap.Add(new GridIndex(exit.m_x + m_posIndex.m_x, exit.m_y + m_posIndex.m_y));
        }

        return possibleExitsOnMap;
    }

    public void AddExitToRoom(GridIndex t_exitOnMapIndex)
    {
        GridIndex roomExitIndex = new GridIndex(t_exitOnMapIndex.m_x - m_posIndex.m_x,
            t_exitOnMapIndex.m_y - m_posIndex.m_y);

        if (!m_exitList.Contains(roomExitIndex))
        {
            m_exitList.Add(roomExitIndex);
            m_roomGrid.SetTileType(roomExitIndex, TileType.Exit);
        }
    }

    public List<GridIndex> GetExitsOnMap()
    {
        List<GridIndex> exitsOnMap = new List<GridIndex>();

        foreach (GridIndex exit in m_exitList)
        {
            exitsOnMap.Add(new GridIndex(exit.m_x + m_posIndex.m_x, exit.m_y + m_posIndex.m_y));
        }

        return exitsOnMap;
    }
}
