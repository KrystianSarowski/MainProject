﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossStateType
{ 
    Defence, 
    Attack,
    Projectal,
    Charge
}

public class Boss : MonoBehaviour
{
    [SerializeField]
    int m_currentHealth = 100;
    int m_previousHealth = 100;
    int m_stateIndex = 0;

    float m_maxWalkVelocity = 1.5f;
    float m_maxChargeVelocity = 7.0f;

    bool m_canRotate = true;
    bool m_wasPlayerHit = true;
    bool m_hasCollidedWithWall = false;

    int m_attackDamage = 10;
    int m_chargeDamage = 30;

    Transform m_playerTransform;

    [SerializeField]
    Transform m_projectalSpawn;

    List<BossState> m_bossStates = new List<BossState>();

    Room m_room;

    Rigidbody m_rb;

    [SerializeField]
    GameObject m_goopProjectal;

    [SerializeField]
    ParticleSystem m_bloodSystem;

    public void Initialize(Room t_room, Transform t_playerTransform)
    {
        m_room = t_room;
        m_playerTransform = t_playerTransform;
        m_previousHealth = m_currentHealth;

        m_bossStates.Add(new BossDefenceState());
        m_bossStates.Add(new BossAttackState());
        m_bossStates.Add(new BossProjectalState());
        m_bossStates.Add(new BossChargeState());

        foreach(BossState bossState in m_bossStates)
        {
            bossState.Initialize(this);
        }

        m_rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        LookAtPlayer();
        m_bossStates[m_stateIndex].Update();
        LimitVelocity();

        m_previousHealth = m_currentHealth;
    }

    void LookAtPlayer()
    {
        if(m_canRotate)
        {
            Vector3 dirNormalized = (m_playerTransform.position - transform.position).normalized;

            Quaternion lookRotation = Quaternion.LookRotation(dirNormalized);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 5.0f);
        }
    }

    void LimitVelocity()
    {
        Vector3 velocity = m_rb.velocity;

        velocity.y = 0;

        if(m_stateIndex != 3)
        {
            if (velocity.magnitude > m_maxWalkVelocity)
            {
                velocity = velocity.normalized * m_maxWalkVelocity;
            }
        }
        else
        {
            if (velocity.magnitude > m_maxChargeVelocity)
            {
                velocity = velocity.normalized * m_maxChargeVelocity;
            }
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
            m_wasPlayerHit = false;
        }
    }

    public void StopMoving()
    {
        m_rb.velocity = new Vector3(0, m_rb.velocity.y, 0);
    }

    public void Attack()
    {
        if(m_stateIndex == (int)BossStateType.Projectal)
        {
            Vector3 projectalDir = transform.TransformDirection(Vector3.forward);

            GameObject bullet = Instantiate(m_goopProjectal, m_projectalSpawn.position, m_goopProjectal.transform.rotation);

            bullet.GetComponent<Projectal>().Fire(projectalDir, 1);
        }
        else if(m_stateIndex == (int)BossStateType.Attack)
        {
            PlayerStats.DealDamage(m_attackDamage);
        }
    }

    public void TakeDamage(int t_damage)
    {
        m_currentHealth -= t_damage;
        m_bloodSystem.StartSystem();

        if (m_currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetCanRotate(bool t_canRotate)
    {
        m_canRotate = t_canRotate;
    }

    public void SetHasCollidedWithWall(bool t_hasCollidedWithWall)
    {
        m_hasCollidedWithWall = t_hasCollidedWithWall;
    }

    public bool HasCollidedWithWall()
    {
        return m_hasCollidedWithWall;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Walls")
        {
            if(!HasCollidedWithWall())
            {
                SetHasCollidedWithWall(true);
            }
        }

        if(collision.transform.tag == "Player")
        {
            if(!m_wasPlayerHit && m_stateIndex == (int)BossStateType.Charge)
            {
                PlayerStats.DealDamage(m_chargeDamage);
                m_wasPlayerHit = true;
            }
        }
    }
}

public class BossState
{
    protected float m_timePassed;
    protected float m_totalDistance;
    protected float m_averageDistance;

    protected int m_numOfUpdates;

    protected Boss m_boss;

    public void Initialize(Boss t_boss)
    {
        m_boss = t_boss;
    }

    public virtual void Enter()
    {
        m_timePassed = 0.0f;
        ResetData();
    }

    void ResetData()
    {
        m_totalDistance = 0.0f;
        m_averageDistance = 0.0f;
        m_numOfUpdates = 0;
    }

    public virtual void Update()
    {
        if (m_numOfUpdates == 60)
        {
            HandleData();
            ResetData();
        }

        m_numOfUpdates++;

        m_timePassed += Time.fixedDeltaTime;

        m_totalDistance += m_boss.GetDistanceToPlayer();

        m_averageDistance = m_totalDistance / m_numOfUpdates;
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

    public const float s_ATTACK_RANGE = 1.5f;
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
            else if (m_averageDistance > BossProjectalState.s_ATTACK_RANGE)
            {
                m_boss.SetBossState(BossStateType.Projectal);
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
                m_boss.Attack();
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
    float m_speed = 4.0f;

    Vector3 m_moveDir = Vector3.zero;

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        Vector3 dir = m_boss.transform.TransformDirection(m_moveDir).normalized;
        Vector3 bodyPos = m_boss.transform.position;

        if (!m_boss.IsWithinRoom(dir + bodyPos) || m_moveDir == Vector3.zero)
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

        if (m_boss.IsWithinRoom(dir + bodyPos))
        {
            m_moveDir = Vector3.left;
            return;
        }

        dir = m_boss.transform.TransformDirection(Vector3.right).normalized;

        if (m_boss.IsWithinRoom(dir + bodyPos))
        {
            m_moveDir = Vector3.right;
            return;
        }

        m_moveDir = Vector3.zero;
    }

    public override void HandleData()
    {
        if(m_timePassed > 4.0f)
        {
            m_boss.SetBossState(BossStateType.Charge);
        }
        else if(m_averageDistance < BossAttackState.s_ATTACK_RANGE)
        {
            m_boss.SetBossState(BossStateType.Attack);
        }
        else if(m_averageDistance > BossProjectalState.s_ATTACK_RANGE)
        {
            m_boss.SetBossState(BossStateType.Projectal);
        }
    }

    public override void Exit()
    {
        m_boss.StopMoving();
    }
}

public class BossProjectalState : BossState
{
    bool m_isAttacking = false;

    public const float s_ATTACK_RANGE = 4.0f;
    const float s_MAX_ATTACK_DELAY = 1.0f;

    float m_attackDelay = 0.0f;

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        if (m_isAttacking)
        {
            Attack();
        }
        base.Update();
    }

    public override void HandleData()
    {
        if(!m_isAttacking)
        {
            if(m_timePassed > 5.0f)
            {
                m_boss.SetBossState(BossStateType.Charge);
            }

            else if (m_averageDistance >= s_ATTACK_RANGE)
            {
                m_isAttacking = true;
                m_attackDelay = 0.0f;
                m_boss.Attack();
            }
            else
            {
                m_boss.SetBossState(BossStateType.Defence);
            }
        }
    }

    void Attack()
    {
        if (m_attackDelay >= s_MAX_ATTACK_DELAY)
        {
            m_isAttacking = false;
        }

        m_attackDelay += Time.fixedDeltaTime;
    }

    public override void Exit()
    {

    }
}

public class BossChargeState : BossState
{
    bool m_isCharging = false;

    float m_speed = 6.0f;

    const float s_MAX_CHARGE_TIME = 2.0f;

    Vector3 m_chargeDir = Vector3.zero;

    public override void Enter()
    {
        base.Enter();
        m_isCharging = true;

        m_chargeDir = m_boss.transform.TransformDirection(Vector3.forward).normalized;
        m_boss.SetCanRotate(false);
        m_boss.SetHasCollidedWithWall(false);
    }

    public override void Update()
    {
        if(m_isCharging)
        {
            Charge();
        }

        base.Update();
    }

    public override void HandleData()
    {
        if(!m_isCharging)
        {
            if (m_averageDistance < BossAttackState.s_ATTACK_RANGE)
            {
                m_boss.SetBossState(BossStateType.Attack);
            }
            else
            {
                m_boss.SetBossState(BossStateType.Defence);
            }
        }
        else
        {
            if(m_timePassed > s_MAX_CHARGE_TIME)
            {
                m_boss.SetBossState(BossStateType.Defence);
            }
        }
    }

    public override void Exit()
    {
        m_boss.SetCanRotate(true);
    }

    void Charge()
    {
        m_boss.GetComponent<Rigidbody>().AddForce(m_chargeDir * m_speed, ForceMode.Impulse);

        if(m_boss.HasCollidedWithWall())
        {
            m_isCharging = false;
        }
    }
}