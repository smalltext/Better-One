using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    CameraMovement _cameramovement;

    private Parameters _parameters;

    private Controller _controller;
    private float _normalizedHorizonalSpeed;
    private float _starttime;

    void Start () {
        _controller = GetComponent<Controller>();
        _parameters = GetComponent<Parameters>();
        _starttime = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (_controller._state.IsGrounded)
        {
            _starttime = Time.time;
        }

        HandleInput();

        var movementFactor = _controller._state.IsGrounded ? _parameters.SpeedAccelerationOnGround : _parameters.SpeedAccelerationInAir;
        _controller.SetHorizontalForce(_normalizedHorizonalSpeed*movementFactor);

        _cameramovement.UpdatePosition();
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
        if (Input.GetKey(KeyCode.Space) && Time.time - _starttime < _parameters.HangTime)
        {
            _controller._state.HasGravity = false;
        } else
        {
            _controller._state.HasGravity = true;
        }
    }

}
