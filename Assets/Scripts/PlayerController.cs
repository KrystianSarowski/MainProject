using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0.1f, 100.0f)]
    public float m_speed = 50.0f;       //The speed at which the player moves in a certain direction.

    [Range(0.1f, 10.0f)]
    public float m_maxSpeed = 5.0f;     //The max speed at which the player can travel.

    //The rigid body of the player.
    private Rigidbody m_rb;

    [Range(0.1f, 5.0f)]
    public float m_jumpVelocity = 3.5f;

    bool m_isFalling;

    //Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();

        GameObject camera = FindObjectOfType<CameraController>().gameObject;

        camera.transform.parent = gameObject.transform;

        camera.GetComponent<CameraController>().AttachPlayer();
    }

    //Update is called once per frame
    void Update()
    {
        Move();
        Jump();
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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            m_isFalling = false;
        }
    }
}