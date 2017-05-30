using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class Parameters : MonoBehaviour {

    public Vector2 MaxVelocity = new Vector2(float.MaxValue, float.MaxValue);

    public float Gravity = -.5f;
    public float FrictionOnGround = -.15f;
    public float FrictionInAir = -.1f;
    public float HangTime = 5;
    public float ThumpAccel = 1;

    public float SpeedAccelerationOnGround = 1.5f;
    public float SpeedAccelerationInAir = 1f;
}
