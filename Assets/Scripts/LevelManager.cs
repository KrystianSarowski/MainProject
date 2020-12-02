using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    TileGrid m_mapGrid;

    Room[] m_rooms;
    List<TileArc> m_arcsMST = new List<TileArc>();
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
            m_rooms = new Room[m_numOfRooms];

            for(int i =0; i < m_numOfRooms; i++)
            {
                m_rooms[i] = new Room();
                m_rooms[i].SetRoomID(i);
                m_rooms[i].GenerateRoom();
            }

            StartCoroutine("PlaceRooms");
        }
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
    public IEnumerator PlaceRooms()
    {
        int roomIndex = 0;
        int safeLockCount = 0;

        while (roomIndex < m_numOfRooms && safeLockCount < 400)
        {
            Pair<int, int> position = new Pair<int, int>(GameplayMananger.s_seedRandom.Next(0, m_mapGrid.m_width),
               GameplayMananger.s_seedRandom.Next(0, m_mapGrid.m_height));

            TileGrid roomGrid = m_rooms[roomIndex].m_roomGrid;

            if (ValidateRoomPlacement(position.m_first, position.m_second, roomGrid.m_width, roomGrid.m_height))
            {
                for (int x = 0; x < roomGrid.m_width; x++)
                {
                    for (int y = 0; y < roomGrid.m_height; y++)
                    {
                        int posOnMapX = position.m_first + x;
                        int posOnMapY = position.m_second + y;

                        m_mapGrid.SetTileType(posOnMapX, posOnMapY, roomGrid.GetTile(x, y).GetTileType());
                        m_mapGrid.GetTile(posOnMapX, posOnMapY).SetOwnerID(m_rooms[roomIndex].GetRoomID());
                    }
                }

                m_rooms[roomIndex].SetPositionIndex(position);
                roomIndex++;

                yield return new WaitForSeconds(0.1f);
            }
            safeLockCount++;
        }

        CreateRoomArcs();
        m_arcsMST = PrimsAlgorithm.primMST(m_rooms);
        Debug.Log("Count List:" + m_arcsMST.Count);
        createExitArcs();
        CreateCorridors();
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(m_mapGrid, 1);
        yield return new WaitForSeconds(0.01f);
    }

    void createExitArcs()
    {
        List<Pair<int, int>> possibleExits1;
        List<Pair<int, int>> possibleExits2;

        foreach(TileArc arc in m_arcsMST)
        {
            TileArc exitArc = arc;

            possibleExits1 = arc.GetStartRoom().GetPossibleExitsOnMap();
            possibleExits2 = arc.GetTargetRoom().GetPossibleExitsOnMap();

            foreach(Pair<int,int> possibleExit1 in possibleExits1)
            {
                foreach (Pair<int, int> possibleExit2 in possibleExits2)
                {
                    if(exitArc.GetWeigtht() > TileArc.CalculateWeight(possibleExit1, possibleExit2))
                    {
                        exitArc.SetStartPos(possibleExit1);
                        exitArc.SetTargetPos(possibleExit2);
                    }
                }
            }

            exitArc.GetStartRoom().AddExitToRoom(exitArc.GetStartPos());
            exitArc.GetTargetRoom().AddExitToRoom(exitArc.GetTargetPos());
            m_exitArcs.Add(exitArc);
        }

        foreach(Room room in m_rooms)
        {
            List<Pair<int, int>> exitList = room.GetExitsOnMap();

            foreach( Pair<int, int> exitIndex in exitList)
            {
                m_mapGrid.SetTileType(exitIndex.m_first, exitIndex.m_second, TileType.Exit);
            }
        }
    }

    void CreateRoomArcs()
    {
        Debug.Log("Start");
        int curRoomID = -1;
        int prevRoomID = -1;

        for (int x = 0; x < m_levelWidth; x++)
        {
            prevRoomID = -1;

            for (int y = 0; y < m_levelHight; y++)
            {
                curRoomID = m_mapGrid.GetTile(x, y).GetOwnerID();

                if (curRoomID != -1)
                {
                    if(prevRoomID == -1)
                    {
                        prevRoomID = curRoomID;
                    }
                    else if(prevRoomID != curRoomID)
                    {
                        m_rooms[prevRoomID].AddArc(m_rooms[curRoomID]);
                        m_rooms[curRoomID].AddArc(m_rooms[prevRoomID]);
                        prevRoomID = curRoomID;
                    }
                }
            }
        }

        for (int y = 0; y < m_levelHight; y++)
        {
            prevRoomID = -1;

            for (int x = 0; x < m_levelWidth; x++)
            {
                curRoomID = m_mapGrid.GetTile(x, y).GetOwnerID();

                if (curRoomID != -1)
                {
                    if (prevRoomID == -1)
                    {
                        prevRoomID = curRoomID;
                    }
                    else if (prevRoomID != curRoomID)
                    {
                        m_rooms[prevRoomID].AddArc(m_rooms[curRoomID]);
                        m_rooms[curRoomID].AddArc(m_rooms[prevRoomID]);
                        prevRoomID = curRoomID;
                    }
                }
            }
        }

        Debug.Log("Success");
    }

    void CreateCorridors()
    {
        bool[,] visitedGrid = new bool[m_levelWidth, m_levelHight];
        Pair<int, int>[,] previousGrid = new Pair<int, int>[m_levelWidth, m_levelHight];

        List<Pair<int, int>> dirList = new List<Pair<int, int>>();

        dirList.Add(new Pair<int, int>(-1, 0));
        dirList.Add(new Pair<int, int>(0, -1));
        dirList.Add(new Pair<int, int>(1, 0));
        dirList.Add(new Pair<int, int>(0, 1));

        foreach (TileArc arc in m_exitArcs)
        {
            Pair<int, int> startPos = arc.GetStartPos();
            Pair<int, int> destPos = arc.GetTargetPos();

            bool targetTileFound = false;

            for (int x= 0; x < m_levelWidth; x++)
            {
                for (int y = 0; y < m_levelHight; y++)
                {
                    visitedGrid[x, y] = false;
                    previousGrid[x, y] = null;
                }
            }

            List<Pair<int,int>> possibleTileList = new List<Pair<int, int>>();
            possibleTileList.Add(startPos);

            Pair<int, int> curPos = null;

            while (!targetTileFound && possibleTileList.Count != 0)
            {
                curPos = possibleTileList[0];

                if(!targetTileFound)
                {
                    foreach (Pair<int, int> dir in dirList)
                    {
                        Pair<int, int> tempPos = new Pair<int, int>(curPos.m_first + dir.m_first, curPos.m_second + dir.m_second);

                        if (m_mapGrid.GetTile(tempPos.m_first, tempPos.m_second).GetOwnerID() == -1 &&
                            visitedGrid[tempPos.m_first, tempPos.m_second] == false)
                        {
                            visitedGrid[tempPos.m_first, tempPos.m_second] = true;
                            previousGrid[tempPos.m_first, tempPos.m_second] = curPos;
                            possibleTileList.Add(new Pair<int, int>(tempPos.m_first, tempPos.m_second));
                        }
                        else if(tempPos.Equals(destPos))
                        {
                            previousGrid[tempPos.m_first, tempPos.m_second] = curPos;
                            targetTileFound = true;
                            break;
                        }
                    }
                    possibleTileList.Remove(curPos);
                    possibleTileList = possibleTileList.OrderBy(o => TileArc.CalculateWeight(o, destPos)).ToList();
                }
            }

            if(targetTileFound)
            {
                curPos = previousGrid[destPos.m_first, destPos.m_second];

                while(!curPos.Equals(startPos))
                {
                    m_mapGrid.SetTileType(curPos.m_first, curPos.m_second, TileType.Empty);
                    curPos = previousGrid[curPos.m_first, curPos.m_second];
                }
            }               
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

            if(m_arcsMST.Count != 0)
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
