using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public struct GridIndex
{
    public int m_x;     //X component of the grid index position.
    public int m_y;     //Y component of the grid index position.

    public GridIndex(int t_xIndex, int t_yIndex)
    {
        m_x = t_xIndex;
        m_y = t_yIndex;
    }

    public GridIndex(GridIndex t_mapIndex)
    {
        m_x = t_mapIndex.m_x;
        m_y = t_mapIndex.m_y;
    }
}

public class TileGrid
{
    public int m_height;
    public int m_width;

    Tile[,] m_grid;

    public TileGrid()
    {
        m_width = -1;
        m_height = -1;
    }

    public TileGrid(int t_width, int t_height)
    {
        m_width = t_width;
        m_height = t_height;
    }

    public void CreateTileGrid()
    {
        m_grid = new Tile[m_width, m_height];

        if (m_grid.GetLength(0) == 0 || m_grid.GetLength(1) == 0)
        {
            Debug.Log("Warning One of the dimensions has a size of 0");
        }
        else
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    m_grid[x, y] = new Tile();
                }
            }
        }
    }

    public Tile GetTile(GridIndex t_gridIndex)
    {
        if(m_grid != null)
        {
            if (t_gridIndex.m_x >= 0 || t_gridIndex.m_x < m_width || t_gridIndex.m_y >= 0 || t_gridIndex.m_y < m_height)
            {
                return m_grid[t_gridIndex.m_x, t_gridIndex.m_y];
            }
        }

        return null;
    }

    public void SetTileType(GridIndex t_gridIndex, TileType t_newTileValue)
    {
        m_grid[t_gridIndex.m_x, t_gridIndex.m_y].SetTileType(t_newTileValue);
    }

    public void SetTileOwner(GridIndex t_gridIndex, int t_newOwnerID)
    {
        m_grid[t_gridIndex.m_x, t_gridIndex.m_y].SetOwnerID(t_newOwnerID);
    }
}
