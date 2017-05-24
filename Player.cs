using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    CameraMovement _cameramovement;

    Parameters _parameters;

    private bool _isFacingRight;
    private Controller _controller;
    private float _normalizedHorizonalSpeed;

    void Start () {
        _controller = GetComponent<Controller>();
        _isFacingRight = transform.localScale.x > 0;    //checks sprite orientation to determine bool
    }
	
	// Update is called once per frame
	void Update () {
        HandleInput();

        var movementFactor = _controller._state.IsGrounded ? _parameters.SpeedAccelerationOnGround : _parameters.SpeedAccelerationInAir;
        _controller.SetHorizontalForce(Mathf.Lerp(_controller.Velocity.x, _normalizedHorizonalSpeed * _parameters.MaxVelocity.x, Time.deltaTime * movementFactor));

        _cameramovement.UpdatePosition();
    }

    public void HandleInput() //self explanatory
    {
        if (Input.GetKey(KeyCode.D))
        {
            _normalizedHorizonalSpeed = 1;
            if (!_isFacingRight)
                Flip();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _normalizedHorizonalSpeed = -1;
            if (_isFacingRight)
                Flip();
        }
        else
        {
            _normalizedHorizonalSpeed = 0;
        }
    }

    private void Flip()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _isFacingRight = transform.localScale.x > 0;    //sets this again
    }
}
