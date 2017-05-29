 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerState {

    public bool IsCollidingBelow { get; set; }
    public bool HasGravity { get; set; }
    public bool IsGrounded { get { return IsCollidingBelow; } }

    public void Reset()
    {
        HasGravity =
        IsCollidingBelow = false;
    }

    public override string ToString()
    {
        return string.Format(
            "(controller: r:{0} l:{1} a:{2} b:{3} down-slope:{4} up-slope:{5} angle:{7})",
            IsCollidingBelow
        );
    }
}
