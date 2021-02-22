using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField]
    GameObject m_bulletPrefab;

    [SerializeField]
    Transform m_bulletSpawn;

    [SerializeField]
    float m_reloadDelay;

    int m_ammoCount;

    [SerializeField]
    int m_maxAmmo;

    bool m_isReloading = false;

    [SerializeField]
    bool m_isPlayerWeapon;

    PlayerUI m_playerUI = null;

    public override void Initialize()
    {
        m_ammoCount = m_maxAmmo;

        if(m_isPlayerWeapon)
        {
            m_playerUI = FindObjectOfType<PlayerUI>();
            m_playerUI.EnableAmmoText();
            m_playerUI.UpdateAmmoText(m_ammoCount, m_maxAmmo);
        }
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

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 bulletDir = hit.point - m_bulletSpawn.position;

                GameObject bullet = Instantiate(m_bulletPrefab, m_bulletSpawn.position, m_bulletPrefab.transform.rotation);

                bullet.GetComponent<Projectal>().Fire(bulletDir, m_stats.m_damageMultiplier);

                StartCoroutine(GunCoolDown());
            }
        }
    }
}
