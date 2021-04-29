using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//@Author Krystian Sarowski

public enum EmissionShape
{
    Cone,
    Cube,
    Sphere
}

[System.Serializable]
public struct ConeEmission
{
    public float m_maxRadius;
    public float m_minRadius;
    public float m_angle;

    public Vector3 m_direction;
}

[System.Serializable]
public struct CubeEmission
{
    public Vector3 m_rotation;
    public Vector3 m_scale;
}

[System.Serializable]
public struct SphereEmission
{
    public float m_maxRadius;
    public float m_minRadius;
}

public class ParticleSystem : MonoBehaviour
{
    [SerializeField]
    GameObject m_pariclePrefab;

    [SerializeField]
    float m_initialSpeed;

    [SerializeField]
    int m_maxParticleCount;

    [HideInInspector]
    public bool m_isContinues;

    [SerializeField]
    bool m_isRelative;

    [HideInInspector]
    public float m_particleDelay;

    [SerializeField]
    bool m_isActive;

    [HideInInspector]
    public bool m_destoryAfterDelay;

    [HideInInspector]
    public float m_timeToLive;

    [SerializeField]
    ParticleData m_particleData;

    [SerializeField]
    EmissionShape m_emissionShape;

    [SerializeField]
    ConeEmission m_coneEmission;

    [SerializeField]
    CubeEmission m_cubeEmission;

    [SerializeField]
    SphereEmission m_sphereEmission;

    float m_currentTime = 0.0f;

    int m_currentParticleCount = 0;

    void Awake()
    {
        if(m_isActive)
        {
            StartSystem();
        }

        if(m_destoryAfterDelay)
        {
            Destroy(gameObject, m_timeToLive);
        }
    }

    public void StartSystem()
    {
        if(!m_isContinues)
        {
            EmitParticles(m_maxParticleCount);
        }
        else
        {
            EmitParticles(1);
            m_currentTime = 0.0f;
        }

        m_isActive = true;
    }

    public void StopSystem()
    {
        m_isActive = false;
    }

    void FireParticle(Vector3 t_direction, Vector3 t_position)
    {
        GameObject particle = Instantiate(m_pariclePrefab, t_position, m_pariclePrefab.transform.rotation);

        particle.GetComponent<Rigidbody>().AddForce(t_direction * m_initialSpeed, ForceMode.Impulse);
        particle.GetComponent<Particle>().SetData(m_particleData, this);
    }

    void ConeEmission(int t_numOfParticles)
    {
        Vector3 innerAxis;
        Vector3 emissionDir = m_coneEmission.m_direction;

        if(m_isRelative)
        {
            emissionDir = transform.TransformDirection(m_coneEmission.m_direction);
        }

        if (m_coneEmission.m_direction.normalized != Vector3.forward && m_coneEmission.m_direction.normalized != Vector3.back)
        {
            innerAxis = Vector3.Cross(emissionDir, Vector3.forward).normalized;
        }
        else
        {
            innerAxis = Vector3.Cross(emissionDir, Vector3.left).normalized;
        }  

        Vector3 dirToOuterAxis = Quaternion.AngleAxis(m_coneEmission.m_angle, innerAxis) * emissionDir;

        Vector3 outerAxis = (transform.position + dirToOuterAxis) - (transform.position + emissionDir);
        outerAxis = Quaternion.AngleAxis(90, emissionDir) * outerAxis;

        for (int i = 0; i < t_numOfParticles; i++)
        {
            Quaternion angle = Quaternion.AngleAxis(Random.Range(0, 360), emissionDir);

            float offset = Random.Range(m_coneEmission.m_minRadius, m_coneEmission.m_maxRadius);

            Vector3 position = transform.position + (angle * innerAxis) * offset;

            Vector3 targetPos = (angle * outerAxis) + position + emissionDir;

            Vector3 travelDir = (targetPos - position).normalized;      

            FireParticle(travelDir, position);
        }
    }

    void CubeEmission(int t_numOfParticles)
    {
        for (int i = 0; i < t_numOfParticles; i++)
        {
            Vector3 position = Vector3.zero;

            position.x += (Random.value - 0.5f) * Random.Range(0, m_cubeEmission.m_scale.x);
            position.y += (Random.value - 0.5f) * Random.Range(0, m_cubeEmission.m_scale.y);
            position.z += (Random.value - 0.5f) * Random.Range(0, m_cubeEmission.m_scale.z);

            Vector3 travelDir = position.normalized;

            if(m_isRelative)
            {
                travelDir = transform.TransformDirection(travelDir);
                position = transform.TransformPoint(position);
            }
            else
            {
                position += transform.position;
            }

            FireParticle(travelDir, position);
        }
    }

    void SphereEmission(int t_numOfParticles)
    {
        for (int i = 0; i < t_numOfParticles; i++)
        {
            Vector3 position = transform.position + Random.onUnitSphere * Random.Range(m_sphereEmission.m_minRadius, m_sphereEmission.m_maxRadius);
            Vector3 travelDir = (position - transform.position).normalized;

            FireParticle(travelDir, position);
        }
    }

    void EmitParticles(int t_numOfParticles)
    {
        switch(m_emissionShape)
        {
            case EmissionShape.Cone:
                ConeEmission(t_numOfParticles);
                break;
            case EmissionShape.Cube:
                CubeEmission(t_numOfParticles);
                break;
            case EmissionShape.Sphere:
                SphereEmission(t_numOfParticles);
                break;
            default:
                break;
        }
    }

    public void AddParticle()
    {
        m_currentParticleCount++;
    }

    public void RemoveParticle()
    {
        m_currentParticleCount--;
    }

    void Update()
    {
        if(m_isActive && m_isContinues)
        {
            m_currentTime += Time.deltaTime;

            if(m_currentTime >= m_particleDelay)
            {
                if(m_currentParticleCount < m_maxParticleCount)
                {
                    EmitParticles(1);
                    m_currentTime = 0.0f;
                }
            }
        }
    }
}

[CustomEditor(typeof(ParticleSystem))]
public class RandomScript_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ParticleSystem particleSystem = (ParticleSystem)target;

        particleSystem.m_isContinues = EditorGUILayout.Toggle("Is Continues", particleSystem.m_isContinues);

        if(particleSystem.m_isContinues)
        {
            particleSystem.m_particleDelay = EditorGUILayout.FloatField("Particle Delay", particleSystem.m_particleDelay);
        }

        particleSystem.m_destoryAfterDelay = EditorGUILayout.Toggle("Destroy After Delay", particleSystem.m_destoryAfterDelay);

        if (particleSystem.m_destoryAfterDelay)
        {
            particleSystem.m_timeToLive = EditorGUILayout.FloatField("Time To Live", particleSystem.m_timeToLive);
        }
    }
}