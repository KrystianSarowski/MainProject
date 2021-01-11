using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    List<RoomLayout> m_layouts;
    List<Room> m_rooms;
    List<TileArc> m_shortestRoomArcs = new List<TileArc>();
    List<TileArc> m_exitArcs = new List<TileArc>();

    public GameObject m_playerPrefab;
    public GameObject m_exitPrefab;
    public GameObject m_roomPrefab;

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

        m_mapGrid = new TileGrid(m_levelWidth, m_levelHight);
        m_mapGrid.CreateTileGrid();

        if (m_mapGrid != null)
        {
            m_layouts = new List<RoomLayout>();

            for(int i =0; i < m_numOfRooms; i++)
            {
                m_layouts.Add(new RoomLayout());
                m_layouts[i].GenerateLayout();
            }

            switch (FindObjectOfType<GameplayManager>().m_generationType)
            {
                case GenerationType.BottomUp:
                    StartCoroutine(CreateLevelBottomUp());
                    break;
                case GenerationType.TopDown:
                    StartCoroutine(CreateLevelTopDown());
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator CreateLevelTopDown()
    {
        GridArea root = new GridArea();

        yield return StartCoroutine(TopDownGenerator.PlaceRooms(root, m_mapGrid, m_layouts));

        TopDownGenerator.ConnectRooms(root, m_shortestRoomArcs);

        TopDownGenerator.CreateExitArcs(m_shortestRoomArcs, m_exitArcs, m_mapGrid, m_layouts);
        TopDownGenerator.CreateCorridors(m_exitArcs, m_mapGrid);

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(m_mapGrid, m_tileSize, m_wallHeight);
        CreateCeilingAndFloor();
        SpawnPlayer();
        SpawnExit();
        BuildRooms();

        yield return null;
    }

    IEnumerator CreateLevelBottomUp()
    {
        yield return StartCoroutine(BottomUpGenerator.PlaceRooms(m_mapGrid, m_layouts));

        BottomUpGenerator.CreateRoomArcs(m_mapGrid, m_layouts);

        m_shortestRoomArcs = BottomUpGenerator.CreateMST(m_layouts);

        BottomUpGenerator.CreateExitArcs(m_shortestRoomArcs, m_exitArcs, m_mapGrid, m_layouts);
        BottomUpGenerator.CreateCorridors(m_exitArcs, m_mapGrid);

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(m_mapGrid, m_tileSize, m_wallHeight);
        CreateCeilingAndFloor();
        SpawnPlayer();
        SpawnExit();
        BuildRooms();

        yield return null;
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
        floor.tag = "Ground";

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
        GridIndex nodeIndex = m_layouts[0].GetNodePositonOnMap();

        Vector3 spawnPosition = new Vector3(nodeIndex.m_x * m_tileSize + m_halfTileSize, -m_wallHeight / 2.0f, nodeIndex.m_y * m_tileSize + m_halfTileSize);

        Instantiate(m_playerPrefab, spawnPosition, Quaternion.identity);
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
            //DrawTileTypes();
            DrawMST();
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
                switch (m_mapGrid.GetTile(new GridIndex(x, y)).GetTileType())
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
        if (m_exitArcs.Count != 0)
        {
            foreach (TileArc arc in m_exitArcs)
            {
                GridIndex roomMapIndex = arc.GetStartPos();
                Vector3 roomPos = new Vector3(
                    roomMapIndex.m_x * m_tileSize + m_halfTileSize,
                    0,
                    roomMapIndex.m_y * m_tileSize + m_halfTileSize
                    );

                GridIndex conRoomMapIndex = arc.GetTargetPos();
                Vector3 conRoomPos = new Vector3(
                    conRoomMapIndex.m_x * m_tileSize + m_halfTileSize,
                    0,
                    conRoomMapIndex.m_y * m_tileSize + m_halfTileSize
                    );

                Gizmos.color = Color.green;
                Gizmos.DrawLine(conRoomPos, roomPos);
            }
        }
    }

    void DrawMST()
    {
        if (m_shortestRoomArcs.Count != 0)
        {
            foreach (TileArc arc in m_shortestRoomArcs)
            {
                GridIndex roomMapIndex = arc.GetStartPos();
                Vector3 roomPos = new Vector3(
                    roomMapIndex.m_x * m_tileSize + m_halfTileSize,
                    0,
                    roomMapIndex.m_y * m_tileSize + m_halfTileSize
                    );

                GridIndex conRoomMapIndex = arc.GetTargetPos();
                Vector3 conRoomPos = new Vector3(
                    conRoomMapIndex.m_x * m_tileSize + m_halfTileSize,
                    0,
                    conRoomMapIndex.m_y * m_tileSize + m_halfTileSize
                    );

                Gizmos.color = Color.red;
                Gizmos.DrawLine(conRoomPos, roomPos);
            }
        }
    }

    void DrawConnectionArcs()
    {
        foreach (RoomLayout room in m_layouts)
        {
            if (room.GetID() != -1)
            {
                List<TileArc> arcs = room.m_nodeArcs;

                GridIndex roomMapIndex = room.GetNodePositonOnMap();
                Vector3 roomPos = new Vector3(
                roomMapIndex.m_x * m_tileSize + m_halfTileSize,
                0,
                roomMapIndex.m_y * m_tileSize + m_halfTileSize
                );

                foreach (TileArc arc in arcs)
                {
                    GridIndex conRoomMapIndex = arc.GetTargetPos();
                    Vector3 conRoomPos = new Vector3(
                            conRoomMapIndex.m_x * m_tileSize + m_halfTileSize,
                            0,
                            conRoomMapIndex.m_y * m_tileSize + m_halfTileSize
                            );

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(conRoomPos, roomPos);
                }
            }
        }
    }

    void SpawnExit()
    {
        int index = 0;

        for(int i = 0; i < m_layouts.Count(); i++)
        {
            if(m_layouts[i].m_roomAdded == true)
            {
                index = i;
            }
        }

        GridIndex nodeIndex = m_layouts[index].GetNodePositonOnMap();

        Vector3 spawnPosition = new Vector3(nodeIndex.m_x * m_tileSize + m_halfTileSize, -m_wallHeight + 0.005f, nodeIndex.m_y * m_tileSize + m_halfTileSize);
        
        GameObject exitObject = Instantiate(m_exitPrefab, spawnPosition, Quaternion.identity);

        exitObject.transform.Rotate(Vector3.right * -90);
    }

    void BuildRooms()
    {
        foreach(RoomLayout layout in m_layouts)
        {
            if(layout.m_roomAdded)
            {
                GameObject room = Instantiate(m_roomPrefab, transform);
                room.GetComponent<Room>().SetLayout(layout);
                room.GetComponent<Room>().ImplementLayout(m_tileSize);
            }
        }
    }
}
