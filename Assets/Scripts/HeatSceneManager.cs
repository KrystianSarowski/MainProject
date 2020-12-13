using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatSceneManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    List<Room> m_rooms;
    List<TileArc> m_shortestRoomArcs = new List<TileArc>();
    List<TileArc> m_exitArcs = new List<TileArc>();

    public int m_mapsToGenerate = 10;
    public int m_mapsGenerated = 0;

    int m_numOfRooms = 10;
    int m_levelWidth = 40;
    int m_levelHight = 40;

    int[,] m_heatMap;

    private void Start()
    {
        m_heatMap = new int[m_levelWidth, m_levelHight];


    }

    void GenerateHeatMap()
    {
        m_rooms.Clear();
        m_shortestRoomArcs.Clear();
        m_exitArcs.Clear();

        for (int x = 0; x < m_levelWidth; x++)
        {
            for(int y = 0; y < m_levelHight; y++)
            {
                m_heatMap[x, y] = 0;
            }
        }

        m_mapsGenerated = 0;

        m_mapGrid = new TileGrid();
        m_mapGrid.m_width = m_levelWidth;
        m_mapGrid.m_height = m_levelHight;
        m_mapGrid.CreateTileGrid();

        if (m_mapGrid != null)
        {
            m_rooms = new List<Room>();

            for (int i = 0; i < m_numOfRooms; i++)
            {
                m_rooms.Add(new Room());
                m_rooms[i].SetRoomID(i);
                m_rooms[i].GenerateRoom();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
