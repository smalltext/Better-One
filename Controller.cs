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
       _raycastTopRight,
       _raycastBottomRight,
       _raycastBottomLeft;

    private float   //the distance between the rays on each side, calculated in Awake
       _verticalDistanceBetweenRays,
       _horizontalDistanceBetweenRays;

    private bool
        _collidingleft,
        _collidingright;

    private float[] _possibleSlopes = { -1, -0.5f, 0, 0.5f, 1 };

    void Awake()
    {
        HandleCollisions = true;
        _state = new ControllerState();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = this.GetComponent<BoxCollider2D>();

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

        _velocity = _controllervelocity;
        _velocity.y += _parameters.Gravity;

        RotateUpdate();
        /*
        if ((!IsGoingRight() && !_collidingleft) || (IsGoingRight() && _collidingright))
            _transform.RotateAround(_raycastBottomLeft-SkinWidth*(Vector3)_boxright-SkinWidth*(Vector3)_boxup, Vector3.forward, -Vector3.Angle(_rotateto, _boxup));
        else
            _transform.RotateAround(_raycastBottomRight+SkinWidth*(Vector3)_boxright-SkinWidth*(Vector3)_boxup, Vector3.forward, -Vector3.Angle(_rotateto, _boxup));
        */
        //_transform.Rotate(new Vector3(0,0,-Vector3.Angle(_rotateto,_boxup)), Space.World);


        _collidingleft = _collidingright = false;
        if (Vector2.Dot(_velocity, _boxright) != 0)
            XBlock(_velocity*Time.deltaTime);
        YBlock(_velocity*Time.deltaTime);

        _velocity.x = Mathf.Min(_velocity.x, _parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, _parameters.MaxVelocity.y);
        _transform.Translate(Velocity*Time.deltaTime, Space.World);
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
        var isGoingRight = Vector2.Dot(deltaMovement,_boxright) > 0;
        var rayDistance = Mathf.Abs(Vector2.Dot(deltaMovement, _boxright)) + SkinWidth;
        var rayDirection = isGoingRight ? _boxright : -_boxright;
        var rayOrigin = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;
        float allowedMovement = 0;
        bool caught = false;

        for (var i = 0; i < TotalHorizontalRays; i++)
        {
            var rayVector = (Vector2)rayOrigin + i*_verticalDistanceBetweenRays*(Vector2)_boxup; //gives a vector to the origin of the ray cast
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);    //draws a visual representation of the raycast

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask); //returns a boolean as to whether the ray is hitting anything in a certain platform mask (level)
            if (!rayCastHit)    //if it does not hit anything, break and check next ray
                continue;

            allowedMovement = Mathf.Min(allowedMovement, rayCastHit.distance);     //sets the right movement to the furthest that can be gone without hitting
            caught = true;

            if (rayDistance < SkinWidth + 0.0001f)  //catch, if it's inside a wall
                break;
        }

        if (caught)
        {
            allowedMovement -= SkinWidth;
            _velocity = _velocity - Vector2.Dot(_velocity, _boxright) * (Vector2)_boxright;
            if (isGoingRight)
            {
                _velocity += allowedMovement / Time.deltaTime * _boxright;
                _collidingright = true;
            }
            else
            {
                _velocity -= allowedMovement / Time.deltaTime * _boxright;
                _collidingleft = true;
            }
        }

    }

    private void YBlock(Vector2 deltaMovement)
    {
        var isGoingUp = Vector2.Dot(deltaMovement, _boxup) > 0;
        var rayDistance = Mathf.Abs(Vector2.Dot(deltaMovement, _boxup)) + SkinWidth;
        var rayDirection = isGoingUp ? _boxup : -_boxup;
        var rayOrigin = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;
        float allowedMovement = 0;
        bool caught = false;

        for (var i = 0; i < TotalVerticalRays; i++)
        {
            var rayVector = (Vector2)rayOrigin + i * _horizontalDistanceBetweenRays * (Vector2)_boxright; //gives a vector to the origin of the ray cast
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);    //draws a visual representation of the raycast

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask); //returns a boolean as to whether the ray is hitting anything in a certain platform mask (level)
            if (!rayCastHit)    //if it does not hit anything, break and check next ray
                continue;

            allowedMovement = Mathf.Min(allowedMovement, rayCastHit.distance);     //sets the right movement to the furthest that can be gone without hitting

            caught = true;

            if (rayDistance < SkinWidth + 0.0001f)  //catch, if it's inside a wall
                break;
        }

        if (caught)
        {
            allowedMovement -= SkinWidth;
            _velocity = _velocity - Vector2.Dot(_velocity, _boxup) * (Vector2)_boxup;

            if (isGoingUp)
            {
                _velocity += allowedMovement / Time.deltaTime * _boxup;
            }
            else
            {
                _velocity -= allowedMovement / Time.deltaTime * _boxup;
            }
        }
    }

    private void RotateUpdate()
    {
        var rayDistance = 2.25f * _boxCollider.size.y * Mathf.Abs(_localScale.y);
        float[] hitDist = { -1, -1, -1};

        var rayVector = (Vector2)_raycastBottomLeft;
        var raycastHit = Physics2D.Raycast(rayVector, _boxup, rayDistance, PlatformMask);
        
        for (var i = 0; i < 3; i++)
        {
            rayVector = (Vector2)_transform.position - 0.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x) * _boxright + i * 0.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x) * _boxright;
            //rayVector = (Vector2)_transform.position + (i * .25f * _boxCollider.size.x * Mathf.Abs(_localScale.x) * _boxright.x) * _boxright ;
            //rayVector = (Vector2)_raycastTopLeft + .1f*Vector2.right  + (i * .2f * _boxCollider.size.x * Mathf.Abs(_localScale.x)*_boxright.x) * Vector2.right;

            raycastHit = Physics2D.Raycast(rayVector, Vector3.down, rayDistance, PlatformMask);
            if (raycastHit)
            {
                hitDist[i] = -raycastHit.distance;
            }
        }

        /*
        rayVector = (Vector2)_raycastBottomLeft;
        raycastHit = Physics2D.Raycast(rayVector, Vector3.down, rayDistance, PlatformMask);
        if (raycastHit)
        {
            hitDist[0] = 1-raycastHit.distance;
        }
        rayVector = (Vector2)_raycastBottomRight;
        raycastHit = Physics2D.Raycast(rayVector, Vector3.up, rayDistance, PlatformMask);
        if (raycastHit)
        {
            hitDist[2] = raycastHit.distance;
        }
        */

        float leftSlope = (hitDist[1] - hitDist[0]) / (.5f* _boxCollider.size.x * Mathf.Abs(_localScale.x));
        float rightSlope = (hitDist[2] - hitDist[1]) / ( .5f*_boxCollider.size.x * Mathf.Abs(_localScale.x));

        /*
        if (hitDist[1] == -1 && (hitDist[0] != -1 || hitDist[2] != -1))
        {
            if (hitDist[0] != -1)
            {
                rayVector = (Vector2)_raycastBottomLeft - SkinWidth * (Vector2)_boxright;

                raycastHit = Physics2D.Raycast(rayVector, Vector3.down, 1, PlatformMask);
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

                raycastHit = Physics2D.Raycast(rayVector, Vector3.down, rayDistance, PlatformMask);
                if (raycastHit)
                {
                    float outHitDist = raycastHit.distance;
                    rightSlope = (outHitDist - hitDist[2]) / (SkinWidth * Vector3.Dot(_boxright, Vector3.right));
                    leftSlope = -5;
                }
            }
        }
        */

        //Debug.Log(leftSlope);
       // Debug.Log(rightSlope);

        for (int i = 0; i < _possibleSlopes.Length; i++)
        {
            if (System.Math.Abs(leftSlope - _possibleSlopes[i] + (_boxup.x/_boxup.y)) < 0.01f && IsGoingLeft())
            {
                _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, _possibleSlopes[i], 0));
            }
            if (System.Math.Abs(rightSlope - _possibleSlopes[i] + (_boxup.x/_boxup.y)) < 0.01f && IsGoingRight())
            {
                _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, _possibleSlopes[i], 0));
            }
        }

        //Debug.Log(_rotateto);
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
        Vector3 rotationPoint = new Vector3(0,0,0);
        bool rotateit = false;
        var rayCastHit = Physics2D.Raycast(_raycastBottomLeft, _boxright, _boxCollider.size.x * Mathf.Abs(_localScale.x), PlatformMask);
        if (rayCastHit.distance != 0)
        {
            rotationPoint = (Vector3)rayCastHit.point;
            rotateit = true;
        }
        rayCastHit = Physics2D.Raycast(_raycastBottomRight, -_boxright, 1.5f*_boxCollider.size.x * Mathf.Abs(_localScale.x), PlatformMask);
        if (rayCastHit)
        {
            Debug.Log("hit");
        }
        if (rayCastHit.distance != 0)
        {
            rotationPoint = (Vector3)rayCastHit.point;
            rotateit = true;
        }

        Debug.Log(rotateit);

        if (rotateit)
        {
            _transform.Rotate(new Vector3(rotationPoint.x,rotationPoint.y, -Vector3.Angle(_rotateto, _boxup)),Space.World);
        }

    }
}
