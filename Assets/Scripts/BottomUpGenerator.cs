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
            GridIndex position = new GridIndex(GameplayManager.s_seedRandom.Next(0, t_mapGrid.m_width),
               GameplayManager.s_seedRandom.Next(0, t_mapGrid.m_height));

            TileGrid roomGrid = t_roomsToPlace[roomIndex].m_grid;

            if (ValidatePlacement(position, roomGrid.m_width, roomGrid.m_height, t_mapGrid))
            {
                t_roomsToPlace[roomIndex].SetPositionIndex(position);
                t_roomsToPlace[roomIndex].SetRoomID(roomIndex);

                for (int x = 0; x < roomGrid.m_width; x++)
                {
                    for (int y = 0; y < roomGrid.m_height; y++)
                    {
                        GridIndex posOnMap;

                        posOnMap.m_x = position.m_x + x;
                        posOnMap.m_y = position.m_y + y;

                        t_mapGrid.SetTileType(posOnMap, roomGrid.GetTile(new GridIndex(x, y)).GetTileType());
                        t_mapGrid.GetTile(posOnMap).SetOwnerID(t_roomsToPlace[roomIndex].GetRoomID());
                    }
                }

                roomIndex++;
                yield return null;
            }
            safeLockCount++;
        }

        yield return null;
    }

    public static bool ValidatePlacement(GridIndex t_index, int t_roomWidth, int t_roomHeight, TileGrid t_mapGrid)
    {
        bool canBePlaced = true;

        //Check each tile within the room space aswell as 1 tile layer
        //around the bounds of the room to make sure it is surrounded by walls.
        for (int x = -1; x < t_roomWidth + 1; x++)
        {
            for (int y = -1; y < t_roomHeight + 1; y++)
            {
                if (t_index.m_x + x < 0 || t_index.m_x + x >= t_mapGrid.m_width
                    || t_index.m_y + y < 0 || t_index.m_y + y >= t_mapGrid.m_height)
                {
                    canBePlaced = false;
                    return canBePlaced;
                }
                else if (t_mapGrid.GetTile(new GridIndex(t_index.m_x + x, t_index.m_y + y)).GetOwnerID() != -1)
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
        int curRoomID;
        int prevRoomID;

        for (int x = 0; x < t_mapGrid.m_width; x++)
        {
            prevRoomID = -1;

            for (int y = 0; y < t_mapGrid.m_height; y++)
            {
                curRoomID = t_mapGrid.GetTile(new GridIndex(x, y)).GetOwnerID();

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
                curRoomID = t_mapGrid.GetTile(new GridIndex(x, y)).GetOwnerID();

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
        List<GridIndex> possibleExits1;
        List<GridIndex> possibleExits2;

        foreach (TileArc arc in t_roomArcs)
        {
            TileArc exitArc = arc;

            possibleExits1 = arc.GetStartRoom().GetPossibleExitsOnMap();
            possibleExits2 = arc.GetTargetRoom().GetPossibleExitsOnMap();

            foreach (GridIndex possibleExit1 in possibleExits1)
            {
                foreach (GridIndex possibleExit2 in possibleExits2)
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
            List<GridIndex> exitList = room.GetExitsOnMap();

            foreach (GridIndex exitIndex in exitList)
            {
                t_mapGrid.SetTileType(exitIndex, TileType.Exit);
            }
        }
    }

    public static void CreateCorridors(List<TileArc> t_exitArcs, TileGrid t_mapGrid)
    {
        bool[,] visitedGrid = new bool[t_mapGrid.m_width, t_mapGrid.m_height];
        GridIndex[,] previousGrid = new GridIndex[t_mapGrid.m_width, t_mapGrid.m_height];

        List<GridIndex> dirList = new List<GridIndex>();

        dirList.Add(new GridIndex(-1, 0));
        dirList.Add(new GridIndex(0, -1));
        dirList.Add(new GridIndex(1, 0));
        dirList.Add(new GridIndex(0, 1));

        foreach (TileArc arc in t_exitArcs)
        {
            GridIndex startPos = arc.GetStartPos();
            GridIndex destPos = arc.GetTargetPos();

            bool targetTileFound = false;

            for (int x = 0; x < t_mapGrid.m_width; x++)
            {
                for (int y = 0; y < t_mapGrid.m_height; y++)
                {
                    visitedGrid[x, y] = false;
                    previousGrid[x, y] = new GridIndex(-1, -1);
                }
            }

            List<GridIndex> possibleTileList = new List<GridIndex>();
            possibleTileList.Add(startPos);

            GridIndex curPos;

            while (!targetTileFound && possibleTileList.Count != 0)
            {
                curPos = possibleTileList[0];

                if (!targetTileFound)
                {
                    foreach (GridIndex dir in dirList)
                    {
                        GridIndex tempPos = new GridIndex(curPos.m_x + dir.m_x, curPos.m_y + dir.m_y);

                        if (t_mapGrid.GetTile(tempPos).GetOwnerID() == -1 &&
                            visitedGrid[tempPos.m_x, tempPos.m_y] == false)
                        {
                            visitedGrid[tempPos.m_x, tempPos.m_y] = true;
                            previousGrid[tempPos.m_x, tempPos.m_y] = curPos;
                            possibleTileList.Add(new GridIndex(tempPos.m_x, tempPos.m_y));
                        }
                        else if (tempPos.Equals(destPos))
                        {
                            previousGrid[tempPos.m_x, tempPos.m_y] = curPos;
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
                curPos = previousGrid[destPos.m_x, destPos.m_y];

                while (!curPos.Equals(startPos))
                {
                    t_mapGrid.SetTileType(curPos, TileType.Empty);
                    curPos = previousGrid[curPos.m_x, curPos.m_y];
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