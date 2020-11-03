using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public MapGrid m_mapGrid;

    // Start is called before the first frame update
    void Start()
    {
        if(m_mapGrid != null)
        {
            m_mapGrid.CreateMap();
        }
    }

    void OnDrawGizmos()
    {
        if (m_mapGrid != null)
        {
            for (int x = 0; x < m_mapGrid.m_width; x++)
            {
                for (int y = 0; y < m_mapGrid.m_height; y++)
                {
                    Gizmos.color = (m_mapGrid.GetTile(x, y) == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-m_mapGrid.m_width / 2 + x + 0.5f, 0, -m_mapGrid.m_height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
