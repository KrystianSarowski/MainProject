using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public enum RoomType
{
    Spawn,
    Boss,
    Combat,
    Powerup,
}

public class Room : MonoBehaviour
{
    RoomLayout m_layout = null;

    //Has the room been cleared by the player or not.
    bool m_isCleared = false;
    bool m_isLocked = false;
    bool m_spawnShopkeeper = false;

    int m_powerUpIndex = 0;     //The index of the powerup that has been selected from the possible power ups.
    int m_width = 0;            //The width of the room.
    int m_height = 0;           //The height of the room.

    [SerializeField]
    GameObject m_doorPrefab;

    [SerializeField]
    GameObject m_enemyPrefab;

    [SerializeField]
    GameObject m_bossPrefab;

    [SerializeField]
    GameObject m_shopkeeperPrefab;

    [SerializeField]
    GameObject m_exitPrefab;

    List<GameObject> m_doors = new List<GameObject>();

    [SerializeField]
    public List<GameObject> m_enemies = new List<GameObject>();

    [SerializeField]
    public Vector3 m_size;

    [SerializeField]
    public Vector3 m_position;

    List<Vector3> m_doorPositions = new List<Vector3>();

    [SerializeField]
    RoomType m_roomType = RoomType.Combat;

    [SerializeField]
    List<GameObject> m_pickUpPrefabs;

    [SerializeField]
    GameObject m_wallPrefab;

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

        if(m_roomType == RoomType.Powerup)
        {
            m_powerUpIndex = GameplayManager.s_seedRandom.Next(0, 4);
        }
    }

    public void SetSpawnShopkeeper(bool t_spawnShopkeeper)
    {
        m_spawnShopkeeper = t_spawnShopkeeper;
    }

    public void ImplementLayout(float t_tileSize)
    {
        //The width of the layout minus the surrounding walls.
        m_width = m_layout.m_grid.m_width - 2;

        //The height of the layout minus the surrounding walls.
        m_height = m_layout.m_grid.m_height - 2;

        GetComponent<BoxCollider>().size = new Vector3(m_width, 2, m_height);

        GridIndex indexPos = m_layout.GetNodePositonOnMap();
        
        Vector3 nodePos = new Vector3((indexPos.m_x + 0.5f) * t_tileSize, -1, (indexPos.m_y + 0.5f) * t_tileSize);
        
        m_position = nodePos;
        
        if (m_width % 2 == 0)
        {
            m_position.x -= 0.5f * t_tileSize;
        }

        if (m_height % 2 == 0)
        {
            m_position.z -= 0.5f * t_tileSize;
        }

        transform.position = m_position;

        PlaceDoors(t_tileSize, nodePos);
        PlaceInnerWallColliders();
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

            m_doorPositions.Add(dirVector.normalized + position);

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

        if(m_spawnShopkeeper)
        {
            SpawnShopkeeper();
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
        GameObject pickupPrefab = m_pickUpPrefabs[m_powerUpIndex];
        Instantiate(pickupPrefab, transform.position, pickupPrefab.transform.rotation);
        m_isCleared = true;
    }

    void SpawnShopkeeper()
    {
        bool validPos = false;
        float rotation = 0;
        Vector3 spawnPos = Vector3.zero;

        while (!validPos)
        {
            spawnPos = GenerateWallPosition(m_shopkeeperPrefab.transform.localScale, ref rotation);

            Collider[] objectColliders = Physics.OverlapBox(spawnPos, m_shopkeeperPrefab.transform.localScale * 0.49f);

            if (objectColliders.Length == 1)
            {
                validPos = true;
            }
        }

        GameObject shopkeeper = Instantiate(m_shopkeeperPrefab, spawnPos, m_shopkeeperPrefab.transform.rotation);
        shopkeeper.transform.Rotate(Vector3.up, rotation);
    }

    void PlaceInnerWallColliders()
    {
        GridIndex nodeIndex = m_layout.m_nodePosIndex;

        for (int x = 0; x < m_width; x++)
        {
            for(int y = 0; y < m_height; y++)
            {
                GridIndex indexPos = new GridIndex(x, y);

                if(m_layout.m_grid.GetTile(indexPos).GetTileType() == TileType.InnerWall)
                {
                    Vector3 position = new Vector3(indexPos.m_x - nodeIndex.m_x, 0, indexPos.m_y - nodeIndex.m_y);

                    if (m_width % 2 == 0)
                    {
                        position.x += 0.5f;
                    }

                    if (m_height % 2 == 0)
                    {
                        position.z += 0.5f;
                    }

                    GameObject wall = Instantiate(m_wallPrefab, transform.position + position, m_wallPrefab.transform.rotation);
                    wall.transform.SetParent(transform);
                }
            }
        }
    }

    Vector3 GenerateWallPosition(Vector3 t_objectScale, ref float t_rotation)
    {
        Vector3 pos = transform.position;
        Vector3 offset = Vector3.zero;
        Vector3 generatedPos = Vector3.zero;

        bool valid = false;

        int attemptCount = 0;

        while (!valid && attemptCount < 30)
        {
            valid = true;

            float value1 = Random.Range(-1.0f, 1.0f);
            float value2 = Random.Range(0, 2);

            if (value2 == 0)
            {
                value2 = -1;
            }

            int random = Random.Range(0, 2);

            if (random == 0)
            {
                offset.x = m_size.x * value1;
                offset.z = m_size.z * value2;

                if(value2 == 1)
                {
                    t_rotation = 180;
                }
                else
                {
                    t_rotation = 0;
                }
            }
            else
            {
                offset.x = m_size.x * value2;
                offset.z = m_size.z * value1;

                if (value2 == 1)
                {
                    t_rotation = -90;
                }
                else
                {
                    t_rotation = 90;
                }
            }

            offset.y = -m_size.y + (t_objectScale.y / 2);

            if (offset.x <= 0)
            {
                offset.x += t_objectScale.x / 2;
            }
            else
            {
                offset.x -= t_objectScale.x / 2;
            }

            if (offset.z <= 0)
            {
                offset.z += t_objectScale.z / 2;
            }
            else
            {
                offset.z -= t_objectScale.z / 2;
            }

            generatedPos = pos + offset;

            foreach(Vector3 doorPos in m_doorPositions)
            {
                if((generatedPos - doorPos).magnitude < 1.2f)
                {
                    valid = false;
                }
            }
        }

        return generatedPos;
    }

    void SpawnEnemies()
    {
        int numOfEnemies = (int)(m_size.x * m_size.z / 4.0f);

        for (int i = 0; i < numOfEnemies; i++)
        {
            bool spawned = false;

            while(!spawned)
            {
                Vector3 spawnPos = transform.position;

                spawnPos.x += Random.Range(-m_size.x, m_size.x);
                spawnPos.z += Random.Range(-m_size.z, m_size.z);
                spawnPos.y += -m_size.y + (m_enemyPrefab.transform.localScale.y / 2);

                Collider[] objectColliders = Physics.OverlapBox(spawnPos, m_enemyPrefab.transform.localScale * 0.49f);

                if (objectColliders.Length == 1)
                {
                    GameObject enemy = Instantiate(m_enemyPrefab, spawnPos, Quaternion.identity);
                    enemy.transform.SetParent(transform);
                    m_enemies.Add(enemy);
                    spawned = true;
                }
            }
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