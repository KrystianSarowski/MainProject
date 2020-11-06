using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public int m_height;
    public int m_width;

    int[,] m_map;

    public void CreateMap()
    {
        m_map = new int[m_width, m_height];

        if (m_map.GetLength(0) == 0 || m_map.GetLength(1) == 0)
        {
            Debug.Log("Warning One of the dimensions has a size of 0");
        }
        else
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    m_map[x, y] = 1;
                }
            }
        }
    }

    public int GetTile(int t_xIndex, int t_yIndex)
    {
        if(m_map != null)
        {
            if (t_xIndex >= 0 || t_xIndex < m_width || t_yIndex >= 0 || t_yIndex < m_height)
            {
                return m_map[t_xIndex, t_yIndex];
            }
        }
        return -1;
    }

    public void SetTile(int t_xIndex, int t_yIndex, int t_newTileValue)
    {
        m_map[t_xIndex, t_yIndex] = t_newTileValue;
    }
}
