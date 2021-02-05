using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{ 
    Idle,
    Pushback,
    Wandering,
    Chasing,
    Attack,
    Attacking,
    TurnAround
}

public class Enemy : MonoBehaviour
{
    const float m_DETECT_RANGE = 3.0f;
    const float m_ATTACK_RANGE = 1.0f;
    const float m_WANDER_OFFSET = 0.75f;
    const float m_MOVE_FORCE_STRENGTH = 13.0f;
    const float m_REPULSION_DECAY = 5.0f;
    const float m_WANDER_RATE = 7.5f;

    Room m_room = null;

    Rigidbody m_rb;

    Transform m_player;

    EnemyState m_state;

    [SerializeField]
    Material m_defaultMat;

    [SerializeField]
    Material m_damagedMat;

    Vector3 m_acceleration = Vector3.zero;
    Vector3 m_targetPos = Vector3.zero;

    Vector3 m_roomCentre;
    Vector3 m_roomSize;

    float m_moveForce = 0.0f;
    float m_wanderOrientation = 0.0f;
    float m_detectionAngle = 30.0f;

    int m_health = 30;

    bool m_canAttack = true;

    // Start is called before the first frame update
    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();

        m_player = GameObject.FindGameObjectWithTag("Player").transform;

        m_state = EnemyState.Idle;
    }

    public void SetRoom(Room t_room)
    {
        m_roomCentre = t_room.m_position;
        m_roomSize = t_room.m_size;

        m_room = t_room;

        m_state = EnemyState.Wandering;
    }

    void FixedUpdate()
    {
        Move();

        switch (m_state)
        {
            case EnemyState.Wandering:
                UpdateWandering();
                break;
            case EnemyState.Pushback:
                UpdatePushback();
                break;
            case EnemyState.Chasing:
                UpdateChasing();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Attacking:
                UpdateAttacking();
                break;
            case EnemyState.TurnAround:
                UpdateTurnAround();
                break;
            default:
                break;
        }
    }

    void UpdateWandering()
    {
        Wander();
        DetectPlayer();
    }

    void UpdateChasing()
    {
        ChasePlayer();
    }

    void UpdatePushback()
    {
        if (m_rb.velocity.magnitude == 0.0f)
        {
            m_state = EnemyState.Chasing;
        }
    }

    void UpdateAttack()
    {
        if (m_canAttack)
        {
            StartCoroutine(Attack());
        }
    }

    void UpdateAttacking()
    {
        if (m_canAttack)
        {
            if ((transform.position - m_player.position).magnitude < m_ATTACK_RANGE)
            {
                m_state = EnemyState.Attack;
            }
            else
            {
                m_state = EnemyState.Chasing;
            }
        }
    }

    void UpdateTurnAround()
    {
        Vector3 direction = (m_targetPos - transform.position).normalized;

        if (FaceDirection(direction) <= 5.0f)
        {
            Vector3 wanderDir = transform.rotation * Vector3.forward;
            Vector3 orientationDir = Quaternion.AngleAxis(m_wanderOrientation, Vector3.up) * Vector3.forward;

            m_wanderOrientation -= Vector3.Angle(orientationDir, wanderDir);

            m_state = EnemyState.Wandering;
        }
    }

    void CalculateAcceleration()
    {
        m_acceleration = (m_targetPos - transform.position).normalized * m_moveForce;

        foreach (GameObject enemy in m_room.m_enemies)
        {
            if(enemy != null)
            {
                if (enemy.GetComponent<Enemy>() != this)
                {
                    AvoidTarget(enemy.transform.position);
                }
            }
        }

        m_acceleration.y = 0;

        FaceDirection(m_acceleration);
    }

    void Move()
    {
        if (m_moveForce != 0)
        {
            CalculateAcceleration();

            m_rb.AddForce(m_acceleration, ForceMode.Acceleration);

            LimitVelocity();
        }
    }

    void LimitVelocity()
    {
        Vector3 velocity = m_rb.velocity;

        velocity.y = 0;

        if (velocity.magnitude > 1.0f)
        {
            velocity = velocity.normalized * 1.0f;
        }

        velocity.y = m_rb.velocity.y;

        m_rb.velocity = velocity;
    }

    void Wander()
    {
        Vector3 tempTarget = transform.position + (transform.rotation * Vector3.forward * m_WANDER_OFFSET);

        m_wanderOrientation += Random.Range(-m_WANDER_RATE, m_WANDER_RATE);

        Quaternion radiusAngle = Quaternion.AngleAxis(m_wanderOrientation, Vector3.up);

        Vector3 tempDir = tempTarget - transform.position;

        tempDir += radiusAngle * Vector3.forward * 0.25f;

        m_targetPos = transform.position + tempDir;

        if (CheckOutOfBounds())
        {
            m_state = EnemyState.TurnAround;
            m_moveForce = 0;
        }
        else
        {
            m_moveForce = m_MOVE_FORCE_STRENGTH;
        }
    }

    float FaceDirection(Vector3 t_targetDir)
    {
        Vector3 dirNormalized = t_targetDir.normalized;

        Quaternion lookRotation = Quaternion.LookRotation(dirNormalized);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 5.0f);

        return Quaternion.Angle(transform.rotation, lookRotation);
    }

    bool CheckIsTargetInVision(Vector3 t_targetPos)
    {
        Vector3 targetDir = t_targetPos - transform.position;

        float angleToTarget = (Vector3.Angle(targetDir, m_acceleration));

        if(targetDir.magnitude <= m_DETECT_RANGE)
        {
            if (angleToTarget <= m_detectionAngle && angleToTarget >= -m_detectionAngle)
            {
                return true;
            }
        }

        return false;
    }

    public void AvoidTarget(Vector3 t_targetPos)
    {
        if(CheckIsTargetInVision(t_targetPos))
        {
            Vector3 repulsionVect = transform.position - t_targetPos;

            Debug.DrawRay(transform.position, repulsionVect.normalized, Color.green);

            float strength = m_REPULSION_DECAY / (repulsionVect.magnitude * repulsionVect.magnitude);

            repulsionVect = repulsionVect.normalized * strength;

            m_acceleration += repulsionVect;
        }
    }

    bool CheckOutOfBounds()
    {
        bool isOutOfBounds = false;

        Vector3 dir = m_targetPos - transform.position;

        if (Mathf.Abs((m_roomCentre - m_targetPos).x) > m_roomSize.x)
        {
            dir.x = -(dir.x);
            isOutOfBounds = true;
        }

        if(Mathf.Abs((m_roomCentre - m_targetPos).z) > m_roomSize.z)
        {
            dir.z = -(dir.z);
            isOutOfBounds = true;
        }

        m_targetPos = transform.position + dir;

        return isOutOfBounds;
    }

    IEnumerator Attack()
    {
        m_canAttack = false;

        m_state = EnemyState.Attacking;

        m_moveForce = 0.0f;

        m_player.gameObject.GetComponent<PlayerController>().TakeDamage(5);

        yield return new WaitForSeconds(1.0f);

        m_canAttack = true;
    }

    void DetectPlayer()
    {
        if(CheckIsTargetInVision(m_player.position))
        {
            if ((transform.position - m_player.position).magnitude < m_ATTACK_RANGE)
            {
                m_state = EnemyState.Attack;
            }

            else 
            {
                m_state = EnemyState.Chasing;
            }
        }
    }

    void ChasePlayer()
    {
        m_moveForce = m_MOVE_FORCE_STRENGTH;
        m_targetPos = m_player.position;

        DetectPlayer();
    }

    public void TakeDamage(int t_incomingDamage)
    {
        m_health -= t_incomingDamage;

        if(m_health < 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(ChangeColour());
        }
    }

    IEnumerator ChangeColour()
    {
        gameObject.GetComponent<MeshRenderer>().material = m_damagedMat;

        yield return new WaitForSeconds(0.5f);

        gameObject.GetComponent<MeshRenderer>().material = m_defaultMat;
    }

    public void PushBack(Vector3 t_sourcePos)
    {
        m_state = EnemyState.Pushback;

        Vector3 pushBackDir = (t_sourcePos - transform.position).normalized;
        pushBackDir.x = -(pushBackDir.x);
        pushBackDir.z = -(pushBackDir.z);

        m_rb.AddForce(pushBackDir * 10, ForceMode.Impulse);

        m_moveForce = 0.0f;
    }
}