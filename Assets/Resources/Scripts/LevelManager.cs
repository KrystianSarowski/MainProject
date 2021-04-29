using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//@Author Krystian Sarowski

public class LevelManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    List<RoomLayout> m_layouts;
    List<Room> m_rooms = new List<Room>();
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

    [Range(1.0f, 4.0f)]
    public float m_innerWallHeight;

    [SerializeField]
    Material m_floorMaterial;

    [SerializeField]
    Material m_ceilingMaterial;

    [SerializeField]
    Material m_lowerCeilingMaterial;

    [SerializeField]
    MeshGenerator m_wallsGenerator;

    [SerializeField]
    MeshGenerator m_innerWallGenerator;

    //Start is called before the first frame update
    void Start()
    {
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

        m_lowerCeilingMaterial.mainTextureScale = new Vector2(m_levelWidth, m_levelHight);
    }

    IEnumerator CreateLevelTopDown()
    {
        GridArea root = new GridArea();

        yield return StartCoroutine(TopDownGenerator.PlaceRooms(root, m_mapGrid, m_layouts));

        TopDownGenerator.ConnectRooms(root, m_shortestRoomArcs);

        TopDownGenerator.CreateExitArcs(m_shortestRoomArcs, m_exitArcs, m_mapGrid, m_layouts);
        TopDownGenerator.CreateCorridors(m_exitArcs, m_mapGrid);

        InitializeLevelData();

        yield return null;
    }

    IEnumerator CreateLevelBottomUp()
    {
        yield return StartCoroutine(BottomUpGenerator.PlaceRooms(m_mapGrid, m_layouts));

        BottomUpGenerator.CreateRoomArcs(m_mapGrid, m_layouts);

        m_shortestRoomArcs = BottomUpGenerator.CreateMST(m_layouts);

        BottomUpGenerator.CreateExitArcs(m_shortestRoomArcs, m_exitArcs, m_mapGrid, m_layouts);
        BottomUpGenerator.CreateCorridors(m_exitArcs, m_mapGrid);

        InitializeLevelData();

        yield return null;
    }

    void InitializeLevelData()
    {
        RemoveBossRoomInnerWalls();

        m_wallsGenerator.GenerateMesh(m_mapGrid, m_tileSize, m_wallHeight, TileType.Wall);
        m_innerWallGenerator.GenerateMesh(m_mapGrid, m_tileSize, m_innerWallHeight, TileType.InnerWall);

        CreateCeilingAndFloor();
        BuildRooms();
        SpawnPlayer();
        SpawnExit();
        SetShopKeepers();
        SetPowerUpRooms();
        ClearRoomLayoutArcs();
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
        m_rooms[0].SetRoomType(RoomType.Spawn);

        Instantiate(m_playerPrefab, m_rooms[0].m_position, Quaternion.identity);
    }

    void SpawnExit()
    {
        m_rooms[m_rooms.Count - 1].SetRoomType(RoomType.Boss);
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
                room.GetComponent<Room>().SetRoomType(RoomType.Combat);
                room.GetComponent<Room>().SetRoomSize();
                m_rooms.Add(room.GetComponent<Room>());
            }
        }
    }

    void SetShopKeepers()
    {
        int shopKeeperCount = 0;

        foreach (Room room in m_rooms)
        {
            int random = GameplayManager.s_seedRandom.Next(0, 100);

            if(random < 100)
            {
                room.SetSpawnShopkeeper(true);
                shopKeeperCount++;
            }
        }

        if(shopKeeperCount == 0)
        {
            int random = GameplayManager.s_seedRandom.Next(1, m_rooms.Count - 1);
            m_rooms[random].SetSpawnShopkeeper(true);
        }
    }

    void SetPowerUpRooms()
    {
        int roomCount = 0;

        for(int i = 1; i < m_rooms.Count - 1; i++)
        {
            int random = GameplayManager.s_seedRandom.Next(0, 100);

            if (random < 15)
            {
                m_rooms[i].SetRoomType(RoomType.Powerup);
                roomCount++;
            }
        }

        if (roomCount == 0)
        {
            int random = GameplayManager.s_seedRandom.Next(1, m_rooms.Count - 2);
            m_rooms[random].SetRoomType(RoomType.Powerup);
        }
    }

    void RemoveBossRoomInnerWalls()
    {
        int lastIndex = 0;

        for (int i =0; i < m_layouts.Count; i++)
        {
            if(!m_layouts[i].m_roomAdded)
            {
                break;
            }

            lastIndex = i;
        }

        m_layouts[lastIndex].RemoveInnerWalls();
        TileGrid roomGrid = m_layouts[lastIndex].m_grid;

        for (int x = 0; x < roomGrid.m_width; x++)
        {
            GridIndex position = m_layouts[lastIndex].GetPositionIndex();

            for (int y = 0; y < roomGrid.m_height; y++)
            {
                GridIndex posOnMap;

                posOnMap.m_x = position.m_x + x;
                posOnMap.m_y = position.m_y + y;

                m_mapGrid.SetTileType(posOnMap, roomGrid.GetTile(new GridIndex(x, y)).GetTileType());
            }
        }
    }

    void ClearRoomLayoutArcs()
    {
        foreach (RoomLayout layout in m_layouts)
        {
            layout.RemoveNodeArcs();
        }
    }
}