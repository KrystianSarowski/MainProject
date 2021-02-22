using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0.1f, 100.0f)]
    public float m_speed = 50.0f;       //The speed at which the player moves in a certain direction.

    [Range(0.1f, 10.0f)]
    public float m_maxSpeed = 5.0f;     //The max speed at which the player can travel.

    [Range(0.1f, 5.0f)]
    public float m_jumpVelocity = 3.5f;

    //The rigid body of the player.
    Rigidbody m_rb;

    [SerializeField]
    string m_weaponName;

    Weapon m_weapon;

    PlayerUI m_playerUI;

    bool m_isFalling;

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
        GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/" + m_weaponName);

        GameObject weaponObject = Instantiate(weaponPrefab, transform.position + weaponPrefab.GetComponent<Weapon>().m_spawnOffset, weaponPrefab.transform.rotation);

        weaponObject.transform.SetParent(t_camera.transform);

        m_weapon = weaponObject.GetComponent<Weapon>();
        m_weapon.Initialize();
    }

    //Update is called once per frame
    void Update()
    {
        Move();
        Jump();

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
                break;
            case "AttackSpeed":
                m_weapon.DecreaseAttackDelay();
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
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Pickup")
        {
            ApplyPickup(other.GetComponent<Pickup>().GetName());

            Destroy(other.gameObject);
        }
    }
}