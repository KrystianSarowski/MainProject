using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    Room[] m_rooms;

    int numOfRooms = 40;
    public int m_levelWidth;
    public int m_levelHight;

    //Start is called before the first frame update
    void Start()
    {
        m_mapGrid = new TileGrid();
        m_mapGrid.m_width = m_levelWidth;
        m_mapGrid.m_height = m_levelHight;
        m_mapGrid.CreateTileGrid();

        if (m_mapGrid != null)
        {
            m_rooms = new Room[numOfRooms];

            for(int i =0; i < numOfRooms; i++)
            {
                m_rooms[i] = new Room();
                m_rooms[i].SetRoomID(i);
                m_rooms[i].GenerateRoom();
            }

            StartCoroutine("PlaceRooms");
        }
    }

    public IEnumerator PlaceRooms()
    {
        int roomIndex = 0;
        int safeLockCount = 0;

        while (roomIndex < numOfRooms && safeLockCount < 400)
        {
            Pair<int, int> mapPos = new Pair<int, int>(GameplayMananger.s_seedRandom.Next(0, m_mapGrid.m_width),
               GameplayMananger.s_seedRandom.Next(0, m_mapGrid.m_height));

            TileGrid roomGrid = m_rooms[roomIndex].m_roomGrid;

            if (ValidateRoomPlacement(mapPos.m_first, mapPos.m_second, roomGrid.m_width, roomGrid.m_height))
            {
                for (int x = 0; x < roomGrid.m_width; x++)
                {
                    for (int y = 0; y < roomGrid.m_height; y++)
                    {
                        int posOnMapX = mapPos.m_first + x;
                        int posOnMapY = mapPos.m_second + y;

                        m_mapGrid.SetTileType(posOnMapX, posOnMapY, roomGrid.GetTile(x, y).GetTileType());
                        m_mapGrid.GetTile(posOnMapX, posOnMapY).SetOwnerID(m_rooms[roomIndex].GetRoomID());
                    }
                }
                roomIndex++;
            }
            yield return new WaitForSeconds(0.001f);
            safeLockCount++;
        }
        yield return new WaitForSeconds(0.01f);
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
                else if(m_mapGrid.GetTile(t_xIndex + x, t_yIndex + y).GetOwnerID() != -1)
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
                    switch(m_mapGrid.GetTile(x,y).GetTileType())
                    {
                        case TileType.Empty:
                            Gizmos.color = Color.white; 
                            break;
                        case TileType.Node:
                            Gizmos.color = Color.red;
                            break;
                        case TileType.Wall:
                            Gizmos.color = Color.black;
                            break;
                        default:
                            break;
                    }

                    Vector3 pos = new Vector3(-m_mapGrid.m_width / 2 + x + 0.5f, 0, -m_mapGrid.m_height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
