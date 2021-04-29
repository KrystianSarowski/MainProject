using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski
public class Projectal : MonoBehaviour
{
    [SerializeField]
    float m_speed = 2.0f;

    [SerializeField]
    float m_timeToLive = 5.0f;

    [SerializeField]
    float m_damageFallOffRange = 4.0f;

    [SerializeField]
    float m_damageAreaRange = 1.0f;

    [SerializeField]
    int m_damageFallOff = 1;

    [SerializeField]
    int m_baseMinDamage = 2;

    [SerializeField]
    int m_baseDamage = 10;

    int m_minDamage;
    int m_startDamage;
    int m_damage;

    [SerializeField]
    bool m_isAreaEffect = false;

    Vector3 m_position;

    Rigidbody m_rigidBody;

    [SerializeField]
    GameObject m_explosionPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody>();

        Destroy(gameObject, m_timeToLive);

        m_position = transform.position;
    }

    public void Fire(Vector3 t_direction, float t_damageMultiplier)
    {
        m_minDamage = (int)(m_baseMinDamage * t_damageMultiplier);
        m_startDamage = (int)(m_baseDamage * t_damageMultiplier);

        m_damage = m_startDamage;

        t_direction.Normalize();

        transform.LookAt(transform.position + t_direction);

        m_rigidBody.AddForce(t_direction * m_speed, ForceMode.Impulse);
    }

    void Update()
    {
        if((m_position - transform.position).magnitude > m_damageFallOffRange)
        {
            float distPastFallOff = (m_position - transform.position).magnitude - m_damageFallOffRange;

            m_damage = m_startDamage - (int)(distPastFallOff * m_damageFallOff);

            if(m_damage < m_minDamage)
            {
                m_damage = m_minDamage;
            }
        }
    }

    void CreateExplosion()
    {
        Collider[] colliders =  Physics.OverlapSphere(transform.position, m_damageAreaRange);

        foreach(Collider collider in colliders)
        {
            DealDamage(collider.gameObject);
        }
    }

    void DealDamage(GameObject t_object)
    {
        switch(t_object.tag)
        {
            case "Enemy":
                t_object.GetComponent<Enemy>().TakeDamage(m_damage);
                break;
            case "Player":
                PlayerStats.DealDamage(m_damage);
                break;
            case "Boss":
                t_object.GetComponent<Boss>().TakeDamage(m_damage);
                break;
            default:
                break;
        }
    }

    void OnCollisionEnter(Collision t_collision)
    {
        if (m_isAreaEffect)
        {
            CreateExplosion();
        }
        else
        {
            DealDamage(t_collision.gameObject);
        }

        if(m_explosionPrefab != null)
        {
            Instantiate(m_explosionPrefab, transform.position, m_explosionPrefab.transform.rotation);
        }

        Destroy(gameObject);
    }
}