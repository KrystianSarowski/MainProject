using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//@Author Krystian Sarowski

public class BottomUpGenerator
{
    /// <summary>
    /// Places the passed in RoomLayouts on the passed in TileGrid by transfering
    /// the Tiles from the local TileGird of RoomLayout to the correct location the map TileGird.
    /// When a RoomLayout is placed down succesfully it is marked as placed.
    /// The while loop runs until all Roomlayouts have been placed or we tried sufficent enough times.
    /// </summary>
    /// <param name="t_mapGrid">TileGrid on which each of the RoomLayouts are going to be placed in</param>
    /// <param name="t_roomsToPlace">List of RoomLayouts to be placed down in passed in TileGird</param>
    /// <returns></returns>
    public static IEnumerator PlaceRooms(TileGrid t_mapGrid, List<RoomLayout> t_roomsToPlace)
    {
        int roomIndex = 0;
        int safeLockCount = 0;
        int numOfRooms = t_roomsToPlace.Count;
        int maxSafeLock = numOfRooms * 25;

        while (roomIndex < numOfRooms && safeLockCount < maxSafeLock)
        {
            GridIndex position = new GridIndex(GameplayManager.s_seedRandom.Next(0, t_mapGrid.m_width),
               GameplayManager.s_seedRandom.Next(0, t_mapGrid.m_height));

            TileGrid roomGrid = t_roomsToPlace[roomIndex].m_grid;

            if (ValidatePlacement(position, roomGrid.m_width, roomGrid.m_height, t_mapGrid))
            {
                t_roomsToPlace[roomIndex].SetPositionIndex(position);
                t_roomsToPlace[roomIndex].SetID(roomIndex);
                t_roomsToPlace[roomIndex].m_roomAdded = true;

                for (int x = 0; x < roomGrid.m_width; x++)
                {
                    for (int y = 0; y < roomGrid.m_height; y++)
                    {
                        GridIndex posOnMap;

                        posOnMap.m_x = position.m_x + x;
                        posOnMap.m_y = position.m_y + y;

                        t_mapGrid.SetTileType(posOnMap, roomGrid.GetTile(new GridIndex(x, y)).GetTileType());
                        t_mapGrid.GetTile(posOnMap).SetOwnerID(t_roomsToPlace[roomIndex].GetID());
                    }
                }

                roomIndex++;
            }
            safeLockCount++;
        }

        yield return null;
    }

    /// <summary>
    /// Checks if all the tiles starting from the passed in index location for the passed in width
    /// do not have an owner and are indeside the bounds of the passed in TileGird.
    /// </summary>
    /// <param name="t_index">The start index location within the TileGird</param>
    /// <param name="t_roomWidth">Width of the search within the TileGrid</param>
    /// <param name="t_roomHeight">Height of the search within the TileGrid</param>
    /// <param name="t_mapGrid">The TileGrid in which the check occures</param>
    /// <returns>Bool for if the RoomLayout can be placed in this location</returns>
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

    /// <summary>
    /// Creates the TileArcs between each of the RoomLayouts placed within the TileGrid
    /// traveling through th TileGrid horizonatlly then vertically and connecting RoomLayouts
    /// thogetheer by checking if there are no rooms between the individual Tile within each of
    /// the rooms in a straigh line.
    /// </summary>
    /// <param name="t_mapGrid">The TileGrid that contains all the RoomLayouts</param>
    /// <param name="t_rooms">List of RoomLayout to which each of the TileArcs will be added to</param>
    public static void CreateRoomArcs(TileGrid t_mapGrid, List<RoomLayout> t_rooms)
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

    /// <summary>
    /// Creates a Minimum Spanning Tree of TileArcs using the TileArcs from the passed
    /// in RoomLayouts. Each of the RoomLayout becomes connected to atleast to one other
    /// RoomLayout. The Resulting list of TileArcs is then returned.
    /// </summary>
    /// <param name="t_rooms">RoomLayouts contain the TileArcs to use for the MST</param>
    /// <returns>The resulting MST of TileArcs as a list</returns>
    public static List<TileArc> CreateMST(List<RoomLayout> t_rooms)
    {
        foreach (RoomLayout room in t_rooms)
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

    /// <summary>
    /// Moves the passed in list TileArcs from the center node of the RoomLayout to
    /// potential exit tiles by selecting the closes exit Tiles from start and end RoomLayout
    /// in each arc. The Result is stored in the passed in list t_exitArcs.
    /// </summary>
    /// <param name="t_roomArcs">List of TileArcs from the center of each room</param>
    /// <param name="t_exitArcs">Resulting list of TileArcs that have been move to exit Tiles</param>
    /// <param name="t_mapGrid">The TileGrid in which this process occures</param>
    /// <param name="t_rooms">The RoomLayout which contain the TileArcs</param>
    public static void CreateExitArcs(List<TileArc> t_roomArcs, List<TileArc> t_exitArcs, TileGrid t_mapGrid, List<RoomLayout> t_rooms)
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

            exitArc.GetStartRoom().AddExitToLayout(exitArc.GetStartPos());
            exitArc.GetTargetRoom().AddExitToLayout(exitArc.GetTargetPos());
            t_exitArcs.Add(exitArc);
        }

        foreach (RoomLayout room in t_rooms)
        {
            List<GridIndex> exitList = room.GetExitsOnMap();

            foreach (GridIndex exitIndex in exitList)
            {
                t_mapGrid.SetTileType(exitIndex, TileType.Exit);
            }
        }
    }

    /// <summary>
    /// Creates corridors between each of the rooms using the passed in TileArcs.
    /// The corridor path is created using Astar. The Tiles along the path are set
    /// to empty.
    /// </summary>
    /// <param name="t_exitArcs">List of TileArcs used to create corridors between rooms</param>
    /// <param name="t_mapGrid">TileGrid in which this process occures</param>
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
}