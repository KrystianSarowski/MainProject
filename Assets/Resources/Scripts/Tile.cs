using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public enum TileType
{
    Node,
    Empty,
    InnerWall,
    Wall,
    Exit
}

public class Tile
{
    private int m_tileID = -1;
    private int m_ownerID = -1;

    private GridIndex m_pos;

    private TileType m_type = TileType.Wall;

    public void SetTileID(int t_tileID)
    {
        m_tileID = t_tileID;
    }

    public void SetOwnerID(int t_ownerID)
    {
        m_ownerID = t_ownerID;
    }

    public void SetTileType(TileType t_tileType)
    {
        m_type = t_tileType;
    }

    public void SetPosition(GridIndex t_pos)
    {
        m_pos = t_pos;
    }

    public int GetTileID()
    {
        return m_tileID;
    }

    public int GetOwnerID()
    {
        return m_ownerID;
    }

    public TileType GetTileType()
    {
        return m_type;
    }

    public GridIndex GetPosition()
    {
        return m_pos;
    }

}