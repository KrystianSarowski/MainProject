using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    string m_name;

    const float m_ROTATION_SPEED = 5.0f;
    const float m_HOVER_SPEED = 0.01f;

    float m_count = 0;

    void FixedUpdate()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(rotation.x, transform.rotation.eulerAngles.y + m_ROTATION_SPEED, rotation.z);

        Vector3 position = transform.position;

        if(m_count < 30)
        {
            position.y -= m_HOVER_SPEED;
        }
        else
        {
            position.y += m_HOVER_SPEED;
        }

        m_count++;

        transform.position = position;

        if(m_count == 60)
        {
            m_count = 0;
        }
    }

    public string GetName()
    {
        return m_name;
    }
}
