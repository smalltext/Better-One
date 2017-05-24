﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class Parameters : MonoBehaviour {

    public Vector2 MaxVelocity = new Vector2(float.MaxValue, float.MaxValue);

    [Range(0, 90)]
    public float SlopeLimit = 30;

    public float Gravity = -25f;

    public float SpeedAccelerationOnGround = 10f;
    public float SpeedAccelerationInAir = 5f;
}
