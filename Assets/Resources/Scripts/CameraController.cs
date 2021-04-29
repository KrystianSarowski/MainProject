using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

public class CameraController : MonoBehaviour
{
    Vector2 m_mousePosition;            //The position of the mouse on screen.
    Vector2 m_changeInMousePosition;    //The vector for the change in the position of the mouse.

    [Range(0.1f, 10.0f)]
    public float m_sensetiivity = 5.0f; //The speed at which the mouse moves on screen.

    [Range(0.1f, 10.0f)]
    public float m_smoothing = 2.0f;    //The smoothing of the mouse movement multiplier.

    //Refernce to player gameobject that is controlling the camera.
    GameObject m_player;

    public void AttachPlayer()
    {
        m_player = transform.parent.gameObject;

        transform.rotation = Quaternion.identity;
        transform.localPosition = new Vector3(0, 0.4f, 0);
    }

    //Update is called once per frame
    void LateUpdate()
    {
        if (GameplayManager.s_isPaused)
        {
            return;
        }

        if (m_player != null)
        {
            Vector2 newChangeVector = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            newChangeVector = newChangeVector * m_sensetiivity * m_smoothing;

            m_changeInMousePosition.x = Mathf.Lerp(m_changeInMousePosition.x, newChangeVector.x, 1 / m_sensetiivity);
            m_changeInMousePosition.y = Mathf.Lerp(m_changeInMousePosition.y, newChangeVector.y, 1 / m_sensetiivity);

            m_mousePosition += m_changeInMousePosition;

            m_mousePosition.y = Mathf.Clamp(m_mousePosition.y, -90f, 90f);

            transform.localRotation = Quaternion.AngleAxis(-m_mousePosition.y, Vector3.right);

            m_player.transform.localRotation = Quaternion.AngleAxis(m_mousePosition.x, m_player.transform.up);
        }
    }
}