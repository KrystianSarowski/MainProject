﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public struct GenerationStats
{
    public float m_avergeTime;
    public float m_shortestTime;
    public float m_longestTime;

    public int m_avergeRoomsPlaced;
    public int m_mostRoomsPlaced;
    public int m_leastRoomsPlaced;
}

public struct GenerationData
{
    public int m_mapsToGenerate;
    public int m_numOfRooms;
    public int m_levelWidth;
    public int m_levelHeight;
}

public class HeatSceneManager : MonoBehaviour
{
    List<TileGrid> m_mapGrids;
    List<List<RoomLayout>> m_roomLists;

    public GameObject m_heatTile;

    GenerationData m_currentData;
    GenerationData m_nextData;

    int[,] m_heatMapBottomUp;
    int[,] m_heatMapTopDown;

    GameObject[,] m_tilesBottomUp;
    GameObject[,] m_tilesTopDown;
    GameObject m_camera;

    Canvas m_canvas;

    public Color m_startColour;
    public Color m_endColour;

    GenerationStats m_bottomUpStats;
    GenerationStats m_topDownStats;

    [SerializeField]
    Slider m_mapSizeSlider;

    [SerializeField]
    Slider m_mapsToGenerateSlider;

    [SerializeField]
    Slider m_roomsToGenerateSlider;

    [SerializeField]
    GameObject m_heatTilesCanvas;

    public List<TMP_Text> m_bottomUpStatsText;
    public List<TMP_Text> m_topDownStatsText;

    void Start()
    {
        m_canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        m_camera = GameObject.FindGameObjectWithTag("MainCamera");

        ChangeMapSize();
        ChangeMapsToGenerate();
        ChangeRoomsToGenerate();
    }

    void Update()
    {
        m_camera.transform.Translate(Vector3.up * Input.GetAxis("Mouse ScrollWheel") * 2000);

        if (m_camera.transform.position.y > 1600.0f)
        {
            Vector3 pos = m_camera.transform.position;

            pos.y = 1600.0f;

            m_camera.transform.position = pos;
        }

        else if (m_camera.transform.position.y < -6000.0f)
        {
            Vector3 pos = m_camera.transform.position;

            pos.y = -6000.0f;

            m_camera.transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        StopAllCoroutines();
        ResetData();

        foreach (Transform child in m_heatTilesCanvas.transform)
        {
            Destroy(child.gameObject);
        }

        m_currentData = m_nextData;

        m_heatMapBottomUp = new int[m_currentData.m_levelWidth, m_currentData.m_levelHeight];
        m_tilesBottomUp = new GameObject[m_currentData.m_levelWidth, m_currentData.m_levelHeight];

        m_heatMapTopDown = new int[m_currentData.m_levelWidth, m_currentData.m_levelHeight];
        m_tilesTopDown = new GameObject[m_currentData.m_levelWidth, m_currentData.m_levelHeight];

        int tilseSize = 4000 / m_currentData.m_levelWidth;
        int secondGridOffset = tilseSize * m_currentData.m_levelWidth + 1000;

        for (int x = 0; x < m_currentData.m_levelWidth; x++)
        {
            for (int y = 0; y < m_currentData.m_levelHeight; y++)
            {
                m_tilesBottomUp[x, y] = Instantiate(m_heatTile, m_canvas.transform);
                m_tilesBottomUp[x, y].transform.SetParent(m_canvas.transform);
                m_tilesBottomUp[x, y].GetComponent<RectTransform>().sizeDelta = new Vector2(tilseSize, tilseSize);
                m_tilesBottomUp[x, y].transform.position += new Vector3(tilseSize * x, tilseSize * y, 0);
                m_tilesBottomUp[x, y].GetComponent<Image>().color = m_startColour;

                m_tilesTopDown[x, y] = Instantiate(m_heatTile, m_canvas.transform);
                m_tilesTopDown[x, y].transform.SetParent(m_canvas.transform);
                m_tilesTopDown[x, y].GetComponent<RectTransform>().sizeDelta = new Vector2(tilseSize, tilseSize);
                m_tilesTopDown[x, y].transform.position += new Vector3(secondGridOffset + tilseSize * x, tilseSize * y, 0);
                m_tilesTopDown[x, y].GetComponent<Image>().color = m_startColour;
            }
        }

        m_mapGrids = new List<TileGrid>();
        m_roomLists = new List<List<RoomLayout>>();

        m_mapGrids.Add(new TileGrid());
        m_mapGrids.Add(new TileGrid());

        m_roomLists.Add(new List<RoomLayout>());
        m_roomLists.Add(new List<RoomLayout>());

        StartCoroutine(GenerateHeatMapBottomUp());
        StartCoroutine(GenerateHeatMapTopDown());
    }

    public void ResetData()
    {
        m_bottomUpStats.m_avergeTime = 0.0f;
        m_bottomUpStats.m_shortestTime = 0.0f;
        m_bottomUpStats.m_longestTime = 0.0f;

        m_bottomUpStats.m_avergeRoomsPlaced = 0;
        m_bottomUpStats.m_mostRoomsPlaced = 0;
        m_bottomUpStats.m_leastRoomsPlaced = 0;

        m_topDownStats.m_avergeTime = 0.0f;
        m_topDownStats.m_shortestTime = 0.0f;
        m_topDownStats.m_longestTime = 0.0f;

        m_topDownStats.m_avergeRoomsPlaced = 0;
        m_topDownStats.m_mostRoomsPlaced = 0;
        m_topDownStats.m_leastRoomsPlaced = 0;
    }

    IEnumerator GenerateHeatMapBottomUp()
    {
        for (int x = 0; x < m_currentData.m_levelWidth; x++)
        {
            for(int y = 0; y < m_currentData.m_levelHeight; y++)
            {
                m_heatMapBottomUp[x, y] = 0;
            }
        }

        float mapsGenerated = 0;
        m_bottomUpStats.m_shortestTime = 10000.0f;
        m_bottomUpStats.m_leastRoomsPlaced = 1000;
        UpdateStatsText(m_bottomUpStatsText, m_bottomUpStats);

        while (mapsGenerated != m_currentData.m_mapsToGenerate)
        {
            float startTime = Time.time;

            m_mapGrids[0] = new TileGrid(m_currentData.m_levelWidth, m_currentData.m_levelHeight);
            m_mapGrids[0].CreateTileGrid();

            m_roomLists[0] = new List<RoomLayout>();

            for (int i = 0; i < m_currentData.m_numOfRooms; i++)
            {
                m_roomLists[0].Add(new RoomLayout());
                m_roomLists[0][i].SetID(i);
                m_roomLists[0][i].GenerateLayout();
            }

            yield return StartCoroutine(CreateLevelBottomUp());

            float finishTime = Time.time;

            mapsGenerated = UpdateHeatMap(mapsGenerated, m_mapGrids[0], m_heatMapBottomUp, m_tilesBottomUp);
            m_bottomUpStats = UpdateTimeStats(mapsGenerated, finishTime - startTime, m_bottomUpStats);
            m_bottomUpStats = UpdatePlacementStats(mapsGenerated, m_roomLists[0], m_bottomUpStats);
            UpdateStatsText(m_bottomUpStatsText, m_bottomUpStats);
        }

        Debug.Log(m_bottomUpStats.m_avergeTime);
        Debug.Log(m_bottomUpStats.m_avergeRoomsPlaced);
    }

    IEnumerator GenerateHeatMapTopDown()
    {
        for (int x = 0; x < m_currentData.m_levelWidth; x++)
        {
            for (int y = 0; y < m_currentData.m_levelHeight; y++)
            {
                m_heatMapBottomUp[x, y] = 0;
            }
        }

        float mapsGenerated = 0;
        m_topDownStats.m_shortestTime = 10000.0f;
        m_topDownStats.m_leastRoomsPlaced = 1000;
        UpdateStatsText(m_topDownStatsText, m_topDownStats);

        while (mapsGenerated != m_currentData.m_mapsToGenerate)
        {
            float startTime = Time.time;

            m_mapGrids[1] = new TileGrid(m_currentData.m_levelWidth, m_currentData.m_levelHeight);
            m_mapGrids[1].CreateTileGrid();
            m_roomLists[1] = new List<RoomLayout>();

            for (int i = 0; i < m_currentData.m_numOfRooms; i++)
            {
                m_roomLists[1].Add(new RoomLayout());
                m_roomLists[1][i].SetID(i);
                m_roomLists[1][i].GenerateLayout();
            }

            yield return StartCoroutine(CreateLevelTopDown());

            float finishTime = Time.time;

            mapsGenerated = UpdateHeatMap(mapsGenerated, m_mapGrids[1], m_heatMapTopDown, m_tilesTopDown);
            m_topDownStats = UpdateTimeStats(mapsGenerated, finishTime - startTime, m_topDownStats);   
            m_topDownStats = UpdatePlacementStats(mapsGenerated, m_roomLists[1], m_topDownStats);
            UpdateStatsText(m_topDownStatsText, m_topDownStats);
        }

        Debug.Log(m_topDownStats.m_avergeTime);
        Debug.Log(m_topDownStats.m_avergeRoomsPlaced);
    }

    GenerationStats UpdateTimeStats(float t_mapsGenerated, float t_elapsedTime, GenerationStats t_stats)
    {
        if(t_stats.m_longestTime < t_elapsedTime)
        {
            t_stats.m_longestTime = t_elapsedTime;
        }

        if(t_stats.m_shortestTime > t_elapsedTime)
        {
            t_stats.m_shortestTime = t_elapsedTime;
        }

        t_stats.m_avergeTime = (t_stats.m_avergeTime * (t_mapsGenerated - 1) + t_elapsedTime) / t_mapsGenerated;

        return t_stats;
    }

    GenerationStats UpdatePlacementStats(float t_mapsGenerated, List<RoomLayout> t_rooms, GenerationStats t_stats)
    {
        int count;

        for(count = 0; count < t_rooms.Count; count++)
        {
            if(!t_rooms[count].m_roomAdded)
            {
                break;
            }
        }

        if (t_stats.m_mostRoomsPlaced < count)
        {
            t_stats.m_mostRoomsPlaced = count;
        }

        if (t_stats.m_leastRoomsPlaced > count)
        {
            t_stats.m_leastRoomsPlaced = count;
        }

        t_stats.m_avergeRoomsPlaced = (int)((t_stats.m_avergeRoomsPlaced * (t_mapsGenerated - 1) + count) / t_mapsGenerated);

        return t_stats;
    }

    void UpdateStatsText(List<TMP_Text> t_statsText, GenerationStats t_stats)
    {
        t_statsText[0].text = "Average Time: " + t_stats.m_avergeTime.ToString("F3") + "s";
        t_statsText[1].text = "Shortest Time: " + t_stats.m_shortestTime.ToString("F3") + "s";
        t_statsText[2].text = "Longest Time: " + t_stats.m_longestTime.ToString("F3") + "s";

        t_statsText[3].text = "Average Rooms Placed: " + t_stats.m_avergeRoomsPlaced + "/" + m_currentData.m_numOfRooms;
        t_statsText[4].text = "Least Rooms Placed: " + t_stats.m_leastRoomsPlaced + "/" + m_currentData.m_numOfRooms;
        t_statsText[5].text = "Most Rooms Placed: " + t_stats.m_mostRoomsPlaced + "/" + m_currentData.m_numOfRooms;
    }

    float UpdateHeatMap(float t_mapsGenerated, TileGrid t_grid, int[,] t_heatMap, GameObject[,] t_tileMap)
    {
        t_mapsGenerated++;

        for (int x = 0; x < m_currentData.m_levelWidth; x++)
        {
            for (int y = 0; y < m_currentData.m_levelHeight; y++)
            {
                if (t_grid.GetTile(new GridIndex(x, y)).GetOwnerID() != -1)
                {
                    t_heatMap[x, y]++;
                }

                t_tileMap[x, y].GetComponent<Image>().color = 
                    Color.Lerp(m_startColour, m_endColour, t_heatMap[x, y] / t_mapsGenerated);
            }
        }

        return t_mapsGenerated;
    }

    IEnumerator CreateLevelBottomUp()
    {
        List<TileArc> shortestRoomArcs;
        List<TileArc> exitArcs = new List<TileArc>();

        yield return StartCoroutine(BottomUpGenerator.PlaceRooms(m_mapGrids[0], m_roomLists[0]));

        BottomUpGenerator.CreateRoomArcs(m_mapGrids[0], m_roomLists[0]);

        shortestRoomArcs = BottomUpGenerator.CreateMST(m_roomLists[0]);

        BottomUpGenerator.CreateExitArcs(shortestRoomArcs, exitArcs, m_mapGrids[0], m_roomLists[0]);
        BottomUpGenerator.CreateCorridors(exitArcs, m_mapGrids[0]);

        yield return null;
    }

    IEnumerator CreateLevelTopDown()
    {
        List<TileArc> shortestRoomArcs = new List<TileArc>();
        List<TileArc> exitArcs = new List<TileArc>();

        GridArea root = new GridArea();

        yield return StartCoroutine(TopDownGenerator.PlaceRooms(root, m_mapGrids[1], m_roomLists[1]));

        TopDownGenerator.ConnectRooms(root, shortestRoomArcs);

        TopDownGenerator.CreateExitArcs(shortestRoomArcs, exitArcs, m_mapGrids[1], m_roomLists[1]);
        TopDownGenerator.CreateCorridors(exitArcs, m_mapGrids[1]);

        yield return null;
    }

    public void Return()
    {
        GameplayManager.LoadScene("MenuScene");
    }

    public void ChangeMapSize()
    {
        m_nextData.m_levelWidth = (int)m_mapSizeSlider.value;
        m_nextData.m_levelHeight = (int)m_mapSizeSlider.value;
    }

    public void ChangeMapsToGenerate()
    {
        m_nextData.m_mapsToGenerate = (int)m_mapsToGenerateSlider.value;
    }

    public void ChangeRoomsToGenerate()
    {
        m_nextData.m_numOfRooms = (int)m_roomsToGenerateSlider.value;
    }
}
