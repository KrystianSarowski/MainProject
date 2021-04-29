using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski
public enum EnemyState
{ 
    Wander,
    Pushback,
    Chase,
    Flee,
    Avoid,
    MeleeAttack,
    RangedAttack
}

public class Obstacle
{
    public Collider m_collider;
    public float m_distance;
}

public class Enemy : MonoBehaviour
{
    const float s_COLLISION_VISION_RANGE = 1.0f;
    const float s_COLLISION_DISTANCE = 0.5f;
    const float s_COLLISION_FOV = 120.0f;

    const float s_FOV = 90.0f;
    const float s_MELEE_ATTACK_DELAY = 0.2f;
    const float s_RANGED_ATTACK_DELAY = 1.0f;
    const float s_RANGED_ATTACK_RANGE = 3.0f;
    const float s_VISION_RANGE = 5.0f;
    const float s_MAX_SPEED = 2.0f;
    const float s_ACCELERATION_STRENGTH = 15.0f;
    const float s_WANDER_RATE = 7.5f;
    const float s_WANDER_OFFSET = 0.6f;
    const float s_MELEE_RANGE = 1.0f;

    const int s_HEAL_CHANCE = 5;
    const int s_COLLISION_RAYS = 7;

    int m_centerIndex;
    int m_turnAroundCount;
    int m_meleeDamage = 2;

    float m_angle;
    float m_health = 50;
    float m_wanderOrientation = 0.0f;
    float m_attackCooldown;
    float m_damageMultiplier;

    bool m_move = true;
    bool m_turnAround = false;
    bool m_dead = false;

    EnemyState m_currentState = EnemyState.Wander;
    EnemyState m_previousState = EnemyState.Wander;

    List<Obstacle> m_colliders = new List<Obstacle>();
    List<string> m_ignoreList = new List<string>();

    Rigidbody m_rb;

    Vector3 m_acceleration;
    Vector3 m_targetPosition;

    [SerializeField]
    GameObject m_healPrefab;

    [SerializeField]
    GameObject m_fireballPrefab;

    [SerializeField]
    GameObject m_goldExplosionPrefab;

    [SerializeField]
    Transform m_projectalOrigin;

    [SerializeField]
    ParticleSystem m_bloodSystem;    
    
    [SerializeField]
    ParticleSystem m_fireBreathSystem;

    PlayerController m_player;

    // Start is called before the first frame update
    void Start()
    {
        m_angle = s_COLLISION_FOV / s_COLLISION_RAYS;

        m_centerIndex = s_COLLISION_RAYS / 2 + (s_COLLISION_RAYS % 2) - 1;

        for (int i = 0; i < s_COLLISION_RAYS; i++)
        {
            m_colliders.Add(new Obstacle());
        }

        m_rb = GetComponent<Rigidbody>();

        m_player = FindObjectOfType<PlayerController>();

        m_ignoreList.Add("Player");
        m_ignoreList.Add("Projectal");
        m_ignoreList.Add("Particle");
        m_ignoreList.Add("Coin");

        m_health = m_health * FindObjectOfType<GameplayManager>().GetCurrentLevel();
        m_damageMultiplier = FindObjectOfType<GameplayManager>().GetCurrentLevel();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LookAround();
        ProccessColliders();

        switch (m_currentState)
        {
            case EnemyState.Avoid:
                Avoid();
                break;
            case EnemyState.Wander:
                Wander();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Pushback:
                if (m_rb.velocity.magnitude == 0.0f)
                {
                    SetCurrentState(m_previousState);
                }
                m_move = false;
                break;
            case EnemyState.MeleeAttack:
                MeleeAttack();
                break;
            case EnemyState.RangedAttack:
                RangeAttack();
                break;
            default:
                break;
        }

        LookTowardsTarget();
        Move();
    }

    void SetCurrentState(EnemyState t_newState)
    {
        m_previousState = m_currentState;
        m_currentState = t_newState;

        switch (m_previousState)
        {
            case EnemyState.MeleeAttack:
                m_fireBreathSystem.StopSystem();
                break;
            default:
                break;
        }

        switch (m_currentState)
        {
            case EnemyState.Wander:
                RealignOrientation(transform.forward);
                break;
            case EnemyState.MeleeAttack:
                m_attackCooldown = s_MELEE_ATTACK_DELAY;
                break;
            case EnemyState.RangedAttack:
                m_attackCooldown = s_RANGED_ATTACK_DELAY;
                break;
            default:
                break;
        }
    }

    void LookAround()
    {
        Vector3 dirVector = transform.forward;

        dirVector = Quaternion.AngleAxis(-s_COLLISION_FOV / 2, Vector3.up) * dirVector;

        for (int i = 0; i < s_COLLISION_RAYS; i++)
        {
            Ray ray = new Ray(transform.position, dirVector);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.distance <= s_COLLISION_VISION_RANGE)
                {
                    m_colliders[i].m_collider = hit.collider;
                    m_colliders[i].m_distance = hit.distance;
                }
                else
                {
                    m_colliders[i].m_collider = null;
                }
            }

            dirVector = Quaternion.AngleAxis(m_angle, Vector3.up) * dirVector;
        }
    }

    void CalculateAcceleration()
    {
        m_acceleration = transform.forward * s_ACCELERATION_STRENGTH;
    }

    void Move()
    {
        if (m_move)
        {
            CalculateAcceleration();

            if(m_rb.velocity.magnitude == 0)
            {
                m_rb.velocity = transform.TransformDirection(Vector3.forward) * 0.001f;
            }

            m_rb.AddForce(m_acceleration, ForceMode.Acceleration);

            LimitVelocity();
        }
        else
        {
            m_move = true;
        }
    }

    void LimitVelocity()
    {
        Vector3 velocity = m_rb.velocity;

        velocity.y = 0;

        if (velocity.magnitude > s_MAX_SPEED)
        {
            velocity = velocity.normalized * s_MAX_SPEED;
        }

        velocity.y = m_rb.velocity.y;

        m_rb.velocity = velocity;
    }

    void ProccessColliders()
    {
        if (m_currentState != EnemyState.Avoid && m_currentState != EnemyState.Pushback)
        {
            if(CheckForObstacles())
            {
                SetCurrentState(EnemyState.Avoid);
            }
        }
    }

    bool CheckForObstacles()
    {
        for (int i = 0; i < s_COLLISION_RAYS; i++)
        {
            if (m_colliders[i].m_collider != null)
            {
                if (!m_ignoreList.Contains(m_colliders[i].m_collider.tag))
                {
                    if (i == m_centerIndex)
                    {
                        return true;
                    }
                    else if (m_colliders[i].m_distance < s_COLLISION_DISTANCE)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void Avoid()
    {
        if (!CheckForObstacles() && !m_turnAround)
        {
            if(m_previousState == EnemyState.RangedAttack)
            {
                SetCurrentState(EnemyState.Wander);
            }
            else
            {
                SetCurrentState(m_previousState);
            }
        }
        else
        {
            if((m_colliders[m_centerIndex].m_collider != null && !m_ignoreList.Contains(m_colliders[m_centerIndex].m_collider.tag)) || m_turnAround)
            {
                AvoidFront();
            }
            else
            {
                AvoidSides();
            }
        }
    }

    /// <summary>
    /// Attempts to avoid the obstacles directly infornt of the Enemy
    /// by selecting the side with least amount of obstacles.
    /// If both of the side are fully blocked the enemy will try to rotate
    /// around to the right for 6 updates and try again.
    /// </summary>
    void AvoidFront()
    {
        if(!m_turnAround)
        {
            int leftSideCount = 0;
            int rightSideCount = 0;

            for (int i = 1; i < m_centerIndex + 1; i++)
            {
                if (m_colliders[m_centerIndex - i].m_collider != null)
                {
                    if (m_colliders[m_centerIndex - i].m_collider.tag != "Player")
                    {
                        leftSideCount++;
                    }
                }

                if (m_colliders[m_centerIndex + i].m_collider != null)
                {
                    if (m_colliders[m_centerIndex + i].m_collider.tag != "Player")
                    {
                        rightSideCount++;
                    }
                }
            }

            if (leftSideCount + rightSideCount == s_COLLISION_RAYS - 1)
            {
                m_move = false;
                m_turnAround = true;
                m_turnAroundCount = 0;

                m_targetPosition = transform.position + Quaternion.AngleAxis(30, Vector3.up) * transform.forward; 
            }
            else if (leftSideCount < rightSideCount)
            {
                m_targetPosition = transform.position + Quaternion.AngleAxis(-10, Vector3.up) * transform.forward;
            }
            else
            {
                m_targetPosition = transform.position + Quaternion.AngleAxis(10, Vector3.up) * transform.forward;
            }
        }
        else
        {
            m_turnAroundCount++;
            m_move = false;

            if (m_turnAroundCount == 6)
            {
                m_turnAround = false;
            }
        }
    }

    /// <summary>
    /// Avoids the obstacles that are to the side of Enemy by choosing the direction
    /// for which the obstacles are the furthest away or has the least amount of obstacles.
    /// The resulting direction angle is then capped to avoid it rotating to much to the other side.
    /// </summary>
    void AvoidSides()
    {
        Vector3 dirVector = transform.forward;
        float angleOffset = m_angle * (m_centerIndex + 1);
        float rotationAngle = 0;

        for (int i = 1; i < m_centerIndex + 1; i++)
        {
            if (m_colliders[m_centerIndex - i].m_collider != null)
            {
                if (!m_ignoreList.Contains(m_colliders[m_centerIndex - i].m_collider.tag))
                {
                    float strength = Mathf.InverseLerp(s_COLLISION_VISION_RANGE, s_COLLISION_DISTANCE, Mathf.Max(s_COLLISION_DISTANCE, m_colliders[m_centerIndex - i].m_distance));
                    rotationAngle = angleOffset * strength;
                }
            }

            if (m_colliders[m_centerIndex + i].m_collider != null)
            {
                if (!m_ignoreList.Contains(m_colliders[m_centerIndex + i].m_collider.tag))
                {
                    float strength = Mathf.InverseLerp(s_COLLISION_VISION_RANGE, s_COLLISION_DISTANCE, Mathf.Max(s_COLLISION_DISTANCE, m_colliders[m_centerIndex + i].m_distance));
                    rotationAngle = -angleOffset * strength;
                }
            }
        }

        if(rotationAngle < -90.0f)
        {
            rotationAngle = -90.0f;
        }
        else if(rotationAngle > 90.0f)
        {
            rotationAngle = 90.0f;
        }

        dirVector = Quaternion.AngleAxis(rotationAngle, Vector3.up) * dirVector;
        m_targetPosition = transform.position + dirVector;
    }

    void Wander()
    {
        Vector3 tempTarget = transform.position + (transform.rotation * Vector3.forward * s_WANDER_OFFSET);

        m_wanderOrientation += Random.Range(-s_WANDER_RATE, s_WANDER_RATE);

        Quaternion radiusAngle = Quaternion.AngleAxis(m_wanderOrientation, Vector3.up);

        Vector3 tempDir = tempTarget - transform.position;

        tempDir += radiusAngle * Vector3.forward * 0.7f;

        m_targetPosition = transform.position + tempDir;

        if(LookForPlayer())
        {
            SetCurrentState(EnemyState.Chase);
        }
    }

    void Chase()
    {
        if(LookForPlayer())
        {
            m_targetPosition = m_player.transform.position;
            m_targetPosition.y = transform.position.y;

            if((m_targetPosition - transform.position).magnitude < s_MELEE_RANGE)
            {
                SetCurrentState(EnemyState.MeleeAttack);
                m_fireBreathSystem.StartSystem();
            }
            else if((m_targetPosition - transform.position).magnitude > s_RANGED_ATTACK_RANGE)
            {
                SetCurrentState(EnemyState.RangedAttack);
            }
        }
        else
        {
            if((transform.position - m_targetPosition).magnitude < 0.1f)
            {
                SetCurrentState(EnemyState.Wander);
            }
        }
    }

    /// <summary>
    /// Deals melee damage to the player each time the cooldown reaches 0.
    /// If the certain conditions of this state are met the Enemy will then proceced
    /// to change state. Slight offset is required for the melee range due to the truncation
    /// of player height. When the state is changed the fire ParticleSystem is stopped.
    void MeleeAttack()
    {
        m_attackCooldown -= Time.fixedDeltaTime;
        m_move = false;

        if (LookForPlayer())
        {
            m_targetPosition = m_player.transform.position;

            if (m_attackCooldown <= 0.0f)
            {
                if ((m_targetPosition - transform.position).magnitude < s_MELEE_RANGE + 0.1f)
                {
                    PlayerStats.DealDamage((int)(m_meleeDamage * m_damageMultiplier));
                }
                else
                {
                    m_fireBreathSystem.StopSystem();
                    SetCurrentState(EnemyState.Chase);
                }

                m_attackCooldown = s_MELEE_ATTACK_DELAY;
            }
        }
        else
        {
            if (m_attackCooldown <= 0.0f)
            {
                m_fireBreathSystem.StopSystem();
                SetCurrentState(EnemyState.Chase);
            }
        }
    }

    /// <summary>
    /// Fire a projectal at the player when it is able to.
    /// If the certain conditions of this state are met the Enemy will then proceced
    /// to change state. Slight offset is required for the melee range due to the truncation
    /// of player height.
    /// </summary>
    void RangeAttack()
    {
        m_attackCooldown -= Time.fixedDeltaTime;
        m_move = false;

        if (LookForPlayer())
        {
            m_targetPosition = m_player.transform.position;

            if (m_attackCooldown <= 0.0f)
            {
                float distToPlayer = (m_targetPosition - transform.position).magnitude;

                if (distToPlayer < s_RANGED_ATTACK_RANGE)
                {
                    SetCurrentState(EnemyState.Chase);
                }
                else if(distToPlayer <= s_MELEE_RANGE + 0.1f)
                {
                    SetCurrentState(EnemyState.MeleeAttack);
                }
                else
                {
                    GameObject fireBall = Instantiate(m_fireballPrefab, m_projectalOrigin.position, m_fireballPrefab.transform.rotation);
                    fireBall.GetComponent<Projectal>().Fire((m_targetPosition + new Vector3(0, 0.25f, 0) - m_projectalOrigin.position).normalized, m_damageMultiplier);
                }

                m_attackCooldown = s_RANGED_ATTACK_DELAY;
            }
        }
        else
        {
            if (m_attackCooldown <= 0.0f)
            {
                SetCurrentState(EnemyState.Wander);
            }
        }
    }

    void LookTowardsTarget()
    {
        m_targetPosition.y = transform.position.y;

        Vector3 dirNormalized = (m_targetPosition - transform.position).normalized;

        Quaternion lookRotation = Quaternion.LookRotation(dirNormalized);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 5.0f);
    }

    bool LookForPlayer()
    {
        if(m_player != null)
        {
            Vector3 dirToPlayer = m_player.transform.position - transform.position;
            dirToPlayer.Normalize();

            if(Mathf.Abs(Vector3.Angle(transform.forward, dirToPlayer)) <= s_FOV / 2)
            {
                //We have to do this if the player is right ontop of us or else the ray
                //will miss him.
                if((m_player.transform.position - transform.position).magnitude < 0.5f)
                {
                    return true;
                }

                Ray ray = new Ray(m_projectalOrigin.position, dirToPlayer);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if(hit.distance < s_VISION_RANGE && hit.collider.tag == "Player")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void RealignOrientation(Vector3 t_targetDir)
    {
        Vector3 orientationDir = Quaternion.AngleAxis(m_wanderOrientation, Vector3.up) * Vector3.forward;

        m_wanderOrientation -= Vector3.Angle(orientationDir, t_targetDir);
    }

    public void TakeDamage(int t_incomingDamage)
    {
        m_health -= t_incomingDamage;

        if (m_health < 0 && !m_dead)
        {
            m_dead = true;
            int random = Random.Range(0, 100);

            if (random < s_HEAL_CHANCE)
            {
                GameObject heal = Instantiate(m_healPrefab, transform.position, m_healPrefab.transform.rotation);
                heal.transform.position += Vector3.up * 0.5f;
            }

            if (m_goldExplosionPrefab != null)
            {
                Instantiate(m_goldExplosionPrefab, transform.position, m_goldExplosionPrefab.transform.rotation);
            }

            Destroy(gameObject);
        }
        else
        {
            m_bloodSystem.StartSystem();
            m_targetPosition = m_player.transform.position;
            SetCurrentState(EnemyState.Chase);
        }
    }

    public void PushBack(Vector3 t_sourcePos)
    {
        SetCurrentState(EnemyState.Pushback);

        Vector3 pushBackDir = (t_sourcePos - transform.position).normalized;
        pushBackDir.x = -(pushBackDir.x);
        pushBackDir.z = -(pushBackDir.z);

        m_rb.AddForce(pushBackDir * 10, ForceMode.Impulse);
        m_move = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == tag)
        {
            transform.position += (transform.position - collision.transform.position).normalized * 0.02f;
        }

        else if (collision.gameObject.tag == "Coin")
        {
            Destroy(collision.gameObject);
        }
    }

}