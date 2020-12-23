using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeatSceneManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    List<Room> m_rooms;
    List<TileArc> m_shortestRoomArcs = new List<TileArc>();
    List<TileArc> m_exitArcs = new List<TileArc>();

    public GameObject m_heatTile;

    public int m_mapsToGenerate = 10;
    public float m_mapsGenerated = 0;

    int m_numOfRooms = 10;
    int m_levelWidth = 40;
    int m_levelHight = 40;

    int[,] m_heatMap;
    GameObject[,] m_heatTiles;

    Canvas m_canvas;

    public Color m_startColour;
    public Color m_endColour;

    bool bob = false;

    private void Start()
    {
        m_canvas = FindObjectOfType<Canvas>();

        m_heatMap = new int[m_levelWidth, m_levelHight];
        m_heatTiles = new GameObject[m_levelWidth, m_levelHight];

        for(int x = 0; x < m_levelWidth; x++)
        {
            for(int y = 0; y < m_levelHight; y++)
            {
                m_heatTiles[x, y] = Instantiate(m_heatTile, m_canvas.transform);
                m_heatTiles[x, y].transform.SetParent(m_canvas.transform);
                m_heatTiles[x, y].transform.position += new Vector3(100 * x, 100 * y, 0);
            }
        }

        StartCoroutine(GenerateHeatMap());
    }

    IEnumerator GenerateHeatMap()
    {
        for (int x = 0; x < m_levelWidth; x++)
        {
            for(int y = 0; y < m_levelHight; y++)
            {
                m_heatMap[x, y] = 0;
            }
        }

        m_mapsGenerated = 0;

        while(m_mapsGenerated != m_mapsToGenerate)
        {
            m_mapGrid = new TileGrid();
            m_mapGrid.m_width = m_levelWidth;
            m_mapGrid.m_height = m_levelHight;
            m_mapGrid.CreateTileGrid();

            m_rooms = new List<Room>();

            for (int i = 0; i < m_numOfRooms; i++)
            {
                m_rooms.Add(new Room());
                m_rooms[i].SetRoomID(i);
                m_rooms[i].GenerateRoom();
            }

            yield return StartCoroutine(CreateLevelBottomUp());

            m_mapsGenerated++;

            for (int x = 0; x < m_levelWidth; x++)
            {
                for (int y = 0; y < m_levelHight; y++)
                {
                    if(m_mapGrid.GetTile(new GridIndex(x,y)).GetTileType() != TileType.Wall)
                    {
                        m_heatMap[x, y]++;
                    }
                    m_heatTiles[x, y].GetComponent<Image>().color = Color.Lerp(m_startColour, m_endColour, m_heatMap[x, y] / m_mapsGenerated);
                }
            }
        }
    }

    IEnumerator CreateLevelBottomUp()
    {
        m_shortestRoomArcs.Clear();
        m_exitArcs.Clear();

        yield return StartCoroutine(BottomUpGenerator.PlaceRooms(m_mapGrid, m_rooms));

        BottomUpGenerator.CreateRoomArcs(m_mapGrid, m_rooms);

        m_shortestRoomArcs = BottomUpGenerator.CreateMST(m_rooms);

        BottomUpGenerator.CreateExitArcs(m_shortestRoomArcs, m_exitArcs, m_mapGrid, m_rooms);
        BottomUpGenerator.CreateCorridors(m_exitArcs, m_mapGrid);

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
