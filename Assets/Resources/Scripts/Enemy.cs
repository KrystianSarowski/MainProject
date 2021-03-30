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
    AvoidWall
}

public class Enemy : MonoBehaviour
{
    const float m_DETECT_RANGE = 2.0f;
    const float m_ATTACK_RANGE = 1.0f;
    const float m_WANDER_OFFSET = 0.6f;
    const float m_MOVE_FORCE_STRENGTH = 15.0f;
    const float m_CLOSEST_DISTANCE = 0.5f;
    const float m_WANDER_RATE = 7.5f;
    const float m_MAX_SPEED = 1.5f;
    
    const int m_ATTACK_DAMAGE = 6;

    [SerializeField]
    GameObject m_healPrefab;

    [SerializeField]
    GameObject m_goldExplosionPrefab;

    Room m_room = null;

    Rigidbody m_rb;

    Transform m_player;

    EnemyState m_state;

    [SerializeField]
    Material m_defaultMat;

    [SerializeField]
    Material m_damagedMat;

    [SerializeField]
    ParticleSystem m_bloodSystem;

    Vector3 m_acceleration = Vector3.zero;
    Vector3 m_accelDir = Vector3.zero;
    Vector3 m_targetPos = Vector3.zero;

    Vector3 m_roomCentre;
    Vector3 m_roomSize;

    float m_moveForce = m_MOVE_FORCE_STRENGTH;
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

    public void Initialize(Room t_room)
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
            case EnemyState.AvoidWall:
                UpdateAvoidWall();
                break;
            default:
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == tag)
        {
            transform.position += (transform.position - collision.transform.position).normalized * 0.02f; 
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
        Vector3 vectToPlayer = m_player.position - transform.position;
        vectToPlayer.y = 0;

        if (m_canAttack)
        {
            if (vectToPlayer.magnitude < m_ATTACK_RANGE)
            {
                m_state = EnemyState.Attack;
            }

            else
            {
                m_state = EnemyState.Chasing;
            }
        }

        FaceDirection(vectToPlayer);
    }

    void UpdateAvoidWall()
    {
        Vector3 currentDir = transform.rotation * Vector3.forward;

        if (Vector3.Angle(m_targetPos - transform.position, currentDir) < 2.0f)
        {
            RealignOrientation(currentDir);
            m_state = EnemyState.Wandering;
        }
    }

    void RealignOrientation(Vector3 t_targetDir)
    {
        Vector3 orientationDir = Quaternion.AngleAxis(m_wanderOrientation, Vector3.up) * Vector3.forward;

        m_wanderOrientation -= Vector3.Angle(orientationDir, t_targetDir);
    }

    void CalculateAcceleration()
    {
        m_accelDir = (m_targetPos - transform.position).normalized * m_moveForce;

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

        m_accelDir.y = 0;
        FaceDirection(m_accelDir);

        m_acceleration = transform.rotation * Vector3.forward * m_accelDir.magnitude;
    }

    void Move()
    {
        if (m_state != EnemyState.Pushback)
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

        if (velocity.magnitude > m_MAX_SPEED)
        {
            velocity = velocity.normalized * m_MAX_SPEED;
        }

        velocity.y = m_rb.velocity.y;

        m_rb.velocity = velocity;
    }

    void Wander()
    {
        if (CheckForWall())
        {
            AvoidWall();
            m_state = EnemyState.AvoidWall;
        }
        else
        {
            Vector3 tempTarget = transform.position + (transform.rotation * Vector3.forward * m_WANDER_OFFSET);

            m_wanderOrientation += Random.Range(-m_WANDER_RATE, m_WANDER_RATE);

            Quaternion radiusAngle = Quaternion.AngleAxis(m_wanderOrientation, Vector3.up);

            Vector3 tempDir = tempTarget - transform.position;

            tempDir += radiusAngle * Vector3.forward * 0.4f;

            m_targetPos = transform.position + tempDir;
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
            Vector3 dirToTarget = t_targetPos - transform.position;

            float angleToTarget =  Vector3.Angle(m_accelDir, dirToTarget);

            Debug.DrawRay(transform.position, dirToTarget.normalized, Color.green);

            float strength = m_CLOSEST_DISTANCE / dirToTarget.magnitude;

            angleToTarget = angleToTarget * strength;

            m_accelDir = Quaternion.AngleAxis(-angleToTarget, Vector3.up) * m_accelDir; ;

            RealignOrientation(m_accelDir);
        }
    }

    void AvoidWall()
    {
        Vector3 dir = transform.rotation * Vector3.forward * 1.2f;
        Vector3 tempTargetPos = transform.position + dir;

        if (Mathf.Abs((m_roomCentre - tempTargetPos).x) > m_roomSize.x)
        {
            dir.x = -(dir.x);
        }

        if (Mathf.Abs((m_roomCentre - tempTargetPos).z) > m_roomSize.z)
        {
            dir.z = -(dir.z);
        }

        m_targetPos = transform.position + dir;
    }

    bool CheckForWall()
    {
        Vector3 dir = transform.rotation * Vector3.forward * 1.2f;
        Vector3 detectPos = transform.position + dir;

        if (Mathf.Abs((m_roomCentre - detectPos).x) > m_roomSize.x ||
            Mathf.Abs((m_roomCentre - detectPos).z) > m_roomSize.z)
        {
            return true;
        }

        return false;
    }

    IEnumerator Attack()
    {
        m_canAttack = false;

        m_state = EnemyState.Attacking;

        m_moveForce = 0.0f;

        PlayerStats.DealDamage(m_ATTACK_DAMAGE);

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
            int random = Random.Range(0, 100);

            if(random < 5)
            {
                GameObject heal =  Instantiate(m_healPrefab, transform.position, m_healPrefab.transform.rotation);
                heal.transform.position += Vector3.up * 0.5f;
            }

            if(m_goldExplosionPrefab != null)
            {
                Instantiate(m_goldExplosionPrefab, transform.position, m_goldExplosionPrefab.transform.rotation);
            }

            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(ChangeColour());
            m_bloodSystem.StartSystem();
            m_state = EnemyState.Chasing;
        }
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

    IEnumerator ChangeColour()
    {
        gameObject.GetComponent<MeshRenderer>().material = m_damagedMat;

        yield return new WaitForSeconds(0.5f);

        gameObject.GetComponent<MeshRenderer>().material = m_defaultMat;
    }
}