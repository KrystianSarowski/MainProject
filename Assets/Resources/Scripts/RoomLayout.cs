using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski
public class RoomLayout
{
    public TileGrid m_grid;
    public List<TileArc> m_nodeArcs;

    int m_id = -1;

    public GridIndex m_nodePosIndex;
    GridIndex m_posIndex;

    List<GridIndex> m_possibleExitList = new List<GridIndex>();
    List<GridIndex> m_exitList = new List<GridIndex>();

    public bool m_roomAdded = false;
    bool m_isVisited = false;

    const int m_MIN_SIZE = 7;
    const int m_MAX_SIZE = 15;

    public void GenerateLayout()
    {
        m_grid = new TileGrid();
        m_nodeArcs = new List<TileArc>();

        m_grid.m_height = GameplayManager.s_seedRandom.Next(m_MIN_SIZE, m_MAX_SIZE);
        m_grid.m_width = GameplayManager.s_seedRandom.Next(m_MIN_SIZE, m_MAX_SIZE);

        m_grid.CreateTileGrid();
        FillLayout();

        m_nodePosIndex = new GridIndex(m_grid.m_width / 2, m_grid.m_height / 2);
        m_grid.SetTileType(m_nodePosIndex, TileType.Node);

        CreatePossibleExits();
        CreateInnerWalls();
    }

    void FillLayout()
    {
        for (int x = 0; x < m_grid.m_width; x++)
        {
            for (int y = 0; y < m_grid.m_height; y++)
            {
                if (x == 0 || x == m_grid.m_width - 1 || y == 0 || y == m_grid.m_height - 1)
                {
                    m_grid.SetTileType(new GridIndex(x, y), TileType.Wall);
                }
                else
                {
                    m_grid.SetTileType(new GridIndex(x, y), TileType.Empty);
                }
            }
        }
    }

    public void SetID(int t_newRoomID)
    {
        m_id = t_newRoomID;
    }

    public int GetID()
    {
        return m_id;
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

    public void AddArc(RoomLayout t_newRoom)
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
        m_possibleExitList.Add(new GridIndex(m_grid.m_width - 1, m_nodePosIndex.m_y));
        m_possibleExitList.Add(new GridIndex(m_nodePosIndex.m_x, m_grid.m_height - 1));
    }

    void CreateInnerWalls()
    {
        if(m_grid.m_height != m_MIN_SIZE && m_grid.m_width != m_MIN_SIZE)
        {
            int roomDesignIndex = GameplayManager.s_seedRandom.Next(0, 4);

            switch (roomDesignIndex)
            {
                case 0:
                    {
                        int yOffset = Mathf.CeilToInt((m_grid.m_height / 2) / 2.0f);
                        for (int x = 2; x < m_grid.m_width - 2; x++)
                        {
                            m_grid.SetTileType(new GridIndex(x, yOffset), TileType.InnerWall);
                            m_grid.SetTileType(new GridIndex(x, m_grid.m_height - 1 - yOffset), TileType.InnerWall);
                        }
                    }
                    break;
                case 1:
                    {
                        int xOffset = Mathf.CeilToInt((m_grid.m_width / 2) / 2.0f);
                        for (int y = 2; y < m_grid.m_height - 2; y++)
                        {
                            m_grid.SetTileType(new GridIndex(xOffset, y), TileType.InnerWall);
                            m_grid.SetTileType(new GridIndex(m_grid.m_width - 1 - xOffset, y), TileType.InnerWall);
                        }
                    }
                    break;
                case 2:
                    {
                        int xOffset = Mathf.CeilToInt((m_grid.m_width / 2) / 2.0f);
                        for (int y = 2; y < m_grid.m_height - 2; y++)
                        {
                            if (y != m_nodePosIndex.m_y)
                            {
                                m_grid.SetTileType(new GridIndex(xOffset, y), TileType.InnerWall);
                                m_grid.SetTileType(new GridIndex(m_grid.m_width - 1 - xOffset, y), TileType.InnerWall);
                            }
                        }

                        int yOffset = Mathf.CeilToInt((m_grid.m_height / 2) / 2.0f);
                        for (int x = 2; x < m_grid.m_width - 2; x++)
                        {
                            if(x != m_nodePosIndex.m_x)
                            {
                                m_grid.SetTileType(new GridIndex(x, yOffset), TileType.InnerWall);
                                m_grid.SetTileType(new GridIndex(x, m_grid.m_height - 1 - yOffset), TileType.InnerWall);
                            }
                        }
                    }
                    break;
                case 3:
                    {

                        int xOffset = GameplayManager.s_seedRandom.Next(1, (m_grid.m_width - 4) / 2);
                        int yOffset = GameplayManager.s_seedRandom.Next(1, (m_grid.m_height - 4) / 2);

                        m_grid.SetTileType(new GridIndex(m_nodePosIndex.m_x - xOffset, m_nodePosIndex.m_y - yOffset), TileType.InnerWall);
                        m_grid.SetTileType(new GridIndex(m_nodePosIndex.m_x + xOffset, m_nodePosIndex.m_y - yOffset), TileType.InnerWall);
                        m_grid.SetTileType(new GridIndex(m_nodePosIndex.m_x + xOffset, m_nodePosIndex.m_y + yOffset), TileType.InnerWall);
                        m_grid.SetTileType(new GridIndex(m_nodePosIndex.m_x - xOffset, m_nodePosIndex.m_y + yOffset), TileType.InnerWall);
                    }
                    break;
                default:
                    break;
            }
        }
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

    public void AddExitToLayout(GridIndex t_exitOnMapIndex)
    {
        GridIndex roomExitIndex = new GridIndex(t_exitOnMapIndex.m_x - m_posIndex.m_x,
            t_exitOnMapIndex.m_y - m_posIndex.m_y);

        if (!m_exitList.Contains(roomExitIndex))
        {
            m_exitList.Add(roomExitIndex);
            m_grid.SetTileType(roomExitIndex, TileType.Exit);
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

    public void RemoveInnerWalls()
    {
        for (int x = 0; x < m_grid.m_width; x++)
        {
            for (int y = 0; y < m_grid.m_height; y++)
            {
                if(m_grid.GetTile(new GridIndex(x, y)).GetTileType() == TileType.InnerWall)
                {
                    m_grid.SetTileType(new GridIndex(x, y), TileType.Empty);
                }
            }
        }
    }

    public void RemoveNodeArcs()
    {
        m_nodeArcs.Clear();
    }
}