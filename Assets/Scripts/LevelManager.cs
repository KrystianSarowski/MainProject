using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    List<Room> m_rooms;
    List<TileArc> m_shortestRoomArcs = new List<TileArc>();
    List<TileArc> m_exitArcs = new List<TileArc>();

    int m_numOfRooms = 40;
    public int m_levelWidth;
    public int m_levelHight;

    bool drawAllArcs = false;

    //Start is called before the first frame update
    void Start()
    {
        m_mapGrid = new TileGrid();
        m_mapGrid.m_width = m_levelWidth;
        m_mapGrid.m_height = m_levelHight;
        m_mapGrid.CreateTileGrid();

        if (m_mapGrid != null)
        {
            m_rooms = new List<Room>();

            for(int i =0; i < m_numOfRooms; i++)
            {
                m_rooms.Add(new Room());
                m_rooms[i].SetRoomID(i);
                m_rooms[i].GenerateRoom();
            }

            StartCoroutine(CreateLevelBottomUp());
        }
    }

    IEnumerator CreateLevelBottomUp()
    {
        yield return StartCoroutine(BottomUpGenerator.PlaceRooms(m_mapGrid, m_rooms));

        BottomUpGenerator.CreateRoomArcs(m_mapGrid, m_rooms);

        m_shortestRoomArcs = BottomUpGenerator.CreateMST(m_rooms);

        BottomUpGenerator.CreateExitArcs(m_shortestRoomArcs, m_exitArcs, m_mapGrid, m_rooms);
        BottomUpGenerator.CreateCorridors(m_exitArcs, m_mapGrid);

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(m_mapGrid, 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(drawAllArcs)
            {
                drawAllArcs = false;
            }
            else
            {
                drawAllArcs = true;
            }
        }
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
                        case TileType.Exit:
                            Gizmos.color = Color.yellow;
                            break;
                        default:
                            break;
                    }

                    Vector3 pos = new Vector3(x * 1f + 0.5f, 0, y * 1f + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }

            if(m_shortestRoomArcs.Count != 0)
            {
                foreach (TileArc arc in m_exitArcs)
                {
                    Pair<int, int> roomMapIndex = arc.GetStartPos();
                    Vector3 roomPos = new Vector3(
                        roomMapIndex.m_first * 1f + 0.5f,
                        1,
                        roomMapIndex.m_second * 1f + 0.5f
                        );

                    Pair<int, int> conRoomMapIndex = arc.GetTargetPos();
                    Vector3 conRoomPos = new Vector3(
                        conRoomMapIndex.m_first * 1f + 0.5f,
                        1,
                        conRoomMapIndex.m_second * 1f + 0.5f
                        );

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(conRoomPos, roomPos);
                }
            }

            if (drawAllArcs)
            {
                foreach (Room room in m_rooms)
                {
                    if (room.GetPositionIndex() != null)
                    {
                        List<TileArc> arcs = room.m_nodeArcs;

                        Pair<int, int> roomMapIndex = room.GetNodePositonOnMap();
                        Vector3 roomPos = new Vector3(
                        roomMapIndex.m_first * 1f + 0.5f,
                        1,
                        roomMapIndex.m_second * 1f + 0.5f
                        );

                        foreach (TileArc arc in arcs)
                        {
                            Pair<int, int> conRoomMapIndex = arc.GetTargetRoom().GetNodePositonOnMap();
                            Vector3 conRoomPos = new Vector3(
                                    conRoomMapIndex.m_first * 1f + 0.5f,
                                    1,
                                    conRoomMapIndex.m_second * 1f + 0.5f
                                    );

                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(conRoomPos, roomPos);
                        }
                    }
                }
            }  
        } 
    }
}
