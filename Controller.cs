using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private const float SkinWidth = .02f;
    private const int TotalHorizontalRays = 4;
    private const int TotalVerticalRays = 4;

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
    private Vector3 _rotationpoint = new Vector3(0, 0, 0);
    private bool _rotating = false;

    private Vector3
       _raycastTopLeft,
       _raycastTopRight,
       _raycastBottomRight,
       _raycastBottomLeft;

    private float   //the distance between the rays on each side, calculated in Awake
       _verticalDistanceBetweenRays,
       _horizontalDistanceBetweenRays;

    private bool lastRight = false;
    private float lastSlope = 0;

    private bool
        _passleft,
        _passright;

    private float[] _possibleSlopes = { -1, -0.5f, 0, 0.5f, 1 };

    void Awake()
    {
        HandleCollisions = true;
        _state = new ControllerState();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = this.GetComponent<BoxCollider2D>();
        UpdateBox();
        _rotateto = _boxup;

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
        _velocity.y += _parameters.Gravity;
        if (_state.IsGrounded)
        {
            _velocity = Vector2.Dot(_velocity,_boxup) * _boxup + Vector2.Dot(_velocity, _boxright) * _parameters.Friction * _boxright;
        }

        RotateUpdate();
        /*
        if ((!IsGoingRight() && !_collidingleft) || (IsGoingRight() && _collidingright))
            _transform.RotateAround(_raycastBottomLeft-SkinWidth*(Vector3)_boxright-SkinWidth*(Vector3)_boxup, Vector3.forward, -Vector3.Angle(_rotateto, _boxup));
        else
            _transform.RotateAround(_raycastBottomRight+SkinWidth*(Vector3)_boxright-SkinWidth*(Vector3)_boxup, Vector3.forward, -Vector3.Angle(_rotateto, _boxup));
        */
        //_transform.Rotate(new Vector3(0,0,-Vector3.Angle(_rotateto,_boxup)), Space.World);
        if (_rotating)
            Rotate();

        _velocity.x = Mathf.Min(_velocity.x, _parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, _parameters.MaxVelocity.y);

        _state.Reset();
        XBlock(_velocity * Time.deltaTime);
        YBlock(_velocity * Time.deltaTime);

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
        float allowedMovement = 100;
        float greatestDistance = 0;
        bool caught = false;

        for (var i = 0; i < TotalHorizontalRays; i++)
        {
            var rayVector = (Vector2)rayOrigin + i*_verticalDistanceBetweenRays*(Vector2)_boxup; //gives a vector to the origin of the ray cast
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);    //draws a visual representation of the raycast

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask); //returns a boolean as to whether the ray is hitting anything in a certain platform mask (level)
            if (!rayCastHit)    //if it does not hit anything, break and check next ray
                continue;

            allowedMovement = Mathf.Min(allowedMovement, rayCastHit.distance-SkinWidth);     //sets the right movement to the furthest that can be gone without hitting
            greatestDistance = Mathf.Max(greatestDistance, rayCastHit.distance);
            caught = true;

            if (rayDistance < SkinWidth + 0.0001f)  //catch, if it's inside a wall
                break;
        }

        if (caught)
        {
            if (isGoingRight)
                _state.IsCollidingRight = true;
            else
                _state.IsCollidingLeft = true;
            _velocity -= Vector2.Dot(_velocity, _boxright) * (Vector2)_boxright;
            if (isGoingRight)
            {
                _velocity += allowedMovement / Time.deltaTime * _boxright;
            }
            else
            {
                _velocity -= allowedMovement / Time.deltaTime * _boxright;
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
            if ( isGoingUp )
                _state.IsCollidingAbove = true;
            else
                _state.IsCollidingBelow = true;
            _velocity -= Vector2.Dot(_velocity, _boxup) * (Vector2)_boxup;

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
        var rayDistance = 1.75f * _boxCollider.size.y * Mathf.Abs(_localScale.y);
        float[] hitDist = { 1, 1, 1};
        
        for (var i = 0; i < 3; i++)
        {
            if (i != 1)
            {
                var rayVector = (Vector2)_transform.position + (i * 0.5f - 0.5f) * _boxright;
                //rayVector = (Vector2)_transform.position + (i * .25f * _boxCollider.size.x * Mathf.Abs(_localScale.x) * _boxright.x) * _boxright ;
                //rayVector = (Vector2)_raycastTopLeft + .1f*Vector2.right  + (i * .2f * _boxCollider.size.x * Mathf.Abs(_localScale.x)*_boxright.x) * Vector2.right;
                Debug.DrawRay(rayVector, -_boxup * rayDistance, Color.red);

                var raycastHit = Physics2D.Raycast(rayVector, -_boxup, rayDistance, PlatformMask);
                if (raycastHit)
                {
                    hitDist[i] = -raycastHit.distance;
                }
            } else
            {
                var rayVector1 = (Vector2)_transform.position - 0.05f * _boxright;
                var rayVector2 = (Vector2)_transform.position + 0.05f * _boxright;
                Debug.DrawRay(rayVector1, -_boxup * rayDistance, Color.red);
                Debug.DrawRay(rayVector2, -_boxup * rayDistance, Color.red);

                var raycastHit1 = Physics2D.Raycast(rayVector1, -_boxup, rayDistance, PlatformMask);
                var raycastHit2 = Physics2D.Raycast(rayVector2, -_boxup, rayDistance, PlatformMask);
                if (raycastHit1 || raycastHit2)
                {
                    if (Mathf.Abs(raycastHit1.distance - raycastHit2.distance) > 0.2f)
                    {
                        hitDist[i] = -Mathf.Min(raycastHit1.distance, raycastHit2.distance);
                    }
                    else
                    {
                        hitDist[i] = -(raycastHit1.distance + raycastHit2.distance) / 2;
                    }
                }
            }
        }

        if (Mathf.Abs(hitDist[1] + 0.5f) > 0.05f)
        {
            //Debug.Log(hitDist[0] + " " + hitDist[1] + " " + hitDist[2]);
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

        var leftRayVector = _raycastTopLeft - (0.5f+SkinWidth) * (Vector3)_boxright;
        var leftRaycastHit = Physics2D.Raycast(leftRayVector, -_boxup, 0.9f, PlatformMask);
        var rightRayVector = _raycastTopRight + (0.5f+SkinWidth) * (Vector3)_boxright;
        var rightRaycastHit = Physics2D.Raycast(rightRayVector, -_boxup, 0.9f, PlatformMask);

        Debug.DrawRay(leftRayVector, -_boxup * 0.9f, Color.red);
        Debug.DrawRay(rightRayVector, -_boxup * 0.9f, Color.red);
        //if (leftRaycastHit && leftRaycastHit.distance != 0)
        if (false)
        {

            float leftSlope = -Mathf.Tan(Mathf.Atan(_boxup.x / _boxup.y) + Mathf.Atan((hitDist[1] - (-leftRaycastHit.distance + 0.5f)) / (-1)));
            
            for (int i = 0; i<_possibleSlopes.Length; i++)
            {
                if (Mathf.Abs(leftSlope - _possibleSlopes[i]) < 0.1f)
                {
                    lastSlope = _possibleSlopes[i];
                }
            }

            _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, leftSlope, 0));
            _rotating = true;
        } else if (false)
        //if (rightRaycastHit && rightRaycastHit.distance != 0)
        {

            float rightSlope = -Mathf.Tan(Mathf.Atan(_boxup.x / _boxup.y) + Mathf.Atan((-rightRaycastHit.distance + 0.5f) - hitDist[1]) / (-1));

            for (int i = 0; i < _possibleSlopes.Length; i++)
            {
                if (Mathf.Abs(rightSlope - _possibleSlopes[i]) < 0.1f)
                {
                    lastSlope = _possibleSlopes[i];
                }
            }

            _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, rightSlope, 0));
            _rotating = true;
        } else
        {
            float leftSlope = -Mathf.Tan(Mathf.Atan(_boxup.x / _boxup.y) + Mathf.Atan((hitDist[1] - hitDist[0]) / (-.5f)));
            float rightSlope = -Mathf.Tan(Mathf.Atan(_boxup.x / _boxup.y) + Mathf.Atan((hitDist[2] - hitDist[1]) / (-.5f)));
            //Debug.Log(hitDist[0] + " " + hitDist[1] + " " + hitDist[2]);

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
            //Debug.Log(rightSlope);
            //Debug.Log(Mathf.Tan(Mathf.Atan(_boxup.x / _boxup.y) + Mathf.Atan(-leftSlope)));

            if ( Mathf.Abs(leftSlope - lastSlope) >= 0.05f && Mathf.Abs(rightSlope - lastSlope) >= 0.05f)
            {
                for (int i = 0; i < _possibleSlopes.Length; i++)
                {
                    if (Mathf.Abs(rightSlope - _possibleSlopes[i]) < 0.05f)
                    {
                        _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, _possibleSlopes[i], 0));
                        lastSlope = _possibleSlopes[i];
                        _rotating = true;
                    }
                    if (Mathf.Abs(leftSlope - _possibleSlopes[i]) < 0.05f)
                    {
                        _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, _possibleSlopes[i], 0));
                        lastSlope = _possibleSlopes[i];
                        _rotating = true;
                    }
                }
            }
            /*
            for (int i = 0; i < _possibleSlopes.Length; i++)
            {
                if ( Mathf.Abs(leftSlope + _possibleSlopes[i]) < 0.01f)
                {
                    _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, _possibleSlopes[i], 0));
                }
                if (Mathf.Abs(rightSlope + _possibleSlopes[i]) < 0.01f && IsGoingRight())
                {
                    _rotateto = Vector3.Cross(Vector3.forward, new Vector3(1, _possibleSlopes[i], 0));
                }
            }
            */
            //System.Math.Abs(leftSlope - _possibleSlopes[i] + (_boxup.x/_boxup.y))
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
        if (_state.IsGrounded)
        {
            var rayCastHit1 = Physics2D.Raycast(_raycastBottomLeft - (SkinWidth) * (Vector3)_boxup, _boxright, 1.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x), PlatformMask);
            var rayCastHit2 = Physics2D.Raycast(_raycastBottomRight - (SkinWidth) * (Vector3)_boxup, -_boxright, 1.5f * _boxCollider.size.x * Mathf.Abs(_localScale.x), PlatformMask);
            if (rayCastHit1 && rayCastHit1.distance != 0)
            {
                _rotationpoint = (Vector3)rayCastHit1.point;
                rotateit = true;
            }
            else if (rayCastHit2 && rayCastHit2.distance != 0)
            {
                _rotationpoint = (Vector3)rayCastHit2.point;
                rotateit = true;
            } else {
                if (_state.IsCollidingRight)
                {
                    _rotationpoint = _transform.position - 0.5f * (Vector3)_boxright;
                } else if (_state.IsCollidingLeft)
                {
                    _rotationpoint = _transform.position + 0.5f * (Vector3)_boxright;
                }
            }
        }
        //Debug.Log(Vector3.Distance(_transform.position,rotationPoint));
        Debug.Log(_rotationpoint);
        Debug.DrawLine(_transform.position, _rotationpoint);
        //Debug.Log(Vector3.Angle(_rotateto, _boxup));

        if (rotateit)
        {
            _transform.RotateAround(_rotationpoint, Vector3.forward, Mathf.Rad2Deg*(Mathf.Atan(_rotateto.y/_rotateto.x)-Mathf.Atan(_boxup.y/_boxup.x)));
            _rotating = false;
        }

    }
}
