using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    RoomLayout m_layout = null;

    //Has the room been cleared by the playeror not.
    bool m_isCleared = false;

    [SerializeField]
    GameObject m_doorPrefab;

    List<GameObject> m_doors = new List<GameObject>();

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

        transform.localScale = new Vector3(width, 1, height);

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

    IEnumerator LockRoom()
    {
        foreach(GameObject door in m_doors)
        {
            door.SetActive(true);
        }

        yield return new WaitForSeconds(5.0f);

        foreach (GameObject door in m_doors)
        {
            door.SetActive(false);
        }

        m_isCleared = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!m_isCleared)
        {
           if(other.tag == "Player")
           {
                StartCoroutine(LockRoom());
           }
        }
    }
}