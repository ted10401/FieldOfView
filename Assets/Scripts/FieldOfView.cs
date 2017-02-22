using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FieldOfViewType
{
    Original,
    Normal,
    Approximation,
    Bisection,
}

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private FieldOfViewType _fieldOfViewType;
    [SerializeField] protected float _radius;
    [SerializeField] protected float _angle = 45;
    [SerializeField] protected int _divide = 2;
    [SerializeField] protected float _approximationPrecision = 0.01f;
    [SerializeField] protected int _bisectionCount = 10;

    private RaycastHit _hit;

    public RayData[] GetRayDatas()
    {
        RayData[] datas = null;

        switch (_fieldOfViewType)
        {
            case FieldOfViewType.Original:
                datas = GetOriginalDatas();
                break;
            case FieldOfViewType.Normal:
                datas = GetNormalDatas();
                break;
            case FieldOfViewType.Approximation:
                datas = GetApproximationDatas();
                break;
            case FieldOfViewType.Bisection:
                datas = GetBisectionDatas();
                break;
        }

        return datas;
    }

    private RayData[] GetOriginalDatas()
    {        
        RayData[] rayDatas = new RayData[_divide + 1];

        Vector3 center = transform.position;
        float startAngle = transform.eulerAngles.y -_angle / 2;
        float angle = _angle / _divide;
        RayData rayDataCache = null;

        for(int i = 0; i <= _divide; i++)
        {
            rayDataCache = new RayData(center, startAngle + angle * i, _radius);

            rayDatas[i] = rayDataCache;
        }

        return rayDatas;
    }

    private RayData[] GetNormalDatas()
    {        
        RayData[] rayDatas = GetOriginalDatas();

        for (int i = 0; i < rayDatas.Length; i++)
        {
            UpdateRaycast(rayDatas[i]);
        }

        return rayDatas;
    }

    private void UpdateRaycast(RayData rayData)
    {
        rayData.m_hit = Physics.Raycast(transform.position, rayData.m_direction, out _hit, _radius);

        if (rayData.m_hit)
        {
            rayData.m_hitCollider = _hit.collider;
            rayData.m_end = _hit.point;
        }
        else
        {
            rayData.m_hitCollider = null;
            rayData.m_end = rayData.m_start + rayData.m_direction * _radius;
        }
    }

    private RayData[] GetApproximationDatas()
    {
        List<RayData> rayDatas = new List<RayData>(GetNormalDatas());
        EdgeData edgeData = null;

        for (int i = 0; i < rayDatas.Count - 1; i++)
        {
            edgeData = GetApproximationEdge(rayDatas[i], rayDatas[i + 1]);

            if (edgeData != null && edgeData.m_firstRay != null && edgeData.m_secondRay != null)
            {
                rayDatas.Insert(i + 1, edgeData.m_firstRay);
                rayDatas.Insert(i + 2, edgeData.m_secondRay);
                ++i;
            }
        }

        return rayDatas.ToArray();
    }

    private EdgeData GetApproximationEdge(RayData startEdgeRayData, RayData endEdgeRayData)
    {
        if (_approximationPrecision <= 0)
        {
            return null;
        }

        Vector3 center = transform.position;
        float maxAngle = Vector3.Angle(startEdgeRayData.m_direction, endEdgeRayData.m_direction);
        float curAngle = _approximationPrecision;

        RayData edgeRayData = new RayData(center, startEdgeRayData.m_angle + _approximationPrecision, _radius);
        UpdateRaycast(edgeRayData);

        while (RayData.IsHittingSameObject(startEdgeRayData, edgeRayData))
        {
            curAngle += _approximationPrecision;

            if (curAngle > maxAngle)
            {
                edgeRayData = null;
                break;
            }

            edgeRayData.UpdateDirection(_approximationPrecision);
            UpdateRaycast(edgeRayData);
        }

        if (edgeRayData == null)
        {
            return null;
        }

        EdgeData edgeData = new EdgeData();
        edgeData.m_secondRay = edgeRayData;
        edgeData.m_firstRay = new RayData(center, edgeRayData.m_angle - _approximationPrecision, _radius);
        UpdateRaycast(edgeData.m_firstRay);

        return edgeData;
    }

    private RayData[] GetBisectionDatas()
    {
        List<RayData> rayDatas = new List<RayData>(GetNormalDatas());
        EdgeData edgeData = new EdgeData();

        for (int i = 0; i < rayDatas.Count - 1; i++)
        {
            edgeData = GetBisectionEdge(rayDatas[i], rayDatas[i + 1]);

            if (edgeData != null && edgeData.m_firstRay != null && edgeData.m_secondRay != null)
            {
                rayDatas.Insert(i + 1, edgeData.m_firstRay);
                rayDatas.Insert(i + 2, edgeData.m_secondRay);
                ++i;
            }
        }

        return rayDatas.ToArray();
    }

    private EdgeData GetBisectionEdge(RayData startEdgeRayData, RayData endEdgeRayData)
    {
        if (!startEdgeRayData.m_hit && !endEdgeRayData.m_hit)
        {
            return GetApproximationEdge(startEdgeRayData, endEdgeRayData);
        }

        if (RayData.IsHittingSameObject(startEdgeRayData, endEdgeRayData))
        {
            return null;
        }

        Vector3 center = transform.position;
        EdgeData edgeData = new EdgeData();
        float angle = 0;
        RayData edgeRayData = null;

        for (int i = 0; i < _bisectionCount; i++)
        {
            angle = (startEdgeRayData.m_angle + endEdgeRayData.m_angle) / 2;
            edgeRayData = new RayData(center, angle, _radius);
            UpdateRaycast(edgeRayData);

            if (RayData.IsHittingSameObject(startEdgeRayData, edgeRayData))
            {
                startEdgeRayData = edgeRayData;
            }
            else
            {
                endEdgeRayData = edgeRayData;
            }
        }

        edgeData.m_firstRay = startEdgeRayData;
        edgeData.m_secondRay = endEdgeRayData;

        return edgeData;
    }
}
