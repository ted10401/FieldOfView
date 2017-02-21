using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private Transform _target;
    private Transform _transform;
    private Camera _camera;
    private Vector3 _position;

    private void Awake()
    {
        _transform = transform;
        _camera = Camera.main;
    }

    private void Update()
    {
        _transform.Translate(_transform.forward * Input.GetAxis("Vertical") * _moveSpeed * Time.deltaTime, Space.World);

        _position = _camera.ScreenToWorldPoint(Input.mousePosition);
        _position.y = 0;
        _target.position = _position;
        _transform.LookAt(_target);
    }
}
