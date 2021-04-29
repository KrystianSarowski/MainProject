using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public class HoleExit : MonoBehaviour
{
    GameplayManager m_managerScript;

    bool m_isActive = true;

    // Start is called before the first frame update
    void Start()
    {
        m_managerScript = FindObjectOfType<GameplayManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(m_isActive)
            {
                m_isActive = false;
                m_managerScript.LoadNextLevel();
            }
        }
    }
}