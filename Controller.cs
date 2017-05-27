using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private const float SkinWidth = .02f;
    private const int TotalHorizontalRays = 8;
    private const int TotalVerticalRays = 8;

    public LayerMask PlatformMask;

    public Parameters _parameters;
    public ControllerState _state { get; private set; }
    public Vector2 Velocity { get { return _velocity; } }
    public bool HandleCollisions { get; set; }

    private Vector2 _velocity;
    private Transform _transform; // position
    private Vector3 _localScale;    //scale
    private BoxCollider2D _boxCollider; //collider

    private Vector2 _boxright;
    private Vector2 _boxup;

    private Vector2 _controllervelocity;
    private Vector3 _rotateto;
    private Vector2 _rotationpoint;

    private Vector3
       _raycastTopLeft,
       _raycastBottomRight,
       _raycastBottomLeft;

    private float   //the distance between the rays on each side, calculated in Awake
       _verticalDistanceBetweenRays,
       _horizontalDistanceBetweenRays;

    private float[] _possibleSlopes = { -1, -0.5f, 0, 0.5f, 1 };

    void Awake()
    {
        HandleCollisions = true;
        _state = new ControllerState();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = GetComponent<BoxCollider2D>();

        _velocity = new Vector2(0, 0);
        _controllervelocity = new Vector2(0, 0);
        _boxup = transform.up;
        _boxright = Vector3.Cross(Vector3.forward, _boxup);

        var colliderWidth = _boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2 * SkinWidth);
        _horizontalDistanceBetweenRays = colliderWidth / (TotalVerticalRays - 1);

        var colliderHeight = _boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2 * SkinWidth);
        _verticalDistanceBetweenRays = colliderHeight / (TotalHorizontalRays - 1);
    }

    public void SetHorizontalForce(float x)
    {
        _controllervelocity = x * _boxright;
    }

    public void LateUpdate()
    {

    }

    private void Rotate()
    {
        var rayDistance = .75f * _boxCollider.size.y * Mathf.Abs(_localScale.y);
        float[] hitDist = { -1, -1, -1};

        var rayVector = (Vector2)_raycastBottomLeft;
        var raycastHit = Physics2D.Raycast(rayVector, _boxup, rayDistance, PlatformMask);
        
        for (var i = 0; i < 3; i++)
        {
            rayVector = (Vector2)_raycastBottomLeft + (i / 2 * _boxCollider.size.y * Mathf.Abs(_localScale.y)) * (Vector2)_boxright;

            raycastHit = Physics2D.Raycast(rayVector, -_boxup, rayDistance, PlatformMask);
            if (raycastHit)
            {
                hitDist[i] = -raycastHit.distance;
            }
        }

        rayVector = (Vector2)_raycastBottomLeft;
        raycastHit = Physics2D.Raycast(rayVector, _boxup, rayDistance, PlatformMask);
        if (raycastHit)
        {
            hitDist[0] = raycastHit.distance;
        }
        rayVector = (Vector2)_raycastBottomRight;
        raycastHit = Physics2D.Raycast(rayVector, _boxup, rayDistance, PlatformMask);
        if (raycastHit)
        {
            hitDist[2] = raycastHit.distance;
        }

        float leftSlope = (hitDist[1] - hitDist[0]) / (1 / 2 * _boxCollider.size.y * Mathf.Abs(_localScale.y));
        float rightSlope = (hitDist[2] - hitDist[1]) / (1 / 2 * _boxCollider.size.y * Mathf.Abs(_localScale.y));

        if (hitDist[1] == -1 && (hitDist[0] != -1 || hitDist[2] != -1))
        {
            if (hitDist[0] != -1)
            {
                rayVector = (Vector2)_raycastBottomLeft - SkinWidth * (Vector2)_boxright;

                raycastHit = Physics2D.Raycast(rayVector, -_boxup, rayDistance, PlatformMask);
                if (raycastHit)
                {
                    float outHitDist = raycastHit.distance;
                    leftSlope = (hitDist[0] - outHitDist) / (SkinWidth * Vector3.Dot(_boxright, Vector3.right));
                    rightSlope = -5;
                }
            }
            if (hitDist[2] != -1)
            {
                rayVector = (Vector2)_raycastBottomRight + SkinWidth * (Vector2)_boxright;

                raycastHit = Physics2D.Raycast(rayVector, -_boxup, rayDistance, PlatformMask);
                if (raycastHit)
                {
                    float outHitDist = raycastHit.distance;
                    rightSlope = (outHitDist - hitDist[2]) / (SkinWidth * Vector3.Dot(_boxright, Vector3.right));
                    leftSlope = -5;
                }
            }
        }

        for (int i = 0; i < _possibleSlopes.Length; i++)
        {
            if (System.Math.Abs(leftSlope - _possibleSlopes[i]) < 0.05f )
            {
                _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, leftSlope, 0));
            }
            if (System.Math.Abs(rightSlope - _possibleSlopes[i]) < 0.05f)
            {
                _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, rightSlope, 0));
            }
        }
    }

    private void CalculateRayOrigins()
    {
        var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
        var center = new Vector2(_boxCollider.offset.x * _localScale.x, _boxCollider.offset.y * _localScale.y);

        _raycastTopLeft = _transform.position + Vector3.Dot(new Vector3(center.x - size.x + SkinWidth, 0), Vector3.right) * (Vector3)_boxright + Vector3.Dot(new Vector3(0, center.y + size.y - SkinWidth), Vector3.right) * (Vector3)_boxup;
        _raycastBottomRight = _transform.position + Vector3.Dot(new Vector3(center.x + size.x - SkinWidth, 0), Vector3.right) * (Vector3)_boxright + Vector3.Dot(new Vector3(0, center.y - size.y + SkinWidth), Vector3.right) * (Vector3)_boxup;
        _raycastBottomLeft = _transform.position + Vector3.Dot(new Vector3(center.x - size.x + SkinWidth, 0), Vector3.right) * (Vector3)_boxright + Vector3.Dot(new Vector3(0, center.y - size.y + SkinWidth), Vector3.right) * (Vector3)_boxup;
    }

}
