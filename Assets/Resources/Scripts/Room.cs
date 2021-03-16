using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Spawn,
    Boss,
    Combat,
    Powerup,
    Empty
}

public class Room : MonoBehaviour
{
    RoomLayout m_layout = null;

    //Has the room been cleared by the player or not.
    bool m_isCleared = false;
    bool m_isLocked = false;

    [SerializeField]
    GameObject m_doorPrefab;

    [SerializeField]
    GameObject m_enemyPrefab;

    [SerializeField]
    GameObject m_bossPrefab;

    [SerializeField]
    GameObject m_exitPrefab;

    List<GameObject> m_doors = new List<GameObject>();

    [SerializeField]
    public List<GameObject> m_enemies = new List<GameObject>();

    [SerializeField]
    public Vector3 m_size;

    [SerializeField]
    public Vector3 m_position;

    [SerializeField]
    RoomType m_roomType = RoomType.Empty;

    [SerializeField]
    List<GameObject> m_pickUpPrefabs;

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

    public void SetRoomSize()
    {
        Vector3 roomScale = GetComponent<BoxCollider>().size;

        roomScale.x = roomScale.x / 2;
        roomScale.z = roomScale.z / 2;
        roomScale.y = roomScale.y / 2;

        m_size = roomScale;
    }

    public void SetRoomType(RoomType t_roomType)
    {
        m_roomType = t_roomType;
    }

    public void SetRandomRoomType()
    {
        int random = Random.Range(0, 100);

        if(random < 70)
        {
            m_roomType = RoomType.Combat;
        }
        else if( random < 90)
        {
            m_roomType = RoomType.Powerup;
        }
        else
        {
            m_roomType = RoomType.Empty;
        }
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
        
        m_position = nodePos;
        
        if(width % 2 == 0)
        {
            m_position.x -= 0.5f * t_tileSize;
        }

        if (height % 2 == 0)
        {
            m_position.z -= 0.5f * t_tileSize;
        }

        transform.position = m_position;

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

        if(m_roomType == RoomType.Boss)
        {
            Vector3 spawnPosition = m_position;
            spawnPosition.y = -1.9005f;

            GameObject exitObject = Instantiate(m_exitPrefab, spawnPosition, Quaternion.identity);

            exitObject.transform.Rotate(Vector3.right * -90);
        }

        m_isCleared = true;
        m_isLocked = false;
    }    

    void LockRoom()
    {
        if(m_roomType == RoomType.Boss)
        {
            SpawnBoss();
        }
        else
        {
            SpawnEnemies();
        }

        foreach(GameObject door in m_doors)
        {
            door.SetActive(true);
        }

        m_isLocked = true;
    }

    void CreatePowerUp()
    {
        GameObject pickupPrefab = m_pickUpPrefabs[Random.Range(0, 2)];

        Instantiate(pickupPrefab, transform.position, pickupPrefab.transform.rotation);

        m_isCleared = true;
    }

    void SpawnEnemies()
    {
        int numOfEnemies = (int)(m_size.x * m_size.z / 7.5f);

        for (int i = 0; i < numOfEnemies; i++)
        {
            Vector3 spawnPos = transform.position;

            spawnPos.x += Random.Range(-m_size.x, m_size.x);
            spawnPos.z += Random.Range(-m_size.z, m_size.z);
            spawnPos.y += -m_size.y + (m_enemyPrefab.transform.localScale.y / 2);

            GameObject enemy = Instantiate(m_enemyPrefab, spawnPos, Quaternion.identity);
            enemy.GetComponent<Enemy>().SetRoom(this);
            enemy.transform.SetParent(transform);
            m_enemies.Add(enemy);
        }
    }

    void SpawnBoss()
    {
        Vector3 spawnPos = transform.position;

        GameObject boss = Instantiate(m_bossPrefab, spawnPos, m_bossPrefab.transform.rotation);
        boss.GetComponent<Boss>().Initialize(this, GameObject.FindGameObjectWithTag("Player").transform);
        boss.transform.SetParent(transform);
        m_enemies.Add(boss);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isCleared && !m_isLocked)
        {
            if (other.tag == "Player")
            {
                if (m_roomType == RoomType.Combat || m_roomType == RoomType.Boss)
                {
                    LockRoom();
                }
                else if (m_roomType == RoomType.Powerup)
                {
                    CreatePowerUp();
                }
            }
        }
    }
}