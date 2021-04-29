using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public class Sword : Weapon
{
    bool m_return = false;

    Quaternion m_defaultRotation;
    Quaternion m_targetRotation;

    float m_rotation = 0.0f;
    float m_rotationIncrease = 0.1f;
    float m_attackRange = 1.5f;

    [SerializeField]
    int m_baseDamage;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_isAttacking)
        {
            AnimateSwing();
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        m_defaultRotation = transform.localRotation;
        transform.Rotate(Vector3.down, 60.0f);
        m_targetRotation = transform.localRotation;

        FindObjectOfType<PlayerUI>().EnableAmmoText(false);

        transform.localRotation = m_defaultRotation;
    }

    void AnimateSwing()
    {
        m_rotation += m_rotationIncrease;

        if (!m_return)
        {
            transform.localRotation = Quaternion.Lerp(m_defaultRotation, m_targetRotation, m_rotation);

            if (m_rotation >= 1.0f)
            {
                m_rotation = 0.0f;
                m_return = true;
            }
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(m_targetRotation, m_defaultRotation, m_rotation);

            if (m_rotation >= 1.0f)
            {
                m_isAttacking = false;
            }
        }
    }

    public override void Attack()
    {
        if(!m_isAttacking)
        {
            m_isAttacking = true;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.distance <= m_attackRange)
                {
                    if (hit.collider.tag == "Enemy")
                    {
                        hit.collider.GetComponent<Enemy>().TakeDamage((int)(m_baseDamage * m_stats.m_damageMultiplier));
                        hit.collider.GetComponent<Enemy>().PushBack(transform.position);
                    }
                    else if(hit.collider.tag == "Boss")
                    {
                        hit.collider.GetComponent<Boss>().TakeDamage((int)(m_baseDamage * m_stats.m_damageMultiplier));
                    }
                }
            }

            m_isAttacking = true;
            m_return = false;
            m_rotation = 0.0f;

            m_rotationIncrease = 2.0f / (m_stats.m_attackDelay / Time.fixedDeltaTime);
        }
    }
}