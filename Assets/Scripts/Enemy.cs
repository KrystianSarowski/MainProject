using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{ 
    Idle,
    Wandering,
    Chasing,
    Attack,
    Attacking
}

public class Enemy : MonoBehaviour
{
    const float m_DETECT_RANGE = 3.0f;
    const float m_ATTACK_RANGE = 1.0f;

    Rigidbody m_rb;
    EnemyState m_state;
    Transform m_player;

    Vector3 m_acceleration = new Vector3();
    Vector3 m_targetPos;

    Vector3 m_roomCentre;
    Vector3 m_roomSize;

    float m_accelForce = 13.0f;

    int m_health = 30;

    // Start is called before the first frame update
    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();

        m_player = GameObject.FindGameObjectWithTag("Player").transform;

        m_state = EnemyState.Idle;

        GenerateNewTarget();
    }

    public void SetRoomDimensions(Vector3 t_roomCentre, Vector3 t_roomSize)
    {
        m_roomCentre = t_roomCentre;
        m_roomSize = t_roomSize;

        m_state = EnemyState.Wandering;

        GenerateNewTarget();
    }

    void FixedUpdate()
    {
        m_rb.AddForce(m_acceleration * m_accelForce, ForceMode.Acceleration);

        switch (m_state)
        {
            case EnemyState.Wandering:
                transform.LookAt(m_targetPos);
                m_acceleration = transform.rotation * Vector3.forward;
                m_acceleration.y = 0;
                DetectPlayer();
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                StartCoroutine(Attack());
                break;
            case EnemyState.Attacking:
                break;
            default:
                break;
        }
    }

    IEnumerator Attack()
    {
        m_state = EnemyState.Attacking;

        m_acceleration = Vector3.zero;

        m_player.gameObject.GetComponent<PlayerController>().TakeDamage(5);

        yield return new WaitForSeconds(1.0f);

        if ((transform.position - m_player.position).magnitude < m_ATTACK_RANGE)
        {
            m_state = EnemyState.Attack;
        }
        else
        {
            m_state = EnemyState.Chasing;
        }
    }

    void DetectPlayer()
    {
        if((transform.position - m_player.position).magnitude < m_ATTACK_RANGE)
        {
            m_state = EnemyState.Attack;
        }

        else if((transform.position - m_player.position).magnitude < m_DETECT_RANGE)
        {
            m_state = EnemyState.Chasing;
        }

        else if((transform.position - m_targetPos).magnitude < 0.5f)
        {
            GenerateNewTarget();
        }
    }

    void ChasePlayer()
    {
        transform.LookAt(new Vector3(m_player.position.x, transform.position.y, m_player.position.z));

        m_acceleration = transform.rotation * Vector3.forward;
        m_acceleration.y = 0;

        DetectPlayer();
    }

    void GenerateNewTarget()
    {
        m_targetPos = m_roomCentre;
        m_targetPos.y = transform.position.y;
        m_targetPos.x += Random.Range(-m_roomSize.x + 0.5f, m_roomSize.x - 0.5f);
        m_targetPos.z += Random.Range(-m_roomSize.z + 0.5f, m_roomSize.z - 0.5f);
    }

    public void TakeDamage(int t_incomingDamage)
    {
        m_health -= t_incomingDamage;

        if(m_health < 0)
        {
            Destroy(gameObject);
        }
    }

    public void PushBack(Vector3 t_sourcePos)
    {
        Vector3 pushBackDir = (t_sourcePos - transform.position).normalized;
        pushBackDir.x = -(pushBackDir.x);
        pushBackDir.z = -(pushBackDir.z);

        m_rb.AddForce(pushBackDir * 10, ForceMode.Impulse);
    }
}
