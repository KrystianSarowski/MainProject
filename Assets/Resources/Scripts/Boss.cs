using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossStateType
{ 
    Defence, 
    Attack,
    Shockwave,
    Charge,
    Projectal
}

public class Boss : MonoBehaviour
{
    int m_currentHealth = 100;
    int m_previousHealth = 100;
    int m_stateIndex = 0;

    Transform m_playerTransform;

    List<BossState> m_bossStates = new List<BossState>();

    Room m_room;

    Rigidbody m_rb;

    public void Initialize(Room t_room, Transform t_playerTransform)
    {
        m_room = t_room;
        m_playerTransform = t_playerTransform;

        m_bossStates.Add(new BossDefenceState());
        m_bossStates.Add(new BossAttackState());

        foreach(BossState bossState in m_bossStates)
        {
            bossState.Initialize(this);
        }

        m_rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        m_previousHealth = m_currentHealth;

        LookAtPlayer();
        m_bossStates[m_stateIndex].Update();
        LimitVelocity();
    }

    void LookAtPlayer()
    {
        Vector3 dirNormalized = (m_playerTransform.position - transform.position).normalized;

        Quaternion lookRotation = Quaternion.LookRotation(dirNormalized);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 5.0f);
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

    public bool IsWithinRoom(Vector3 t_position)
    {
        Vector3 pos = m_room.m_position - t_position;
        Vector3 roomSize = m_room.m_size;

        if(Mathf.Abs(pos.z) < roomSize.z && Mathf.Abs(pos.x) < roomSize.x)
        {
            return true;
        }

        return false;
    }

    public int GetCurrentHealth()
    {
        return m_currentHealth;
    }

    public int GetPreviousHealth()
    {
        return m_previousHealth;
    }

    public int GetChangeInHealth()
    {
        return m_currentHealth - m_previousHealth;
    }

    public float GetDistanceToPlayer()
    {
        return (m_playerTransform.position - transform.position).magnitude;
    }

    public void SetBossState(BossStateType t_bossStateType)
    {
        int newStateIndex = (int)t_bossStateType;

        if(newStateIndex != m_stateIndex)
        {
            m_bossStates[m_stateIndex].Exit();
            m_bossStates[newStateIndex].Enter();
            m_stateIndex = newStateIndex;
        }
    }

    public void StopMoving()
    {
        m_rb.velocity = new Vector3(0, m_rb.velocity.y, 0);
    }
}

public class BossState
{
    protected float m_timePassed;
    protected float m_totalDistance;
    protected float m_averageDistance;

    protected int m_damageTaken;
    protected int m_averageDamageTaken;
    protected int m_numOfUpdates;

    protected Boss m_boss;

    public void Initialize(Boss t_boss)
    {
        m_boss = t_boss;
    }

    public virtual void Enter()
    {
        ResetData();
    }

    void ResetData()
    {
        m_timePassed = 0.0f;
        m_totalDistance = 0.0f;
        m_averageDistance = 0.0f;

        m_damageTaken = 0;
        m_averageDamageTaken = 0;
        m_numOfUpdates = 0;
    }

    public virtual void Update()
    {
        if (m_numOfUpdates == 20)
        {
            ResetData();
        }

        m_numOfUpdates++;

        m_timePassed += Time.fixedDeltaTime;

        m_damageTaken += m_boss.GetChangeInHealth();

        m_averageDamageTaken = m_damageTaken / m_numOfUpdates;

        m_totalDistance += m_boss.GetDistanceToPlayer();

        m_averageDistance = m_totalDistance / m_numOfUpdates;

        HandleData();
    }

    public virtual void HandleData()
    {

    }

    public virtual void Exit()
    {

    }
}

public class BossAttackState : BossState
{
    bool m_isAttacking = false;

    public const float s_ATTACK_RANGE = 2.0f;
    const float s_MAX_ATTACK_DELAY = 1.0f;

    float m_attackDelay = 0.0f;

    public override void Enter()
    {
        m_isAttacking = false;
        base.Enter();
    }

    public override void Update()
    {
        if(m_isAttacking)
        {
            Attack();
        }
        base.Update();
    }

    public override void HandleData()
    {
        if(!m_isAttacking)
        {
            if(m_averageDistance <= s_ATTACK_RANGE)
            {
                m_isAttacking = true;
                m_attackDelay = 0.0f;
            }
            else
            {
                m_boss.SetBossState(BossStateType.Defence);
            }
        }
    }

    void Attack()
    {
        if(m_attackDelay >= s_MAX_ATTACK_DELAY)
        {
            if(m_boss.GetDistanceToPlayer() <= s_ATTACK_RANGE)
            {
                PlayerStats.DealDamage(15);
            }
            m_isAttacking = false;
        }

        m_attackDelay += Time.fixedDeltaTime;
    }

    public override void Exit()
    {
        
    }
}

public class BossDefenceState : BossState
{
    float m_speed = 1.0f;

    Vector3 m_moveDir = Vector3.zero;

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        Vector3 dir = m_boss.transform.TransformDirection(m_moveDir).normalized;
        Vector3 bodyPos = m_boss.transform.position;

        if (!m_boss.IsWithinRoom(dir * m_speed + bodyPos) || m_moveDir == Vector3.zero)
        {
            SetNewMoveDir();
        }

        dir = m_boss.transform.TransformDirection(m_moveDir).normalized;
        m_boss.GetComponent<Rigidbody>().AddForce(dir * m_speed, ForceMode.Impulse);

        base.Update();
    }

    void SetNewMoveDir()
    {
        Vector3 dir = m_boss.transform.TransformDirection(Vector3.left).normalized;
        Vector3 bodyPos = m_boss.transform.position;

        if (m_boss.IsWithinRoom(dir * m_speed + bodyPos))
        {
            m_moveDir = Vector3.left;
            return;
        }

        dir = m_boss.transform.TransformDirection(Vector3.right).normalized;

        if (m_boss.IsWithinRoom(dir * m_speed + bodyPos))
        {
            m_moveDir = Vector3.right;
            return;
        }

        m_moveDir = Vector3.zero;
    }

    public override void HandleData()
    {
        if(m_averageDistance < BossAttackState.s_ATTACK_RANGE)
        {
            m_boss.SetBossState(BossStateType.Attack);
        }
    }

    public override void Exit()
    {
        m_boss.StopMoving();
    }
}