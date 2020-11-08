using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public TileGrid m_roomGrid;

    int m_roomID = -1;

    public void GenerateRoom()
    {
        m_roomGrid = new TileGrid();

        m_roomGrid.m_height = GameplayMananger.s_seedRandom.Next(5, 15);
        m_roomGrid.m_width = GameplayMananger.s_seedRandom.Next(5, 15);

        m_roomGrid.CreateTileGrid();
        RandomFillRoom();

        m_roomGrid.SetTileType(m_roomGrid.m_width / 2, m_roomGrid.m_height / 2, TileType.Node);
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
}
