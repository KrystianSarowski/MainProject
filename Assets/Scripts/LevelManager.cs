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

    public GameObject m_playerPrefab;

    public int m_numOfRooms = 40;
    public int m_levelWidth;
    public int m_levelHight;

    [Range(1, 4)]
    public int m_tileSize;

    [Range(1.0f, 4.0f)]
    public float m_wallHeight;

    float m_halfTileSize;

    public Material m_floorMaterial;
    public Material m_ceilingMaterial;

    bool drawAllArcs = false;

    //Start is called before the first frame update
    void Start()
    {
        m_halfTileSize = m_tileSize / 2.0f;

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
        meshGen.GenerateMesh(m_mapGrid, m_tileSize, m_wallHeight);
        CreateCeilingAndFloor();
        SpawnPlayer();
    }

    void CreateCeilingAndFloor()
    {
        float sizeDivisor = 10 / m_tileSize;
        float positionScalar = 0.5f * m_tileSize;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.position = new Vector3(m_levelWidth * positionScalar, -m_wallHeight, m_levelHight * positionScalar);
        floor.transform.localScale = new Vector3(m_levelWidth / sizeDivisor, 1, m_levelHight / sizeDivisor);
        floor.AddComponent<MeshCollider>();
        m_floorMaterial.mainTextureScale = new Vector2(m_levelWidth, m_levelHight);
        floor.GetComponent<Renderer>().material = m_floorMaterial;

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.transform.position = new Vector3(m_levelWidth * positionScalar, 0, m_levelHight * positionScalar);
        ceiling.transform.localScale = new Vector3(m_levelWidth / sizeDivisor, 1, m_levelHight / sizeDivisor);
        ceiling.transform.Rotate(Vector3.right, 180); 
        ceiling.AddComponent<MeshCollider>();
        m_ceilingMaterial.mainTextureScale = new Vector2(m_levelWidth, m_levelHight);
        ceiling.GetComponent<Renderer>().material = m_ceilingMaterial;
    }

    public void SpawnPlayer()
    {
        Pair<int, int> nodeIndex = m_rooms[0].GetNodePositonOnMap();

        Vector3 spawnPosition = new Vector3(nodeIndex.m_first * m_tileSize + m_halfTileSize, -m_wallHeight / 2.0f, nodeIndex.m_second * m_tileSize + m_halfTileSize);

        Instantiate(m_playerPrefab, spawnPosition, Quaternion.identity);
        m_rooms[0].GetNodePositonOnMap();
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
            DrawTileTypes();
            DrawExitArcs();

            if (drawAllArcs)
            {
                DrawConnectionArcs();
            }
        }
    }

    void DrawTileTypes()
    {
        for (int x = 0; x < m_mapGrid.m_width; x++)
        {
            for (int y = 0; y < m_mapGrid.m_height; y++)
            {
                switch (m_mapGrid.GetTile(x, y).GetTileType())
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

                Vector3 pos = new Vector3(x * m_tileSize + m_halfTileSize, -1, y * m_tileSize + m_halfTileSize);
                Gizmos.DrawCube(pos, Vector3.one * m_tileSize);
            }
        }
    }

    void DrawExitArcs()
    {
        if (m_shortestRoomArcs.Count != 0)
        {
            foreach (TileArc arc in m_exitArcs)
            {
                Pair<int, int> roomMapIndex = arc.GetStartPos();
                Vector3 roomPos = new Vector3(
                    roomMapIndex.m_first * m_tileSize + m_halfTileSize,
                    1,
                    roomMapIndex.m_second * m_tileSize + m_halfTileSize
                    );

                Pair<int, int> conRoomMapIndex = arc.GetTargetPos();
                Vector3 conRoomPos = new Vector3(
                    conRoomMapIndex.m_first * m_tileSize + m_halfTileSize,
                    1,
                    conRoomMapIndex.m_second * m_tileSize + m_halfTileSize
                    );

                Gizmos.color = Color.green;
                Gizmos.DrawLine(conRoomPos, roomPos);
            }
        }
    }

    void DrawConnectionArcs()
    {
        foreach (Room room in m_rooms)
        {
            if (room.GetPositionIndex() != null)
            {
                List<TileArc> arcs = room.m_nodeArcs;

                Pair<int, int> roomMapIndex = room.GetNodePositonOnMap();
                Vector3 roomPos = new Vector3(
                roomMapIndex.m_first * m_tileSize + m_halfTileSize,
                1,
                roomMapIndex.m_second * m_tileSize + m_halfTileSize
                );

                foreach (TileArc arc in arcs)
                {
                    Pair<int, int> conRoomMapIndex = arc.GetTargetRoom().GetNodePositonOnMap();
                    Vector3 conRoomPos = new Vector3(
                            conRoomMapIndex.m_first * m_tileSize + m_halfTileSize,
                            1,
                            conRoomMapIndex.m_second * m_tileSize + m_halfTileSize
                            );

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(conRoomPos, roomPos);
                }
            }
        }
    }
}
