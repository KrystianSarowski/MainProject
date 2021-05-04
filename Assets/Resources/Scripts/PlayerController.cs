using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

[System.Serializable]
public class UpgradeLevels
{
    public int m_attackSpeedLevel = 0;
    public int m_damageMultiplierLevel = 0;
    public int m_maxHealthLevel = 0;
    public int m_maxSpeedLevel = 0;
}

public class PlayerController : MonoBehaviour
{
    [Range(0.1f, 100.0f)]
    [SerializeField]
    float m_speed = 50.0f;       //The speed at which the player moves in a certain direction.

    [Range(0.1f, 10.0f)]
    [SerializeField]
    float m_baseMaxSpeed = 5.0f;     //The max speed at which the player can travel.

    [Range(0.1f, 5.0f)]
    [SerializeField]
    float m_jumpVelocity = 3.5f;

    float m_interactRange = 1.0f;
    float m_maxSpeed = 5.0f;
    float m_maxSpeedIncrease = 1.0f;

    bool m_isFalling;
    bool m_haxMode = false;                 //Bool for if the play can be killed or not used for testing.

    //The rigid body of the player.
    Rigidbody m_rb;

    Weapon m_weapon;

    UpgradeLevels m_upgradeLevels = new UpgradeLevels();

    PlayerUI m_playerUI;

    //Start is called before the first frame update
    void Start()
    {
        m_maxSpeed = m_baseMaxSpeed;

        m_rb = GetComponent<Rigidbody>();
        m_playerUI = GetComponent<PlayerUI>();

        m_playerUI.Initialize();

        InitializeCamera();
    }

    /// <summary>
    /// Setups the Main Camera within the scene so that it is able to follow
    /// the player around. It then calls the initalize weapon to move with camera.
    /// </summary>
    void InitializeCamera()
    {
        GameObject camera = FindObjectOfType<CameraController>().gameObject;

        camera.transform.parent = gameObject.transform;

        camera.GetComponent<CameraController>().AttachPlayer();

        InitializeWeapon(camera);
    }

    /// <summary>
    /// Loads up the weapon based on the name of the currently held weapon by the
    /// Player, it then attaches to the camera and calls apply upgrades in order to reapply
    /// any active weapon upgrades.
    /// </summary>
    /// <param name="t_camera"></param>
    void InitializeWeapon(GameObject t_camera)
    {
        if(m_weapon != null)
        {
            Destroy(m_weapon.gameObject);
        }

        GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/" + PlayerStats.GetWeaponName());

        GameObject weaponObject = Instantiate(weaponPrefab, transform.position + weaponPrefab.GetComponent<Weapon>().m_spawnOffset, weaponPrefab.transform.rotation);

        weaponObject.transform.SetParent(t_camera.transform);

        if (FindObjectOfType<GameplayManager>().GetCurrentLevel() != 1)
        {
            m_upgradeLevels = DataLoad.LoadUpgradeData("Upgrades");
        }

        m_weapon = weaponObject.GetComponent<Weapon>();
        m_weapon.Initialize();
        ApplyUpgrades();
    }

    void Update()
    {
        if(GameplayManager.s_isPaused)
        {
            return;
        }

        if (PlayerStats.s_health <= 0 && !m_haxMode)
        {
            GameplayManager.LoadScene("GameoverScene");
        }

        Move();
        Jump();
        Interact();
        CheckForInput();

        m_playerUI.UpdateHealthBar(PlayerStats.s_health);
    }

    void CheckForInput()
    {
        if (Input.GetMouseButton(0))
        {
            m_weapon.Attack();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameplayManager.LoadScene("MenuScene");
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            m_haxMode = !(m_haxMode);
        }
    }

    /// <summary>
    /// Creates a ray at the centre of the screen and fires it forward.
    /// If the object with which the ray collides is within the interaction distance
    /// and it is interactable an approperate reaction will be taken.
    /// </summary>
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
            velocity = velocity.normalized * m_baseMaxSpeed;
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

    /// <summary>
    /// Applies the pickup to the Player.
    /// If the pickup is a health up then the Player is healed.
    /// If the pickup is an upgrade that upgrades level is increased and
    /// all the upgrades are reapplied and save to file.
    /// </summary>
    /// <param name="t_name">The name of the pickup that the Player has collected</param>
    void ApplyPickup(string t_name)
    {
        switch (t_name)
        {
            case "Heart":
                PlayerStats.DealDamage(-20);
                break;
            case "DamageUp":
                m_upgradeLevels.m_damageMultiplierLevel++;
                ApplyUpgrades();
                break;
            case "AttackSpeed":
                m_upgradeLevels.m_attackSpeedLevel++;
                ApplyUpgrades();
                break;
            case "MovementSpeed":
                m_upgradeLevels.m_maxSpeedLevel++;
                ApplyUpgrades();
                break;
            case "MaxHealth":
                m_upgradeLevels.m_maxSpeedLevel++;
                ApplyUpgrades();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Reapplies all the upgrades to the PlayerStats and other variables and saves it
    /// to file so that when the Player progresses to next level the upgrades are reapplied.
    /// </summary>
    void ApplyUpgrades()
    {
        m_weapon.IncreaseDamageMultiplier(m_upgradeLevels.m_damageMultiplierLevel);
        m_weapon.DecreaseAttackDelay(m_upgradeLevels.m_attackSpeedLevel);
        PlayerStats.IncreaseMaxHealth(m_upgradeLevels.m_maxHealthLevel);

        UpdateMaxSpeed();

        DataSave.SaveUpgradeData(m_upgradeLevels, "Upgrades");
    }

    void UpdateMaxSpeed()
    {
        m_maxSpeed = m_baseMaxSpeed + (m_maxSpeedIncrease * m_upgradeLevels.m_maxSpeedLevel);
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
                PlayerStats.SetWeaponName(t_itemName);
                InitializeCamera();
                break;
        }
    }
}