using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystem : MonoBehaviour
{
    [SerializeField]
    GameObject m_pariclePrefab;

    [SerializeField]
    float m_initialSpeed;

    [SerializeField]
    float m_startLifeTime;

    [SerializeField]
    float m_particleDelay;

    [SerializeField]
    int m_maxParticleCount;

    [SerializeField]
    bool m_isContinues;

    [SerializeField]
    bool m_isActive;

    int m_particleCount = 0;

    void Awake()
    {
        if(m_isActive)
        {
            StartSystem();
        }
    }

    void StartSystem()
    {
        if(!m_isContinues)
        {
            for (int i = 0; i < m_maxParticleCount; i++)
            {
                Vector3 direction = new Vector3(Random.Range(-1.0F, 1.0F), Random.Range(-1.0F, 1.0F), Random.Range(-1.0F, 1.0F));
                direction.Normalize();

                FireParticle(direction, transform.position);
            }
        }
    }

    void FireParticle(Vector3 t_direction, Vector3 t_position)
    {
        GameObject particle = Instantiate(m_pariclePrefab, t_position, m_pariclePrefab.transform.rotation);

        Destroy(particle, m_startLifeTime);

        particle.GetComponent<Rigidbody>().AddForce(t_direction * m_initialSpeed, ForceMode.Impulse);

        m_particleCount++;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_isActive && m_isContinues)
        {

        }
    }
}
