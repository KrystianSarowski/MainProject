using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public MapGrid m_roomGrid;

    private void Awake()
    { 
        m_roomGrid.m_height = GameplayMananger.s_seedRandom.Next(5, 15);
        m_roomGrid.m_width = GameplayMananger.s_seedRandom.Next(5, 15);

        m_roomGrid.CreateMap();
        GenerateRoom();
    }

    void GenerateRoom()
    {
        RandomFillRoom();
    }

    void RandomFillRoom()
    {
        for (int x = 0; x < m_roomGrid.m_width; x++)
        {
            for (int y = 0; y < m_roomGrid.m_height; y++)
            {
                if (x == 0 || x == m_roomGrid.m_width - 1 || y == 0 || y == m_roomGrid.m_height - 1)
                {
                    m_roomGrid.SetTile(x, y, 1);
                }
                else
                {
                    m_roomGrid.SetTile(x, y, (GameplayMananger.s_seedRandom.Next(0, 100) < 45) ? 1 : 0);
                }
            }
        }
    }
}
