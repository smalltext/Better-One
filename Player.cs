using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    CameraMovement _cameramovement;

    private Parameters _parameters;

    private bool _isFacingRight;
    private Controller _controller;
    private float _normalizedHorizonalSpeed;

    void Start () {
        _controller = GetComponent<Controller>();
        _parameters = GetComponent<Parameters>();
        _isFacingRight = transform.localScale.x > 0;    //checks sprite orientation to determine bool
    }
	
	// Update is called once per frame
	void Update () {
        HandleInput();

        var movementFactor = _controller._state.IsGrounded ? _parameters.SpeedAccelerationOnGround : _parameters.SpeedAccelerationInAir;
        //_controller.SetHorizontalForce(Mathf.Lerp(_controller.Velocity.x, _normalizedHorizonalSpeed * _parameters.MaxVelocity.x, Time.deltaTime * movementFactor));
        _controller.SetHorizontalForce(_normalizedHorizonalSpeed*movementFactor);

        //_cameramovement.UpdatePosition();
    }

    public void HandleInput() //self explanatory
    {
        if (Input.GetKey(KeyCode.D))
        {
            _normalizedHorizonalSpeed = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _normalizedHorizonalSpeed = -1;
        }
        else
        {
            _normalizedHorizonalSpeed = 0;
        }
    }

}
