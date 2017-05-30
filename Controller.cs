using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private const float SkinWidth = .2f;
    private const int TotalHorizontalRays = 4;
    private const int TotalVerticalRays = 4;

    public LayerMask PlatformMask;

    public Parameters _parameters;
    public ControllerState _state { get; private set; }
    public Vector2 Velocity { get { return _velocity; } }

    private Vector2 _velocity;
    private Transform _transform; // position
    private Vector3 _localScale;    //scale
    private BoxCollider2D _boxCollider; //collider

    private Vector2 _boxright;
    private Vector2 _boxup;

    private Vector2 _controllervelocity;
    private Vector2 _rotateto;
    private Vector3 _rotationpoint;
    private bool _rotating;

    private Vector3
       _raycastTopLeft,
       _raycastTopRight,
       _raycastBottomRight,
       _raycastBottomLeft;

    private float   //the distance between the rays on each side, calculated in Awake
       _verticalDistanceBetweenRays,
       _horizontalDistanceBetweenRays;
    private Vector2[] _possibleNormals = { new Vector2(0, 1), new Vector2(1, 1), new Vector2(.5f, 1), new Vector2(-1, 1), new Vector2(.5f, -1) };

    void Awake()
    {
        _state = new ControllerState();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = this.GetComponent<BoxCollider2D>();
        UpdateBox();
        _rotateto = _boxup;
        _rotationpoint = new Vector3(0, 0, 0);
        _rotating = false;

        _velocity = new Vector2(0, 0);
        _controllervelocity = new Vector2(0, 0);

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
        UpdateBox();
        CalculateRayOrigins();

        _velocity += _controllervelocity;
        if (_state.HasGravity)
        {
            _velocity.y += _parameters.Gravity;
            RotateUpdate();
        }
        if (_state.IsGrounded )
        {
            _velocity += Vector2.Dot(_velocity, _boxright) * _parameters.FrictionOnGround * _boxright;
        } else
        {
            _velocity += Vector2.Dot(_velocity, _boxright) * _parameters.FrictionInAir * _boxright;
        }
        if (_rotating)
            Rotate();

        _velocity.x = Mathf.Min(_velocity.x, _parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, _parameters.MaxVelocity.y);

        _state.Reset();
        XBlock(_velocity * Time.deltaTime);
        YBlock(_velocity * Time.deltaTime);

        _transform.Translate(Velocity * Time.deltaTime, Space.World);
    }

    private void UpdateBox()
    {
        _boxup = transform.up;
        _boxright = Vector3.Cross(_boxup, Vector3.forward);
    }


    private void CalculateRayOrigins()
    {
        var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
        var center = new Vector2(_boxCollider.offset.x * _localScale.x, _boxCollider.offset.y * _localScale.y);

        _raycastTopLeft = _transform.position + (center.x - size.x + SkinWidth) * (Vector3)_boxright + (center.y + size.y - SkinWidth) * (Vector3)_boxup;
        _raycastTopRight = _transform.position + (center.x + size.x - SkinWidth) * (Vector3)_boxright + (center.y + size.y - SkinWidth) * (Vector3)_boxup;
        _raycastBottomRight = _transform.position + (center.x + size.x - SkinWidth) * (Vector3)_boxright + (center.y - size.y + SkinWidth) * (Vector3)_boxup;
        _raycastBottomLeft = _transform.position + (center.x - size.x + SkinWidth) * (Vector3)_boxright + (center.y - size.y + SkinWidth) * (Vector3)_boxup;
    }

    private void XBlock(Vector2 deltaMovement)
    {
        var isGoingRight = Vector2.Dot(deltaMovement, _boxright) > 0;
        var rayDistance = Mathf.Abs(Vector2.Dot(deltaMovement, _boxright)) + SkinWidth;
        var rayDirection = isGoingRight ? _boxright : -_boxright;
        var rayOrigin = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;
        float allowedMovement = 100;
        bool caught = false;
        Vector2 normalDirection = new Vector2(0, 0);

        for (var i = 0; i < TotalHorizontalRays; i++)
        {
            var rayVector = (Vector2)rayOrigin + i * _verticalDistanceBetweenRays * (Vector2)_boxup; //gives a vector to the origin of the ray cast
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);    //draws a visual representation of the raycast

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask); //returns a boolean as to whether the ray is hitting anything in a certain platform mask (level)
            if (!rayCastHit)    //if it does not hit anything, break and check next ray
                continue;

            allowedMovement = Mathf.Min(allowedMovement, rayCastHit.distance - SkinWidth);     //sets the right movement to the furthest that can be gone without hitting
            rayDistance = rayCastHit.distance;
            caught = true;

            normalDirection = rayCastHit.normal;

            if (rayDistance < SkinWidth + 0.0001f)  //catch, if it's inside a wall
                break;
        }

        if (caught)
        {
            _velocity -= Vector2.Dot(_velocity, normalDirection.normalized) * normalDirection.normalized;
            if (isGoingRight)
            {
                _transform.Translate(allowedMovement * _boxright, Space.World);
            }
            else
            {
                _transform.Translate(allowedMovement * -_boxright, Space.World);
            }
        }

    }

    private void YBlock(Vector2 deltaMovement)
    {
        var isGoingUp = Vector2.Dot(deltaMovement, _boxup) > 0;
        var rayDistance = Mathf.Abs(Vector2.Dot(deltaMovement, _boxup)) + SkinWidth;
        var rayDirection = isGoingUp ? _boxup : -_boxup;
        var rayOrigin = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;
        float allowedMovement = 100;
        bool caught = false;
        Vector2 normalDirection = new Vector2(0, 0);

        for (var i = 0; i < TotalVerticalRays; i++)
        {
            var rayVector = (Vector2)rayOrigin + i * _horizontalDistanceBetweenRays * (Vector2)_boxright; //gives a vector to the origin of the ray cast
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);    //draws a visual representation of the raycast

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask); //returns a boolean as to whether the ray is hitting anything in a certain platform mask (level)
            if (!rayCastHit)    //if it does not hit anything, break and check next ray
                continue;

            allowedMovement = Mathf.Min(allowedMovement, rayCastHit.distance);     //sets the right movement to the furthest that can be gone without hitting

            caught = true;

            normalDirection = rayCastHit.normal;

            if (rayDistance < SkinWidth + 0.0001f)  //catch, if it's inside a wall
                break;
        }

        if (caught)
        {
            allowedMovement -= SkinWidth;
            if (!isGoingUp)
                _state.IsCollidingBelow = true;
            _velocity -= Vector2.Dot(_velocity, normalDirection.normalized) * normalDirection.normalized;

            if (isGoingUp)
            {
                _transform.Translate(allowedMovement * _boxup, Space.World);
            }
            else
            {
                _transform.Translate(allowedMovement * -_boxup, Space.World);
            }
        }
    }

    private void RotateUpdate()
    {
        var raycastHit1 = Physics2D.Raycast(_transform.position - 0.2f * (Vector3)_boxright, -_boxup, 1.5f, PlatformMask);
        var raycastHit2 = Physics2D.Raycast(_transform.position + 0.2f * (Vector3)_boxright, -_boxup, 1.5f, PlatformMask);

        if (raycastHit1 && raycastHit2 && raycastHit1.normal == raycastHit2.normal && raycastHit1.normal != _rotateto)
        {
            for (int i = 0; i < _possibleNormals.Length; i++)
            {
                if (Vector2.Angle(raycastHit1.normal, _possibleNormals[i]) < 5)
                {
                    _rotateto = _possibleNormals[i];
                    _rotating = true;
                }
            }
        }
        if (!raycastHit1 && !raycastHit2)
        {
            _rotateto = new Vector2(0, 1);
            _rotating = true;
        }
    }

    private bool IsGoingRight()
    {
        return Vector2.Dot(_velocity, (Vector2)_boxright) > 0;
    }
    private bool IsGoingLeft()
    {
        return Vector2.Dot(_velocity, (Vector2)_boxright) < 0;
    }

    private void Rotate()
    {
        bool rotateit = false;
        var rayCastHit1 = Physics2D.Raycast(_raycastBottomLeft - (SkinWidth) * (Vector3)_boxup, _boxright, 1.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x), PlatformMask);
        var rayCastHit2 = Physics2D.Raycast(_raycastBottomRight - (SkinWidth) * (Vector3)_boxup, -_boxright, 1.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x), PlatformMask);
        Debug.DrawRay(_raycastBottomLeft - (SkinWidth) * (Vector3)_boxup, _boxright * 1.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x));
        if (rayCastHit1 && rayCastHit1.distance != 0)
        {
            _rotationpoint = (Vector3)rayCastHit1.point;
            rotateit = true;
        }
        else if (rayCastHit2 && rayCastHit2.distance != 0)
        {
            _rotationpoint = (Vector3)rayCastHit2.point;
            rotateit = true;
        }
        if (!rayCastHit1 && !rayCastHit2)
        {
            _rotationpoint = _transform.position;
            rotateit = true;
        }

        float ang = Vector2.Angle(_boxup, _rotateto);
        Vector3 cross = Vector3.Cross(_boxup, _rotateto);

        if (cross.z > 0)
            ang = 360 - ang;

        if (rotateit)
        {
            _transform.RotateAround(_rotationpoint, Vector3.forward, -ang);
            _rotating = false;
        }

    }
	
}
