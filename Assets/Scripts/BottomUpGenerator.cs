using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BottomUpGenerator
{
    public static IEnumerator PlaceRooms(TileGrid t_mapGrid, List<Room> t_roomsToPlace)
    {
        int roomIndex = 0;
        int safeLockCount = 0;
        int numOfRooms = t_roomsToPlace.Count;

        while (roomIndex < numOfRooms && safeLockCount < 400)
        {
            Pair<int, int> position = new Pair<int, int>(GameplayManager.s_seedRandom.Next(0, t_mapGrid.m_width),
               GameplayManager.s_seedRandom.Next(0, t_mapGrid.m_height));

            TileGrid roomGrid = t_roomsToPlace[roomIndex].m_roomGrid;

            if (ValidatePlacement(position, roomGrid.m_width, roomGrid.m_height, t_mapGrid))
            {
                for (int x = 0; x < roomGrid.m_width; x++)
                {
                    for (int y = 0; y < roomGrid.m_height; y++)
                    {
                        int posOnMapX = position.m_first + x;
                        int posOnMapY = position.m_second + y;

                        t_mapGrid.SetTileType(posOnMapX, posOnMapY, roomGrid.GetTile(x, y).GetTileType());
                        t_mapGrid.GetTile(posOnMapX, posOnMapY).SetOwnerID(t_roomsToPlace[roomIndex].GetRoomID());
                    }
                }

                t_roomsToPlace[roomIndex].SetPositionIndex(position);
                roomIndex++;

                yield return new WaitForSeconds(0.1f);
            }
            safeLockCount++;
        }

        yield return new WaitForSeconds(0.01f);
    }

    public static bool ValidatePlacement(Pair<int, int> t_index, int t_roomWidth, int t_roomHeight, TileGrid t_mapGrid)
    {
        bool canBePlaced = true;

        //Check each tile within the room space aswell as 1 tile layer
        //around the bounds of the room to make sure it is surrounded by walls.
        for (int x = -1; x < t_roomWidth + 1; x++)
        {
            for (int y = -1; y < t_roomHeight + 1; y++)
            {
                if (t_index.m_first + x < 0 || t_index.m_first + x >= t_mapGrid.m_width
                    || t_index.m_second + y < 0 || t_index.m_second + y >= t_mapGrid.m_height)
                {
                    canBePlaced = false;
                    return canBePlaced;
                }
                else if (t_mapGrid.GetTile(t_index.m_first + x, t_index.m_second + y).GetOwnerID() != -1)
                {
                    canBePlaced = false;
                    return canBePlaced;
                }
            }
        }

        return canBePlaced;
    }

    public static void CreateRoomArcs(TileGrid t_mapGrid, List<Room> t_rooms)
    {
        int curRoomID = -1;
        int prevRoomID = -1;

        for (int x = 0; x < t_mapGrid.m_width; x++)
        {
            prevRoomID = -1;

            for (int y = 0; y < t_mapGrid.m_height; y++)
            {
                curRoomID = t_mapGrid.GetTile(x, y).GetOwnerID();

                if (curRoomID != -1)
                {
                    if (prevRoomID == -1)
                    {
                        prevRoomID = curRoomID;
                    }
                    else if (prevRoomID != curRoomID)
                    {
                        t_rooms[prevRoomID].AddArc(t_rooms[curRoomID]);
                        t_rooms[curRoomID].AddArc(t_rooms[prevRoomID]);
                        prevRoomID = curRoomID;
                    }
                }
            }
        }

        for (int y = 0; y < t_mapGrid.m_height; y++)
        {
            prevRoomID = -1;

            for (int x = 0; x < t_mapGrid.m_width; x++)
            {
                curRoomID = t_mapGrid.GetTile(x, y).GetOwnerID();

                if (curRoomID != -1)
                {
                    if (prevRoomID == -1)
                    {
                        prevRoomID = curRoomID;
                    }
                    else if (prevRoomID != curRoomID)
                    {
                        t_rooms[prevRoomID].AddArc(t_rooms[curRoomID]);
                        t_rooms[curRoomID].AddArc(t_rooms[prevRoomID]);
                        prevRoomID = curRoomID;
                    }
                }
            }
        }
    }

    public static void CreateExitArcs(List<TileArc> t_roomArcs, List<TileArc> t_exitArcs, TileGrid t_mapGrid, List<Room> t_rooms)
    {
        List<Pair<int, int>> possibleExits1;
        List<Pair<int, int>> possibleExits2;

        foreach (TileArc arc in t_roomArcs)
        {
            TileArc exitArc = arc;

            possibleExits1 = arc.GetStartRoom().GetPossibleExitsOnMap();
            possibleExits2 = arc.GetTargetRoom().GetPossibleExitsOnMap();

            foreach (Pair<int, int> possibleExit1 in possibleExits1)
            {
                foreach (Pair<int, int> possibleExit2 in possibleExits2)
                {
                    if (exitArc.GetWeigtht() > TileArc.CalculateWeight(possibleExit1, possibleExit2))
                    {
                        exitArc.SetStartPos(possibleExit1);
                        exitArc.SetTargetPos(possibleExit2);
                    }
                }
            }

            exitArc.GetStartRoom().AddExitToRoom(exitArc.GetStartPos());
            exitArc.GetTargetRoom().AddExitToRoom(exitArc.GetTargetPos());
            t_exitArcs.Add(exitArc);
        }

        foreach (Room room in t_rooms)
        {
            List<Pair<int, int>> exitList = room.GetExitsOnMap();

            foreach (Pair<int, int> exitIndex in exitList)
            {
                t_mapGrid.SetTileType(exitIndex.m_first, exitIndex.m_second, TileType.Exit);
            }
        }
    }

    public static void CreateCorridors(List<TileArc> t_exitArcs, TileGrid t_mapGrid)
    {
        bool[,] visitedGrid = new bool[t_mapGrid.m_width, t_mapGrid.m_height];
        Pair<int, int>[,] previousGrid = new Pair<int, int>[t_mapGrid.m_width, t_mapGrid.m_height];

        List<Pair<int, int>> dirList = new List<Pair<int, int>>();

        dirList.Add(new Pair<int, int>(-1, 0));
        dirList.Add(new Pair<int, int>(0, -1));
        dirList.Add(new Pair<int, int>(1, 0));
        dirList.Add(new Pair<int, int>(0, 1));

        foreach (TileArc arc in t_exitArcs)
        {
            Pair<int, int> startPos = arc.GetStartPos();
            Pair<int, int> destPos = arc.GetTargetPos();

            bool targetTileFound = false;

            for (int x = 0; x < t_mapGrid.m_width; x++)
            {
                for (int y = 0; y < t_mapGrid.m_height; y++)
                {
                    visitedGrid[x, y] = false;
                    previousGrid[x, y] = null;
                }
            }

            List<Pair<int, int>> possibleTileList = new List<Pair<int, int>>();
            possibleTileList.Add(startPos);

            Pair<int, int> curPos = null;

            while (!targetTileFound && possibleTileList.Count != 0)
            {
                curPos = possibleTileList[0];

                if (!targetTileFound)
                {
                    foreach (Pair<int, int> dir in dirList)
                    {
                        Pair<int, int> tempPos = new Pair<int, int>(curPos.m_first + dir.m_first, curPos.m_second + dir.m_second);

                        if (t_mapGrid.GetTile(tempPos.m_first, tempPos.m_second).GetOwnerID() == -1 &&
                            visitedGrid[tempPos.m_first, tempPos.m_second] == false)
                        {
                            visitedGrid[tempPos.m_first, tempPos.m_second] = true;
                            previousGrid[tempPos.m_first, tempPos.m_second] = curPos;
                            possibleTileList.Add(new Pair<int, int>(tempPos.m_first, tempPos.m_second));
                        }
                        else if (tempPos.Equals(destPos))
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

            if (targetTileFound)
            {
                curPos = previousGrid[destPos.m_first, destPos.m_second];

                while (!curPos.Equals(startPos))
                {
                    t_mapGrid.SetTileType(curPos.m_first, curPos.m_second, TileType.Empty);
                    curPos = previousGrid[curPos.m_first, curPos.m_second];
                }
            }
        }
    }

    public static List<TileArc> CreateMST(List<Room> t_rooms)
    {
        foreach (Room room in t_rooms)
        {
            room.SetIsVisited(false);
        }

        List<TileArc> arclist = new List<TileArc>();
        List<TileArc> treeArcs = new List<TileArc>();

        foreach (TileArc arc in t_rooms[0].m_nodeArcs)
        {
            arclist.Add(arc);
        }

        arclist = arclist.OrderBy(o => o.GetWeigtht()).ToList();

        t_rooms[0].SetIsVisited(true);

        TileArc curArc;

        while (arclist.Count() != 0)
        {
            curArc = arclist[0];

            if (curArc.GetTargetRoom().GetIsVisited() != true)
            {
                treeArcs.Add(curArc);
                curArc.GetTargetRoom().SetIsVisited(true);

                foreach (TileArc arc in curArc.GetTargetRoom().m_nodeArcs)
                {
                    arclist.Add(arc);
                }

                arclist = arclist.OrderBy(o => o.GetWeigtht()).ToList();
            }

            else
            {
                arclist.Remove(curArc);
            }
        }

        return treeArcs;
    }
}
