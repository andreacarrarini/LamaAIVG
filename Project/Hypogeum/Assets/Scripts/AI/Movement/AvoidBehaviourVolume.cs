using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourVolume : MovementBehaviour {

    [Range( 0f, 1000f )] public float sightRange;
	public float sightAngle, steer, backpedal;

    // Used to adapt sight range
    public float actualSpeed;

    //public float baseSightRange = 20f;


	public override Vector3 GetAcceleration (MovementStatus status) {

        //sightRange = baseSightRange + actualSpeed * 0.5f;
        sightRange = actualSpeed;

		Collider collider = GetComponentInChildren<Collider>();

        bool leftHit = Physics.Raycast( transform.position,
                                        Quaternion.Euler( 0f, -sightAngle, 0f ) * status.movementDirection,
                                        sightRange );

        bool centerHit = Physics.Raycast( transform.position,
                                          status.movementDirection,
                                          sightRange );

        bool rightHit = Physics.Raycast( transform.position,
                                         Quaternion.Euler( 0f, sightAngle, 0f ) * status.movementDirection,
                                         sightRange );

        Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.movementDirection.normalized;

		if (leftHit && !centerHit && !rightHit) {
			return right * steer;
		} else if (leftHit && centerHit && !rightHit) {
			return right * steer * 2f;
		} else if (leftHit && centerHit && rightHit) {
			return -status.movementDirection.normalized * backpedal;
		} else if (!leftHit && centerHit && rightHit) {
			return -right * steer * 2f;
		} else if (!leftHit && !centerHit && rightHit) {
			return -right * steer;
		} else if (!leftHit && centerHit && !rightHit) {
			return right * steer;
		}

		return Vector3.zero;
	}
}
