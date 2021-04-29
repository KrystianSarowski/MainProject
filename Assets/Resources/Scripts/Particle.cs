using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

[System.Serializable]
public struct ParticleData
{
    public float m_timeToLive;
    public float m_rotationSpeed;

    public bool m_changeSizeOverTime;

    public float m_startSize;
    public float m_finalSize;

    public bool m_changeColourOverTime;

    public Color m_startColour;
    public Color m_endColour;

    public bool m_useGravity;
    public bool m_moveWithParent;
}

public class Particle : MonoBehaviour
{
    ParticleData m_data;

    float m_timeAlive = 0.0f;

    Vector3 m_startScale;

    MeshRenderer m_meshRenderer;

    Rigidbody m_rigidbody;

    ParticleSystem m_parentSystem;

    public void SetData(ParticleData t_particleData, ParticleSystem t_parentSystem)
    {
        m_parentSystem = t_parentSystem;
        m_data = t_particleData;
        m_timeAlive = 0.0f;

        Destroy(gameObject, m_data.m_timeToLive);

        m_startScale = transform.localScale;
        transform.localScale = m_startScale * m_data.m_startSize;

        m_meshRenderer = GetComponent<MeshRenderer>();
        m_meshRenderer.material.color = m_data.m_startColour;

        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.useGravity = m_data.m_useGravity;

        if(m_data.m_moveWithParent)
        {
            transform.SetParent(m_parentSystem.gameObject.transform);
        }

        m_parentSystem.AddParticle();
    }

    void OnDestroy()
    {
        if (m_parentSystem != null)
        {
            m_parentSystem.RemoveParticle();
        }
    }

    void Update()
    {
        m_timeAlive += Time.deltaTime;

        if(m_data.m_changeSizeOverTime)
        {
            float scale = Mathf.Lerp(m_data.m_startSize, m_data.m_finalSize, m_timeAlive / m_data.m_timeToLive);

            transform.localScale = m_startScale * scale;
        }

        if(m_data.m_changeColourOverTime)
        {
            Color curColour = Color.Lerp(m_data.m_startColour, m_data.m_endColour, m_timeAlive / m_data.m_timeToLive);

            m_meshRenderer.material.color = curColour;
        }

        if(m_data.m_rotationSpeed != 0)
        {
            transform.Rotate(Random.Range(0, m_data.m_rotationSpeed), Random.Range(0, m_data.m_rotationSpeed), Random.Range(0, m_data.m_rotationSpeed));
        }
    }
}