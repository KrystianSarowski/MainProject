using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridArea
{
    public static int s_padding = 1;

    public int m_width, m_height;
    public int m_childCountLeft, m_childCountRight;
    public GridIndex m_startPos;

    public Room m_room;

    public GridArea m_parent;
    public GridArea[] m_children;

    public GridArea()
    {
        m_parent = null;
        m_children = new GridArea[2];
        m_childCountLeft = 0;
        m_childCountRight = 0;
        m_room = null;
    }

    public bool HasChildrean()
    {
        if (m_children[0] != null && m_children[1] != null)
        {
            return true;
        }

        return false;
    }

    public bool AddRoom(Room t_room)
    {
        if (m_room == null && !HasChildrean())
        {
            if (t_room.m_grid.m_width + s_padding * 2 <= m_width && t_room.m_grid.m_height + s_padding * 2 <= m_height)
            {
                m_room = t_room;
                return true;
            }
        }

        return false;
    }

    void SplitVerticaly(bool t_splitInHalf)
    {
        List<int> widths = new List<int>();

        if (t_splitInHalf)
        {
            widths.Add(m_width / 2);
            widths.Add(m_width / 2);
        }
        else
        {
            widths.Add(m_room.m_grid.m_width + s_padding * 2);
            widths.Add(m_width - widths[0]);
        }

        for (int i = 0; i < 2; i++)
        {
            m_children[i].m_parent = this;
            m_children[i].m_startPos = new GridIndex(m_startPos.m_x + widths[0] * i, m_startPos.m_y);
            m_children[i].m_height = m_height;
            m_children[i].m_width = widths[i];
        }
    }

    void SplitHorizontaly(bool t_splitInHalf)
    {
        List<int> heights = new List<int>();

        if (t_splitInHalf)
        {
            heights.Add(m_height / 2);
            heights.Add(m_height / 2);
        }
        else
        {
            heights.Add(m_room.m_grid.m_height + s_padding * 2);
            heights.Add(m_height - heights[0]);
        }

        for (int i = 0; i < 2; i++)
        {
            m_children[i].m_parent = this;
            m_children[i].m_startPos = new GridIndex(m_startPos.m_x, m_startPos.m_y + heights[0] * i);
            m_children[i].m_width = m_width;
            m_children[i].m_height = heights[i];
        }
    }

    bool ValidateSplitVarticaly(Room t_room)
    {
        if (m_room.m_grid.m_width + t_room.m_grid.m_width + s_padding * 4 <= m_width)
        {
            if (m_height >= t_room.m_grid.m_height + s_padding * 2)
            {
                return true;
            }
        }

        return false;
    }

    bool ValidateSplitHorizontaly(Room t_room)
    {
        if (m_room.m_grid.m_height + t_room.m_grid.m_height + s_padding * 4 <= m_height)
        {
            if (m_width >= t_room.m_grid.m_width + s_padding * 2)
            {
                return true;
            } 
        }
        return false;
    }

    public bool SplitAddRoom(Room t_room)
    {
        bool canSplitVertical = false;
        bool canSplitHorizontal = false;

        if(m_width >= m_height)
        {
            if(ValidateSplitVarticaly(t_room))
            {
                canSplitVertical = true;
            }
            else if (ValidateSplitHorizontaly(t_room))
            {
                canSplitHorizontal = true;
            }
        }

        else
        {
            if (ValidateSplitHorizontaly(t_room))
            {
                canSplitHorizontal = true;
            }
            else if (ValidateSplitVarticaly(t_room))
            {
                canSplitVertical = true;
            }
        }

        if(canSplitVertical)
        {
            m_children[0] = new GridArea();
            m_children[1] = new GridArea();

            if (m_width / 2 >= t_room.m_grid.m_width + s_padding * 2
                && m_width / 2 >= m_room.m_grid.m_width + s_padding * 2)
            {
                SplitVerticaly(true);
            }

            else
            {
                SplitVerticaly(false);
            }
        }

        else if(canSplitHorizontal)
        {
            m_children[0] = new GridArea();
            m_children[1] = new GridArea();

            if (m_height / 2 >= t_room.m_grid.m_height + s_padding * 2
                && m_height / 2 >= m_room.m_grid.m_height + s_padding * 2)
            {
                SplitHorizontaly(true);
            }

            else
            {
                SplitHorizontaly(false);
            }
        }

        if(canSplitHorizontal || canSplitVertical)
        {
            m_children[0].m_room = m_room;
            m_children[1].m_room = t_room;
            UpdateChildCount();
            m_room = null;
            return true;
        }

        return false;
    }

    void UpdateChildCount()
    {
        m_childCountLeft = m_children[0].m_childCountLeft + m_children[0].m_childCountRight + 1;
        m_childCountRight = m_children[1].m_childCountLeft + m_children[1].m_childCountRight + 1;

        if (m_parent != null)
        {
            m_parent.UpdateChildCount();
        }
    }
}

public class TopDownGenerator : MonoBehaviour
{
    public static IEnumerator PlaceRooms(GridArea t_root, TileGrid t_mapGrid, List<Room> t_roomsToPlace)
    {
        t_root.m_startPos = new GridIndex(0, 0);
        t_root.m_width = t_mapGrid.m_width;
        t_root.m_height = t_mapGrid.m_height;

        int index = 0;
        bool noMoreSpace = false;

        while(index < t_roomsToPlace.Count && !noMoreSpace)
        {
            if(!PlaceRoom(t_root, t_roomsToPlace[index]))
            {
                noMoreSpace = true;
            }

            else
            {
                t_roomsToPlace[index].SetRoomID(index);
                index++;
            }
        }

        ConstructRoom(t_root, t_mapGrid);

        yield return null;
    }

    static bool PlaceRoom(GridArea t_current, Room t_room)
    {
        if(t_current.AddRoom(t_room))
        {
            return true;
        }

        else
        {
            if(t_current.HasChildrean())
            {
                if (t_current.m_childCountLeft <= t_current.m_childCountRight)
                {
                    if (PlaceRoom(t_current.m_children[0], t_room))
                    {
                        return true;
                    }

                    else if(PlaceRoom(t_current.m_children[1], t_room))
                    {
                        return true;
                    }
                }

                else
                {
                    if (PlaceRoom(t_current.m_children[1], t_room))
                    {
                        return true;
                    }

                    else if (PlaceRoom(t_current.m_children[0], t_room))
                    {
                        return true;
                    }
                }
            }

            else
            {
                if(t_current.SplitAddRoom(t_room))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static void ConstructRoom(GridArea t_current, TileGrid t_mapGrid)
    {
        if (t_current.m_room != null)
        {
            TileGrid roomGrid = t_current.m_room.m_grid;

            t_current.m_room.SetPositionIndex(new GridIndex(t_current.m_startPos.m_x + t_current.m_width / 2 - roomGrid.m_width / 2,
                t_current.m_startPos.m_y + t_current.m_height / 2 - roomGrid.m_height / 2));

            for (int x = 0; x < roomGrid.m_width; x++)
            {
                GridIndex posOnMap = t_current.m_room.GetPositionIndex();

                posOnMap.m_x = posOnMap.m_x + x;

                for (int y = 0; y < roomGrid.m_height; y++)
                {
                    t_mapGrid.SetTileType(posOnMap, roomGrid.GetTile(new GridIndex(x, y)).GetTileType());
                    t_mapGrid.GetTile(posOnMap).SetOwnerID(t_current.m_room.GetRoomID());

                    posOnMap.m_y += 1;
                }
            }
        }

        if(t_current.HasChildrean())
        {
            for (int i = 0; i < 2; i++)
            {
                ConstructRoom(t_current.m_children[i], t_mapGrid);
            }
        }
    }

    public static List<Room> ConnectRooms(GridArea t_current, List<TileArc> t_nodeArcs)
    {
        List<Room> rooms = new List<Room>();

        if (t_current.HasChildrean())
        {
            List<Room> leftRooms = ConnectRooms(t_current.m_children[0], t_nodeArcs);
            List<Room> rightRooms = ConnectRooms(t_current.m_children[1], t_nodeArcs);

            TileArc tileArc = new TileArc();

            for(int i = 0; i < leftRooms.Count; i++)
            {
                for(int j = 0; j < rightRooms.Count; j++)
                {
                    TileArc newTileArc = new TileArc(leftRooms[i], rightRooms[j]);

                    if(newTileArc.GetWeigtht() < tileArc.GetWeigtht())
                    {
                        tileArc = newTileArc;
                    }
                }
            }

            t_nodeArcs.Add(tileArc);
            rooms.AddRange(leftRooms);
            rooms.AddRange(rightRooms);
        }

        else
        {
            rooms.Add(t_current.m_room);
        }

        return rooms;
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
}

