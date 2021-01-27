using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    bool m_animate = false;
    bool m_return = false;

    Quaternion m_defaultRotation;
    float m_rotation = 5.0f;

    void Start()
    {
        m_defaultRotation = transform.localRotation;    
    }

    void FixedUpdate()
    {
        if(m_animate)
        {
            transform.Rotate(Vector3.down, m_rotation);

            if(transform.localRotation.eulerAngles.x > 45.0f && transform.localRotation.eulerAngles.x < 330.0f && m_return)
            {
                m_animate = false;
                m_return = false;
                transform.localRotation = m_defaultRotation;
            }

            if (transform.localRotation.eulerAngles.x < 340.0f && transform.localRotation.eulerAngles.x > 330.0f && !m_return)
            {
                m_rotation = -m_rotation;
                m_return = true;
            }
        }
    }

    public void StartAttack()
    {
        m_animate = true;

        m_rotation = 10.0f;
    }
}
