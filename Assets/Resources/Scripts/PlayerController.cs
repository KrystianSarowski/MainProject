using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0.1f, 100.0f)]
    [SerializeField]
    float m_speed = 50.0f;       //The speed at which the player moves in a certain direction.

    [Range(0.1f, 10.0f)]
    [SerializeField]
    float m_maxSpeed = 5.0f;     //The max speed at which the player can travel.

    [Range(0.1f, 5.0f)]
    [SerializeField]
    float m_jumpVelocity = 3.5f;

    [SerializeField]
    string m_weaponName;

    float m_interactRange = 1.0f;

    bool m_isFalling;

    //The rigid body of the player.
    Rigidbody m_rb;

    Weapon m_weapon;

    PlayerUI m_playerUI;

    //Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_playerUI = GetComponent<PlayerUI>();

        m_playerUI.Initialize();

        InitializeCamera();
    }

    void InitializeCamera()
    {
        GameObject camera = FindObjectOfType<CameraController>().gameObject;

        camera.transform.parent = gameObject.transform;

        camera.GetComponent<CameraController>().AttachPlayer();

        InitializeWeapon(camera);
    }

    void InitializeWeapon(GameObject t_camera)
    {
        if(m_weapon != null)
        {
            Destroy(m_weapon.gameObject);
        }

        GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/" + m_weaponName);

        GameObject weaponObject = Instantiate(weaponPrefab, transform.position + weaponPrefab.GetComponent<Weapon>().m_spawnOffset, weaponPrefab.transform.rotation);

        weaponObject.transform.SetParent(t_camera.transform);

        m_weapon = weaponObject.GetComponent<Weapon>();
        m_weapon.Initialize();
        m_weapon.SaveWeaponStats();
    }

    //Update is called once per frame
    void Update()
    {
        if(GameplayManager.s_isPaused)
        {
            return;
        }

        Move();
        Jump();
        Interact();

        if(Input.GetMouseButton(0))
        {
            m_weapon.Attack();
        }

        if(PlayerStats.s_health <= 0)
        {
            GameplayManager.LoadScene("GameoverScene");
        }

        m_playerUI.UpdateHealthBar(PlayerStats.s_health);
    }

    void Interact()
    { 
        if(Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.distance <= m_interactRange)
                {
                    switch (hit.collider.tag)
                    {
                        case "Shopkeeper":
                            hit.collider.GetComponent<Shopkeeper>().Interact();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            m_rb.velocity += transform.rotation * Vector3.forward * m_speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            m_rb.velocity += transform.rotation * Vector3.back * (m_speed * 0.5f) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            m_rb.velocity += transform.rotation * Vector3.left * m_speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            m_rb.velocity += transform.rotation * Vector3.right * m_speed * Time.deltaTime;
        }

        Vector2 velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.z);

        if (velocity.magnitude > m_maxSpeed)
        {
            velocity = velocity.normalized * m_maxSpeed;
            m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.y);
        } 
    }

    void Jump()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if(!m_isFalling)
            {
                m_rb.velocity += Vector3.up * m_jumpVelocity;
                m_isFalling = true;
            }
        }
    }

    void ApplyPickup(string t_name)
    {
        switch (t_name)
        {
            case "Heart":
                PlayerStats.DealDamage(-20);
                break;
            case "DamageUp":
                m_weapon.IncreaseDamageMultiplier();
                m_weapon.SaveWeaponStats();
                break;
            case "AttackSpeed":
                m_weapon.DecreaseAttackDelay();
                m_weapon.SaveWeaponStats();
                break;
            default:
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            m_isFalling = false;
        }
        else if(collision.gameObject.tag == "Coin")
        {
            PlayerStats.IncreaseGold(10);
            Destroy(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Pickup")
        {
            ApplyPickup(other.GetComponent<Pickup>().GetName());

            Destroy(other.gameObject);
        }
    }

    public void UsePurchasedItem(ShopItemType t_itemType, string t_itemName)
    {
        switch(t_itemType)
        {
            case ShopItemType.Heal:
                ApplyPickup("Heart");
                break;
            case ShopItemType.Poweup:
                ApplyPickup(t_itemName);
                break;
            case ShopItemType.Weapon:
                m_weaponName = t_itemName;
                InitializeCamera();
                break;
        }
    }

    public string GetWeaponName()
    {
        return m_weaponName;
    }
}