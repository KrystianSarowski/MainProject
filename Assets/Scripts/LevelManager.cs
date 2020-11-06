using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public MapGrid m_mapGrid;

    public GameObject m_roomPrefab;
    GameObject[] m_rooms;

    int numOfRooms = 10;

    //Start is called before the first frame update
    void Start()
    {
        m_mapGrid.CreateMap();

        if (m_mapGrid != null)
        {
            m_rooms = new GameObject[numOfRooms];

            for(int i =0; i < numOfRooms; i++)
            {
                m_rooms[i] = Instantiate(m_roomPrefab);
            }

            PlaceRooms();
        }
    }

    void PlaceRooms()
    {
        int roomsPlaced = 0;
        int safeLockCount = 0;

        while (roomsPlaced < numOfRooms && safeLockCount < 400)
        {
            Pair<int, int> mapPos = new Pair<int, int>(GameplayMananger.s_seedRandom.Next(0, m_mapGrid.m_width),
               GameplayMananger.s_seedRandom.Next(0, m_mapGrid.m_height));

            MapGrid roomGrid = m_rooms[roomsPlaced].GetComponent<MapGrid>();

            if (ValidateRoomPlacement(mapPos.m_first, mapPos.m_second, roomGrid.m_width, roomGrid.m_height))
            {
                for (int x = 0; x < roomGrid.m_width; x++)
                {
                    for (int y = 0; y < roomGrid.m_height; y++)
                    {
                        m_mapGrid.SetTile(mapPos.m_first + x,
                            mapPos.m_second + y,
                            roomGrid.GetTile(x, y));
                    }
                }
                roomsPlaced++;
            }

            safeLockCount++;
        }
    }

    bool ValidateRoomPlacement(int t_xIndex, int t_yIndex, int t_roomWidth, int t_roomHeight)
    {
        bool canBePlaced = true;

        //Check each tile within the room space aswell as 1 tile layer
        //around the bounds of the room to make sure it is surrounded by walls.
        for (int x = -1; x < t_roomWidth + 1; x++)
        {
            for (int y = -1; y < t_roomHeight + 1; y++)
            {
                if(t_xIndex + x < 0 || t_xIndex + x >= m_mapGrid.m_width 
                    || t_yIndex + y < 0 || t_yIndex + y >= m_mapGrid.m_height)
                {
                    canBePlaced = false;
                    return canBePlaced;
                }
                else if(m_mapGrid.GetTile(t_xIndex + x, t_yIndex + y) == 0)
                {
                    canBePlaced = false;
                    return canBePlaced;
                }
            }
        }

        return canBePlaced;
    }

    void OnDrawGizmos()
    {
        if (m_mapGrid != null)
        {
            for (int x = 0; x < m_mapGrid.m_width; x++)
            {
                for (int y = 0; y < m_mapGrid.m_height; y++)
                {
                    Gizmos.color = (m_mapGrid.GetTile(x, y) == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-m_mapGrid.m_width / 2 + x + 0.5f, 0, -m_mapGrid.m_height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
