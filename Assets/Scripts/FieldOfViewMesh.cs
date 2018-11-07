using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewMesh : FieldOfView
{
    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmo = false;
    [SerializeField] private float _hitScale = 0.1f;
    [SerializeField] private Color _gizmoColor = Color.red;

    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private int[] _triangles;
    private RayData[] _rayDatas;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        UpdateMesh();
    }

    private void LateUpdate()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        _rayDatas = GetRayDatas();

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        int meshCount = _rayDatas.Length - 1;
        int vertexCount = meshCount * 2 + 1;
        int triangleCount = meshCount * 3;

        _vertices = new Vector3[vertexCount];
        _vertices[0] = Vector3.zero;
        for (int i = 1, mesh = 0; i < _vertices.Length; i += 2, mesh++)
        {
            _vertices[i] = transform.InverseTransformPoint(_rayDatas[mesh].m_end);
            _vertices[i + 1] = transform.InverseTransformPoint(_rayDatas[mesh + 1].m_end);
        }

        _triangles = new int[triangleCount];
        for (int i = 0; i < meshCount; i++)
        {
            _triangles[i * 3] = 0;
            _triangles[i * 3 + 1] = i * 2 + 1;
            _triangles[i * 3 + 2] = i * 2 + 2;
        }

        _uvs = new Vector2[vertexCount];
        _uvs[0] = new Vector2(0.5f, 0.5f);
        float lerp = 0;
        Vector3 direction = Vector3.zero;
        for (int i = 1, mesh = 0; i < _uvs.Length; i += 2, mesh++)
        {
            lerp = _vertices[i].magnitude / _radius;
            direction = _rayDatas[mesh].m_direction * _rayDatas[mesh].m_distance * 0.6f / _radius;
            _uvs[i] = new Vector2(direction.x, direction.z) * lerp + _uvs[0];

            lerp = _vertices[i + 1].magnitude / _radius;
            direction = _rayDatas[mesh + 1].m_direction * _rayDatas[mesh].m_distance * 0.6f / _radius;
            _uvs[i + 1] = new Vector2(direction.x, direction.z) * lerp + _uvs[0];
        }

        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        _mesh.RecalculateNormals();

        _meshFilter.mesh = _mesh;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawGizmo)
        {
            return;
        }

        Vector3 center = transform.position;

        RayData[] datas = GetRayDatas();
        RayData cacheData = datas[0];

        Handles.color = _gizmoColor;
        Handles.DrawSolidArc(center, transform.up, transform.forward, 360, _hitScale * 2);

        for (int i = 0; i < datas.Length; i++)
        {
            cacheData = datas[i];
            Debug.DrawLine(center, cacheData.m_end, _gizmoColor);
            Handles.DrawSolidArc(cacheData.m_end, transform.up, transform.forward, 360, _hitScale);
        }
    }
    #endif
}
