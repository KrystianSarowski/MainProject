using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleExit : MonoBehaviour
{
    GameplayManager m_managerScript;

    // Start is called before the first frame update
    void Start()
    {
        m_managerScript = FindObjectOfType<GameplayManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            m_managerScript.LoadNextLevel();
        }
    }
}
