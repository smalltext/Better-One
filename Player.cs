using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    CameraMovement _cameramovement;

    private Parameters _parameters;
    public AudioSource _audio;
    public UIDisplayer _uidisplayer;

    private Controller _controller;
    private float _normalizedHorizonalSpeed;
    private float _starttime;
    private Vector2 _lastvelocity;

    void Start () {
        _controller = GetComponent<Controller>();
        _parameters = GetComponent<Parameters>();
        _starttime = 0;
        _lastvelocity = _controller.Velocity;
    }
	
	// Update is called once per frame
	void Update () {

        HandleInput();

        var movementFactor = _controller._state.IsGrounded ? _parameters.SpeedAccelerationOnGround : _parameters.SpeedAccelerationInAir;
        _controller.SetHorizontalForce(_normalizedHorizonalSpeed*movementFactor);

        PlayThump(_normalizedHorizonalSpeed * movementFactor);
        //_cameramovement.UpdatePosition();

        _uidisplayer.DisplayTime(Time.time - _starttime);
    }

    public void HandleInput() //self explanatory
    {
        if (_controller._state.IsGrounded)
        {
            _starttime = Time.time;
        }

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

    public void PlayThump(float controllerspeed)
    {
        if ((Mathf.Abs(_controller.Velocity.x) < 0.01f && Mathf.Abs(_lastvelocity.x) > _parameters.ThumpAccel) || (Mathf.Abs(_controller.Velocity.y) < 0.01f && Mathf.Abs(_lastvelocity.y) > _parameters.ThumpAccel)) {
            _audio.Play();
        }
        _lastvelocity = _controller.Velocity;
    }

}
