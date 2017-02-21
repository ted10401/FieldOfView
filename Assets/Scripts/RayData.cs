using UnityEngine;

public class RayData
{
    public float m_distance;
    public float m_angle;
    public Vector3 m_direction;
    public Vector3 m_end;
    public Collider m_hitCollider;
    public bool m_hit;

    public RayData(float angle, float distance)
    {
        m_distance = distance;

        UpdateDirection(angle);
    }

    public void UpdateDirection(float angle)
    {
        m_angle += angle;
        m_direction = DirFromAngle(m_angle);
        m_end = m_direction * m_distance;
    }

    private Vector3 DirFromAngle(float angle)
    {
        float pi = Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(angle * pi), 0, Mathf.Cos(angle * pi));
    }

    public static bool IsHittingSameObject(RayData data1, RayData data2)
    {
        return data1.m_hitCollider == data2.m_hitCollider;
    }
}