using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    RoomLayout m_layout = null;

    //Has the room been cleared by the playeror not.
    bool m_isCleared = false;

    bool m_isLocked = false;

    [SerializeField]
    GameObject m_doorPrefab;

    [SerializeField]
    GameObject m_enemyPrefab;

    List<GameObject> m_doors = new List<GameObject>();
    List<GameObject> m_enemies = new List<GameObject>();

    void Update()
    {
        if(m_isLocked)
        {
            for (var i = m_enemies.Count - 1; i > -1; i--)
            {
                if (m_enemies[i] == null)
                {
                    m_enemies.RemoveAt(i);
                }
            }

            if(m_enemies.Count == 0)
            {
                UnlockRoom();
            }
        }
    }

    public void SetLayout(RoomLayout t_layout)
    {
        m_layout = t_layout;
    }

    public void ImplementLayout(float t_tileSize)
    {
        //The width of the layout minus the surrounding walls.
        int width = m_layout.m_grid.m_width - 2;

        //The height of the layout minus the surrounding walls.
        int height = m_layout.m_grid.m_height - 2;

        GetComponent<BoxCollider>().size = new Vector3(width, 2, height);

        GridIndex indexPos = m_layout.GetNodePositonOnMap();
        
        Vector3 nodePos = new Vector3((indexPos.m_x + 0.5f) * t_tileSize, -1, (indexPos.m_y + 0.5f) * t_tileSize);
        Vector3 position = nodePos;
        
        if(width % 2 == 0)
        {
            position.x -= 0.5f * t_tileSize;
        }

        if (height % 2 == 0)
        {
            position.z -= 0.5f * t_tileSize;
        }

        transform.position = position;

        PlaceDoors(t_tileSize, nodePos);
    }

    void PlaceDoors(float t_tileSize, Vector3 t_nodePos)
    {
        Vector3 position = t_nodePos;

        List<GridIndex> exitIndexPos = m_layout.GetExitsOnMap();

        foreach (GridIndex exitIndex in exitIndexPos)
        {
            position.x = (exitIndex.m_x + 0.5f) * t_tileSize;
            position.z = (exitIndex.m_y + 0.5f) * t_tileSize;

            Vector3 dirVector = t_nodePos - position;
            float angle = Vector3.Angle(dirVector, Vector3.forward);

            GameObject door = Instantiate(m_doorPrefab, position, Quaternion.AngleAxis(angle, Vector3.up));
            door.transform.SetParent(transform);
            door.SetActive(false);

            m_doors.Add(door);
        }
    }

    void UnlockRoom()
    {
        foreach (GameObject door in m_doors)
        {
            door.SetActive(false);
        }

        m_isCleared = true;
        m_isLocked = false;
    }    

    void LockRoom()
    {
        SpawnEnemies();

        foreach(GameObject door in m_doors)
        {
            door.SetActive(true);
        }

        m_isLocked = true;
    }

    void SpawnEnemies()
    {
        Vector3 roomScale = GetComponent<BoxCollider>().size;
        roomScale.x = roomScale.x / 2;
        roomScale.z = roomScale.z / 2;
        roomScale.y = roomScale.y / 2;

        for(int i = 0; i < 3; i++)
        {
            Vector3 spawnPos = transform.position;

            spawnPos.x += Random.Range(-roomScale.x, roomScale.x);
            spawnPos.z += Random.Range(-roomScale.z, roomScale.z);
            spawnPos.y += -roomScale.y + (m_enemyPrefab.transform.localScale.y / 2);

            GameObject enemy = Instantiate(m_enemyPrefab, spawnPos, Quaternion.identity);
            enemy.GetComponent<Enemy>().SetRoomDimensions(transform.position, roomScale);
            //enemy.transform.SetParent(transform);
            m_enemies.Add(enemy);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!m_isCleared && !m_isLocked)
        {
           if(other.tag == "Player")
           {
                LockRoom();
           }
        }
    }
}