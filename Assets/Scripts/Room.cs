using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public TileGrid m_roomGrid;
    public List<TileArc> m_nodeArcs;

    int m_roomID = -1;
    public bool m_roomAdded = false;

    Pair<int, int> m_nodePosIndex;
    Pair<int, int> m_posIndex;

    List<Pair<int, int>> m_possibleExitList = new List<Pair<int, int>>();
    List<Pair<int, int>> m_exitList = new List<Pair<int, int>>();

    bool m_isVisited = false;
    public void GenerateRoom()
    {
        m_roomGrid = new TileGrid();
        m_nodeArcs = new List<TileArc>();

        m_roomGrid.m_height = GameplayManager.s_seedRandom.Next(5, 15);
        m_roomGrid.m_width = GameplayManager.s_seedRandom.Next(5, 15);

        m_roomGrid.CreateTileGrid();
        RandomFillRoom();

        m_nodePosIndex = new Pair<int, int>(m_roomGrid.m_width / 2, m_roomGrid.m_height / 2);
        m_roomGrid.SetTileType(m_nodePosIndex.m_first, m_nodePosIndex.m_second, TileType.Node);

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
                    m_roomGrid.SetTileType(x, y, TileType.Wall);
                }
                else
                {
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

    public void SetPositionIndex(Pair<int, int> t_newPosIndex)
    {
        m_posIndex = t_newPosIndex;
    }

    public Pair<int, int> GetPositionIndex()
    {
        return m_posIndex;
    }

    public Pair<int, int> GetNodePositonOnMap()
    {
        if (m_posIndex != null)
        {
            return new Pair<int, int>(m_posIndex.m_first + m_nodePosIndex.m_first,
                m_posIndex.m_second + m_nodePosIndex.m_second);
        }

        return null;
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
        m_possibleExitList.Add(new Pair<int, int>(0, m_nodePosIndex.m_second));
        m_possibleExitList.Add(new Pair<int, int>(m_nodePosIndex.m_first, 0));
        m_possibleExitList.Add(new Pair<int, int>(m_roomGrid.m_width - 1, m_nodePosIndex.m_second));
        m_possibleExitList.Add(new Pair<int, int>(m_nodePosIndex.m_first, m_roomGrid.m_height - 1));
    }

    public List<Pair<int, int>> GetPossibleExitsOnMap()
    {
        List<Pair<int, int>> possibleExitsOnMap = new List<Pair<int, int>>();

        foreach (Pair<int, int> exit in m_possibleExitList)
        {
            possibleExitsOnMap.Add(new Pair<int, int>(exit.m_first + m_posIndex.m_first, exit.m_second + m_posIndex.m_second));
        }

        return possibleExitsOnMap;
    }

    public void AddExitToRoom(Pair<int, int> t_exitOnMapIndex)
    {
        Pair<int, int> exitInRoomIndex = new Pair<int, int>(t_exitOnMapIndex.m_first - m_posIndex.m_first,
            t_exitOnMapIndex.m_second - m_posIndex.m_second);

        if (!m_exitList.Contains(exitInRoomIndex))
        {
            m_exitList.Add(exitInRoomIndex);
            m_roomGrid.SetTileType(exitInRoomIndex.m_first, exitInRoomIndex.m_second, TileType.Exit);
        }
    }

    public List<Pair<int, int>> GetExitsOnMap()
    {
        List<Pair<int, int>> exitsOnMap = new List<Pair<int, int>>();

        foreach (Pair<int, int> exit in m_exitList)
        {
            exitsOnMap.Add(new Pair<int, int>(exit.m_first + m_posIndex.m_first, exit.m_second + m_posIndex.m_second));
        }

        return exitsOnMap;
    }
}
