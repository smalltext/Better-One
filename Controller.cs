using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private const float SkinWidth = .02f;
    private const int TotalHorizontalRays = 8;
    private const int TotalVerticalRays = 4;

    private static readonly float SlopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    public LayerMask PlatformMask;

    public Parameters _parameters;
    public ControllerState _state { get; private set; }
    public Vector2 Velocity { get { return _velocity; } }
    public bool HandleCollisions { get; set; }
    public GameObject StandingOn { get; private set; }

    private Vector2 _velocity;
    private Transform _transform; // position
    private Vector3 _localScale;    //scale
    private BoxCollider2D _boxCollider; //collider

    private Vector3
        _raycastTopLeft,
        _raycastBottomRight,
        _raycastBottomLeft;

    private float   //the distance between the rays on each side, calculated in Awake
        _verticalDistanceBetweenRays,
        _horizontalDistanceBetweenRays;

    void Awake () {
        HandleCollisions = true;
        _state = new ControllerState();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = GetComponent<BoxCollider2D>();

        var colliderWidth = _boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2 * SkinWidth);
        _horizontalDistanceBetweenRays = colliderWidth / (TotalVerticalRays - 1);

        var colliderHeight = _boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2 * SkinWidth);
        _verticalDistanceBetweenRays = colliderHeight / (TotalHorizontalRays - 1);
    }

    public void AddForce(Vector2 force)
    {
        _velocity = force;
    }

    public void SetForce(Vector2 force)
    {
        _velocity += force;
    }

    public void SetHorizontalForce(float x)
    {
        _velocity.x = x;
    }

    public void LateUpdate()
    {
        _velocity.y += _parameters.Gravity * Time.deltaTime;
        Move(Velocity * Time.deltaTime);
    }

    private void Move(Vector2 deltaMovement)
    {
        var wasGrounded = _state.IsCollidingBelow;
        _state.Reset();

        if (HandleCollisions)
        {
            CalculateRayOrigins();

            if (deltaMovement.y < 0 && wasGrounded)
                HandleVerticalSlope(ref deltaMovement);

            if (Mathf.Abs(deltaMovement.x) > .001f) //only collision checks if moving
                MoveHorizontally(ref deltaMovement);

            MoveVertically(ref deltaMovement);
        }

        _transform.Translate(deltaMovement, Space.World); //this is what actually moves the character

        if (Time.deltaTime > 0)
            _velocity = deltaMovement / Time.deltaTime; //deltaMovement is a parameter, inputted by LateUpdate (or other)

        _velocity.x = Mathf.Min(_velocity.x, _parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, _parameters.MaxVelocity.y); //clamping velocity to max if over (min chooses min value)

        if (_state.IsMovingUpSlope)
            _velocity.y = 0;
    }

    private void CalculateRayOrigins()
    {
        var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
        var center = new Vector2(_boxCollider.offset.x * _localScale.x, _boxCollider.offset.y * _localScale.y);

        _raycastTopLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y + size.y - SkinWidth);
        _raycastBottomRight = _transform.position + new Vector3(center.x + size.x - SkinWidth, center.y - size.y + SkinWidth);
        _raycastBottomLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y - size.y + SkinWidth);
    }

    private void MoveHorizontally(ref Vector2 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + SkinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        var rayOrigin = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;

        for (var i = 0; i < TotalHorizontalRays; i++)
        {
            var rayVector = new Vector2(rayOrigin.x, rayOrigin.y + (i * _verticalDistanceBetweenRays)); //gives a vector to the origin of the ray cast
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);    //draws a visual representation of the raycast

            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask); //returns a boolean as to whether the ray is hitting anything in a certain platform mask (level)
            if (!rayCastHit)    //if it does not hit anything, break and check next ray
                continue;

            if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(rayCastHit.normal, Vector2.up), isGoingRight))
                break;

            deltaMovement.x = rayCastHit.point.x - rayVector.x;     //sets the right movement to the furthest that can be gone without hitting
            rayDistance = Mathf.Abs(deltaMovement.x);   //handles stairs, constraining the raycast to the shortest distance found

            if (isGoingRight)
            {
                deltaMovement.x -= SkinWidth;
                _state.IsCollidingRight = true;
            }
            else
            {
                deltaMovement.x += SkinWidth;
                _state.IsCollidingLeft = true;
            }

            if (rayDistance < SkinWidth + 0.0001f)  //catch, if it's inside a wall
                break;
        }
    }

    private void MoveVertically(ref Vector2 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var rayOrigin = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;

        rayOrigin.x += deltaMovement.x; //since movevertically comes after movehorizontally

        var standingOnDistance = float.MaxValue;
        for (var i = 0; i < TotalVerticalRays; i++)
        {
            var rayVector = new Vector2(rayOrigin.x + (i * _horizontalDistanceBetweenRays), rayOrigin.y);
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            if (!raycastHit)
                continue;   //same as in horizontal

            if (!isGoingUp) //keeping track of what platform we're standing on
            {
                var verticalDistanceToHit = _transform.position.y - raycastHit.point.y;
                if (verticalDistanceToHit < standingOnDistance) //edge handling
                {
                    standingOnDistance = verticalDistanceToHit;
                    StandingOn = raycastHit.collider.gameObject;
                }
            }

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);   //same as before

            if (isGoingUp)
            {
                deltaMovement.y -= SkinWidth;
                _state.IsCollidingAbove = true;
            }
            else
            {
                deltaMovement.y += SkinWidth;
                _state.IsCollidingBelow = true;
            }

            if (!isGoingUp && deltaMovement.y > .0001f)
                _state.IsMovingUpSlope = true;

            if (rayDistance < SkinWidth + .0001f)   //catch
                break;
        }
    }

    private void HandleVerticalSlope(ref Vector2 deltaMovement)
    {

    }

    private bool HandleHorizontalSlope(ref Vector2 deltaMovement, float angle, bool isGoingRight)
    {
        return false;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {

    }

    public void OnTriggerExit2D(Collider2D other)
    {

    }

}
