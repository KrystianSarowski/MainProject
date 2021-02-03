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

    float m_attackDelay = 0.5f;

    int m_health = 100;

    [SerializeField]
    Transform m_attackHitBox;

    //The rigid body of the player.
    Rigidbody m_rb;

    [SerializeField]
    GameObject m_sword;

    PlayerUI m_playerUI;

    bool m_isFalling;
    bool m_isAttacking;

    //Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();

        GameObject camera = FindObjectOfType<CameraController>().gameObject;

        camera.transform.parent = gameObject.transform;

        camera.GetComponent<CameraController>().AttachPlayer();

        m_sword.transform.SetParent(camera.transform);

        m_playerUI = GetComponent<PlayerUI>();
    }

    //Update is called once per frame
    void Update()
    {
        Move();
        Jump();

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(!m_isAttacking)
            {
                StartCoroutine(attack());
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

    public void TakeDamage(int t_incomingDamage)
    {
        m_health -= t_incomingDamage;

        if(m_health < 0)
        {
            m_health = 0;
            GameplayManager.LoadScene("GameoverScene");
        }

        m_playerUI.UpdateHealthBar(m_health);
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

    IEnumerator attack()
    {
        m_isAttacking = true;

        m_sword.GetComponent<Sword>().StartAttack();

        Vector3 hitBoxSize = new Vector3(0.6f, 1.0f, 1.0f);

        Collider[] hitColliders = Physics.OverlapBox(m_attackHitBox.position, hitBoxSize / 2, m_attackHitBox.rotation);

        for(int i = 0; i < hitColliders.Length; i++)
        {
            if(hitColliders[i].tag == "Enemy")
            {
               hitColliders[i].GetComponent<Enemy>().PushBack(transform.position);
               hitColliders[i].GetComponent<Enemy>().TakeDamage(10);
            }
        }

        yield return new WaitForSeconds(m_attackDelay);

        m_isAttacking = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            m_isFalling = false;
        }
    }
}