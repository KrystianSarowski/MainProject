using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public class Gun : Weapon
{
    [SerializeField]
    GameObject m_bulletPrefab;

    [SerializeField]
    Transform m_bulletSpawn;

    [SerializeField]
    float m_reloadDelay;

    [SerializeField]
    int m_pelletCount;

    int m_ammoCount;

    [SerializeField]
    int m_maxAmmo;

    bool m_isReloading = false;

    [SerializeField]
    bool m_isPlayerWeapon;

    PlayerUI m_playerUI = null;

    ParticleSystem m_particleSystem;

    public override void Initialize()
    {
        base.Initialize();

        m_ammoCount = m_maxAmmo;

        if(m_isPlayerWeapon)
        {
            m_playerUI = FindObjectOfType<PlayerUI>();
            m_playerUI.EnableAmmoText(true);
            m_playerUI.UpdateAmmoText(m_ammoCount, m_maxAmmo);
        }

        m_particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    IEnumerator GunCoolDown()
    {
        m_ammoCount--;

        if (m_isPlayerWeapon)
        {
            m_playerUI.UpdateAmmoText(m_ammoCount, m_maxAmmo);
        }

        yield return new WaitForSeconds(m_stats.m_attackDelay);

        m_isAttacking = false;

        if (m_ammoCount == 0)
        {
            m_isReloading = true;

            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(m_reloadDelay);

        m_isReloading = false;

        m_ammoCount = m_maxAmmo;

        if (m_isPlayerWeapon)
        {
            m_playerUI.UpdateAmmoText(m_ammoCount, m_maxAmmo);
        }
    }

    public override void Attack()
    {
        if (!m_isAttacking && !m_isReloading)
        {
            m_isAttacking = true;

            if(m_pelletCount == 1)
            {
                Vector3 bulletDir = transform.TransformDirection(Vector3.left);

                GameObject bullet = Instantiate(m_bulletPrefab, m_bulletSpawn.position, m_bulletPrefab.transform.rotation);

                bullet.GetComponent<Projectal>().Fire(bulletDir, m_stats.m_damageMultiplier);
            }
            else
            {
                FirePellets();
            }

            StartCoroutine(GunCoolDown());

            if (m_particleSystem != null)
            {
                m_particleSystem.StartSystem();
            }
        }
    }

    void FirePellets()
    {
        for(int i = 0; i < m_pelletCount; i++)
        {
            Vector3 bulletDir = Vector3.left;

            bulletDir.y = Random.Range(-0.3f, 0.3f);
            bulletDir.z = Random.Range(-0.3f, 0.3f);

            bulletDir = transform.TransformDirection(bulletDir);

            GameObject bullet = Instantiate(m_bulletPrefab, m_bulletSpawn.position, m_bulletPrefab.transform.rotation);

            bullet.GetComponent<Projectal>().Fire(bulletDir, m_stats.m_damageMultiplier);
        }
    }
}